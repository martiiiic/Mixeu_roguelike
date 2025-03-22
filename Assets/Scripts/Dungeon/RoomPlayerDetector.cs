using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomPlayerDetector : MonoBehaviour
{
    [Header("Room Settings")]
    public GameObject Room;
    public TilemapRenderer RoomSprite;
    public int roomPriority = 0;

    [Header("Room Size Settings")]
    public bool isLargeRoom = false;
    public float roomZoomLevel = 5f;  // Default zoom level for this room
    [Tooltip("Enable to use custom zoom level instead of preset sizes")]
    public bool useCustomZoom = false;
    [Range(3f, 8f)]
    public float customZoomLevel = 5f;

    [Header("Camera Settings")]
    public bool RoomXfollow = false;
    public bool RoomYfollow = false;
    public float cameraTransitionSpeed = 10f;
    public float transitionduration = 0.2f;

    [Header("Room Visibility")]
    public static float roomLoadRadius = 10f;

    [Header("Trigger Offset")]
    [Tooltip("Enable to use custom offset for the player detection trigger")]
    public bool useCustomTriggerOffset = false;
    public Vector2 triggerOffset = Vector2.zero;

    private PlayerState player;
    private Camera mainCamera;
    private DynamicCamera dynamicCamera;
    private bool playerIsInRoom = false;
    private Collider2D roomTrigger;
    private float transitionLockTime = 0.05f;
    private bool isTransitionLocked = false;
    private static RoomPlayerDetector currentActiveRoom;
    private static List<RoomPlayerDetector> allRooms = new List<RoomPlayerDetector>();
    private Vector3 originalTriggerPosition;

    private void Awake()
    {
        if (!allRooms.Contains(this)) allRooms.Add(this);
        roomTrigger = GetComponent<Collider2D>();
        if (roomTrigger == null) Debug.LogError("No collider found on RoomPlayerDetector: " + gameObject.name);

        // Store original position
        originalTriggerPosition = transform.position;
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerState>();
        mainCamera = Camera.main;
        dynamicCamera = mainCamera.GetComponent<DynamicCamera>();
        if (dynamicCamera == null) Debug.LogError("No DynamicCamera component found on main camera!");

        if (RoomSprite == null) RoomSprite = transform.parent.gameObject.GetComponentInChildren<TilemapRenderer>();
        if (RoomSprite != null && !playerIsInRoom) RoomSprite.enabled = false;
        if (Room == null) Room = transform.parent.gameObject;

        // Set the appropriate zoom level based on room size
        if (useCustomZoom)
        {
            roomZoomLevel = customZoomLevel;
        }
        else
        {
            roomZoomLevel = isLargeRoom ? dynamicCamera.largeRoomZoom : dynamicCamera.normalZoom;
        }

        // Apply custom offset to trigger position if enabled
        ApplyTriggerOffset();
    }

    private void ApplyTriggerOffset()
    {
        if (useCustomTriggerOffset)
        {
            transform.position = new Vector3(
                originalTriggerPosition.x + triggerOffset.x,
                originalTriggerPosition.y + triggerOffset.y,
                originalTriggerPosition.z
            );
        }
        else
        {
            // Reset to original position if offset is disabled
            transform.position = originalTriggerPosition;
        }
    }

    private void Update()
    {
        if (player == null) { player = FindAnyObjectByType<PlayerState>(); if (player == null) return; }

        if (currentActiveRoom == this && playerIsInRoom && mainCamera != null && !isTransitionLocked)
        {
            Vector3 targetPosition = mainCamera.transform.position;
            if (RoomXfollow) targetPosition.x = player.transform.position.x;
            else if (RoomYfollow) targetPosition.y = player.transform.position.y;
            else targetPosition = new Vector3(Room.transform.position.x, Room.transform.position.y, -10);
            targetPosition.z = -10;

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPosition,
                Time.unscaledDeltaTime * cameraTransitionSpeed);
        }

        CheckRoomVisibility();
    }

    private void CheckRoomVisibility()
    {
        if (player == null || !player.gameObject.activeInHierarchy) return;
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= roomLoadRadius || playerIsInRoom) { if (RoomSprite != null && !RoomSprite.enabled) RoomSprite.enabled = true; }
        else { if (RoomSprite != null && RoomSprite.enabled) RoomSprite.enabled = false; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || !other.gameObject.activeInHierarchy) return;
        if (!other.CompareTag("Player") || isTransitionLocked) return;
        playerIsInRoom = true;
        if (RoomSprite != null) RoomSprite.enabled = true;
        if (currentActiveRoom == null || roomPriority >= currentActiveRoom.roomPriority) SetAsActiveRoom();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Player"))
        {
            playerIsInRoom = true;
            if ((currentActiveRoom == null || roomPriority > currentActiveRoom.roomPriority) && !isTransitionLocked) SetAsActiveRoom();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;
        if (other.CompareTag("Player"))
        {
            playerIsInRoom = false;
            if (player != null && player.gameObject.activeInHierarchy)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
                if (distanceToPlayer > roomLoadRadius && RoomSprite != null) RoomSprite.enabled = false;
            }
            else if (RoomSprite != null) RoomSprite.enabled = false;
            if (currentActiveRoom == this && !isTransitionLocked) FindNewActiveRoom();
        }
        else if (other.CompareTag("Collectible") && other.GetComponent<CollectibleCoin>()) Destroy(other.gameObject);
    }

    private void SetAsActiveRoom()
    {
        StartCoroutine(SlowTimeForTransition());

        currentActiveRoom = this;

        if (mainCamera != null && dynamicCamera != null)
        {
            // Set camera zoom level based on room size
            if (dynamicCamera.respectRoomTransitions)
            {
                dynamicCamera.SetRoomZoom(roomZoomLevel);
            }

            Vector3 targetPosition = new Vector3(
                Room.transform.position.x,
                Room.transform.position.y,
                -10);

            StartCoroutine(SmoothCameraTransition(targetPosition));
        }
    }

    private IEnumerator SmoothCameraTransition(Vector3 targetPosition)
    {
        isTransitionLocked = true;

        // Notify DynamicCamera that room is controlling the camera
        if (dynamicCamera != null && dynamicCamera.respectRoomTransitions)
        {
            // Store original camera position
            Vector3 startPosition = mainCamera.transform.position;
            float elapsedTime = 0f;
            float transitionDuration = transitionduration;

            while (elapsedTime < transitionDuration)
            {
                mainCamera.transform.position = Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    elapsedTime / transitionDuration);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            mainCamera.transform.position = targetPosition;
        }

        // Wait a short time to ensure the transition is complete
        yield return new WaitForSeconds(0.1f);
        isTransitionLocked = false;
    }

    private IEnumerator SlowTimeForTransition()
    {
        isTransitionLocked = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.01f;
        float realTimePassed = 0f;
        float maxFreezeTime = 0.25f;

        while (realTimePassed < 0.22f)
        {
            realTimePassed += Time.unscaledDeltaTime;
            if (realTimePassed >= maxFreezeTime) break;
            yield return null;
        }

        Time.timeScale = originalTimeScale;
        isTransitionLocked = false;
    }

    private IEnumerator LockTransitionForTime(float lockTime)
    {
        isTransitionLocked = true;
        yield return new WaitForSeconds(lockTime);
        isTransitionLocked = false;
    }

    private void FindNewActiveRoom()
    {
        RoomPlayerDetector highestPriorityRoom = null;
        foreach (RoomPlayerDetector room in allRooms)
        {
            if (room != null && room != this && room.playerIsInRoom)
            {
                if (highestPriorityRoom == null || room.roomPriority > highestPriorityRoom.roomPriority) highestPriorityRoom = room;
            }
        }
        if (highestPriorityRoom != null) highestPriorityRoom.SetAsActiveRoom();
        else
        {
            currentActiveRoom = null;
            // Reset to normal zoom when exiting all rooms
            if (dynamicCamera != null) dynamicCamera.ResetToNormalZoom();
        }
    }

    public static void CheckAllRoomsProximity()
    {
        foreach (RoomPlayerDetector room in allRooms) { if (room != null) room.CheckRoomVisibility(); }
    }

    // Method to update trigger offset at runtime if needed
    public void UpdateTriggerOffset(Vector2 newOffset)
    {
        triggerOffset = newOffset;
        useCustomTriggerOffset = true;
        ApplyTriggerOffset();
    }

    // Editor-only function that allows updating the position when values change in inspector
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyTriggerOffset();
        }
    }

    private void OnDestroy()
    {
        if (currentActiveRoom == this) currentActiveRoom = null;
        if (allRooms.Contains(this)) allRooms.Remove(this);
    }
}