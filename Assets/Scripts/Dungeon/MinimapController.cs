using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour
{
    public GameObject minimapIconPrefab;
    public GameObject storeIconPrefab;
    public GameObject playerIconPrefab;
    public GameObject bossIconPrefab;
    public GameObject questionMarkIconPrefab;
    public GameObject waypointIconPrefab;
    public GameObject entryRoom;
    public GameObject RunStats;
    public Transform minimapCanvas;
    public List<MinimapIcon> minimapIcons = new List<MinimapIcon>();

    private RoomTemplates roomTemplates;
    private DungeonInfo DI;

    public float baseIconSpacing = 5f;
    public GameObject minimapUI;

    private bool isMinimapVisible = false;
    private Transform playerTransform;
    private GameObject playerIcon;
    private GameObject storeIcon;
    private GameObject bossIcon;
    private GameObject questionIcon;
    private GameObject waypointIcon;

    private float zoomSpeed = 0.1f;
    private float minZoom = 0.5f;
    private float maxZoom = 2f;
    private float currentZoom = 1f;

    private Vector2 minimapOffset = Vector2.zero;
    private Vector2 lastMousePosition;
    private bool isDragging = false;

    private HashSet<Vector3> visitedRooms = new HashSet<Vector3>();
    private HashSet<Vector3> adjacentRooms = new HashSet<Vector3>();
    private bool storeDiscovered = false;
    private bool bossDiscovered = false;
    private bool cauldronDiscovered = false;

    private Vector3 lastPlayerRoundedPos;
    private float roomDiscoveryRadius = 5f;
    private bool minimapInitialized = false;

    private void Start()
    {
        minimapIcons.Clear();
        roomTemplates = FindObjectOfType<RoomTemplates>();
        playerTransform = FindObjectOfType<PlayerStats>().transform;

        if (playerTransform != null)
        {
            lastPlayerRoundedPos = new Vector3(
                Mathf.Round(playerTransform.position.x / 10f) * 10f,
                Mathf.Round(playerTransform.position.y / 10f) * 10f,
                0
            );

            Vector3 startingRoomPos = lastPlayerRoundedPos;
            visitedRooms.Add(startingRoomPos);
            AddAdjacentRooms(startingRoomPos);
        }

        minimapUI.SetActive(false);
    }

    public void AddAdjacentRoomsToEntryRoom()
    {
        Invoke("AddCurrentRoomToVisited", 2.5f);
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            AddCurrentRoomToVisited();
            CheckSpecialRoomDiscovery();
        }

        if (Time.timeScale == 0)
        {
            isMinimapVisible = false;
            minimapUI.SetActive(isMinimapVisible);
            RunStats.SetActive(isMinimapVisible);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMinimap();
        }

        if (isMinimapVisible)
        {
            if (playerIcon != null) UpdatePlayerIconPosition();

            if (storeDiscovered && storeIcon != null) UpdateStoreIconPosition();
            if (bossDiscovered && bossIcon != null) UpdateBossIconPosition();
            if (cauldronDiscovered && questionIcon != null) UpdateQuestionMarkIconPosition();

            minimapIcons.RemoveAll(icon => icon == null || icon.icon == null);

            if (Input.GetMouseButtonDown(0) && isMinimapVisible && !EventSystem.current.IsPointerOverGameObject())
            {
                CreateWaypoint();
            }
        }

        HandleZoom();
        HandleMouseDrag();
    }

    private void AddCurrentRoomToVisited()
    {
        if (playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 currentRoomPos = new Vector3(
            Mathf.Round(playerPos.x / 10f) * 10f,
            Mathf.Round(playerPos.y / 10f) * 10f,
            0
        );

        if (currentRoomPos != lastPlayerRoundedPos)
        {
            lastPlayerRoundedPos = currentRoomPos;

            foreach (var room in roomTemplates.rooms)
            {
                if (room != null)
                {
                    Vector3 roomPos = room.transform.position;
                    Vector3 roomGridPos = new Vector3(
                        Mathf.Round(roomPos.x / 10f) * 10f,
                        Mathf.Round(roomPos.y / 10f) * 10f,
                        0
                    );

                    if (roomGridPos == currentRoomPos && !visitedRooms.Contains(roomGridPos))
                    {
                        visitedRooms.Add(roomGridPos);
                        adjacentRooms.Remove(roomGridPos);
                        AddAdjacentRooms(roomGridPos);
                        UpdateIconVisibility();
                    }
                }
            }
        }
    }

    private void AddAdjacentRooms(Vector3 roomPos)
    {
        float gridSize = 10f;
        adjacentRooms.Add(new Vector3(roomPos.x + gridSize, roomPos.y, 0));
        adjacentRooms.Add(new Vector3(roomPos.x - gridSize, roomPos.y, 0));
        adjacentRooms.Add(new Vector3(roomPos.x, roomPos.y + gridSize, 0));
        adjacentRooms.Add(new Vector3(roomPos.x, roomPos.y - gridSize, 0));
    }

    private void CheckSpecialRoomDiscovery()
    {
        GameObject storeRoom = GameObject.FindGameObjectWithTag("Store");
        if (storeRoom != null && !storeDiscovered)
        {
            Vector3 storePos = new Vector3(
                Mathf.Round(storeRoom.transform.position.x / 10f) * 10f,
                Mathf.Round(storeRoom.transform.position.y / 10f) * 10f,
                0
            );

            if (visitedRooms.Contains(storePos) || adjacentRooms.Contains(storePos))
            {
                storeDiscovered = true;
                if (storeIcon == null && minimapInitialized)
                {
                    CreateStoreIcon();
                }
            }
        }

        GameObject bossRoom = GameObject.FindGameObjectWithTag("BossParent");
        if (bossRoom != null && !bossDiscovered)
        {
            Vector3 bossPos = new Vector3(
                Mathf.Round(bossRoom.transform.position.x / 10f) * 10f,
                Mathf.Round(bossRoom.transform.position.y / 10f) * 10f,
                0
            );

            if (visitedRooms.Contains(bossPos) || adjacentRooms.Contains(bossPos))
            {
                bossDiscovered = true;
                if (bossIcon == null && minimapInitialized)
                {
                    CreateBossIcon();
                }
            }
        }

        GameObject cauldronRoom = GameObject.FindGameObjectWithTag("Cauldron");
        if (cauldronRoom != null && !cauldronDiscovered)
        {
            Vector3 cauldronPos = new Vector3(
                Mathf.Round(cauldronRoom.transform.position.x / 10f) * 10f,
                Mathf.Round(cauldronRoom.transform.position.y / 10f) * 10f,
                0
            );

            if (visitedRooms.Contains(cauldronPos) || adjacentRooms.Contains(cauldronPos))
            {
                cauldronDiscovered = true;
                if (questionIcon == null && minimapInitialized)
                {
                    CreateQuestionMarkIcon();
                }
            }
        }
    }

    private void UpdateIconVisibility()
    {
        if (!minimapInitialized) return;

        foreach (MinimapIcon icon in minimapIcons)
        {
            if (icon.icon == null) continue;

            Vector3 roundedPos = new Vector3(
                Mathf.Round(icon.initialRoomPosition.x / 10f) * 10f,
                Mathf.Round(icon.initialRoomPosition.y / 10f) * 10f,
                0
            );

            if (visitedRooms.Contains(roundedPos))
            {
                icon.icon.SetActive(true);
                icon.icon.GetComponent<Image>().color = icon.originalColor;
            }
            else if (adjacentRooms.Contains(roundedPos))
            {
                icon.icon.SetActive(true);
                // Make adjacent rooms gray and semi-transparent
                icon.icon.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
            else
            {
                icon.icon.SetActive(false);
            }
        }
    }

    public void CreateMinimapIcons()
    {
        minimapIcons.Clear();
        foreach (var room in roomTemplates.rooms)
        {
            if (room != null)
            {
                string roomType = room.CompareTag("Store") ? "Store" :
                                  room.CompareTag("Boss") ? "Boss" :
                                  room.CompareTag("Cauldron") ? "Cauldron" :
                                  "Normal";

                CreateMinimapIcon(room.transform.position, roomType);
            }
        }
    }

    public void CreateMinimapIcon(Vector3 roomPosition, string roomType)
    {
        GameObject icon = Instantiate(minimapIconPrefab, minimapCanvas);
        UpdateIconPosition(icon, roomPosition);

        Image iconImage = icon.GetComponent<Image>();
        Color iconColor = Color.white;

        if (iconImage != null)
        {
            switch (roomType)
            {
                case "Store":
                    iconColor = Color.yellow;
                    break;
                case "Boss":
                    iconColor = Color.red;
                    break;
                case "Cauldron":
                    iconColor = Color.cyan;
                    break;
                case "Normal":
                    iconColor = Color.white;
                    break;
                default:
                    iconColor = Color.white;
                    break;
            }

            iconImage.color = iconColor;
        }

        MinimapIcon newIcon = new MinimapIcon(icon, roomPosition);
        newIcon.originalColor = iconColor;
        minimapIcons.Add(newIcon);

        // Make sure all icons start hidden until properly discovered
        icon.SetActive(false);
    }

    public void UpdateIconPosition(GameObject icon, Vector3 roomPosition)
    {
        Vector2 minimapPosition = new Vector2(roomPosition.x * baseIconSpacing + minimapOffset.x, roomPosition.y * baseIconSpacing + minimapOffset.y);
        icon.GetComponent<RectTransform>().anchoredPosition = minimapPosition;
    }

    public void RemoveMinimapIcon(GameObject room)
    {
        MinimapIcon iconToRemove = minimapIcons.Find(x => x.initialRoomPosition == room.transform.position);
        if (iconToRemove != null)
        {
            Destroy(iconToRemove.icon);
            minimapIcons.Remove(iconToRemove);
        }
    }

    private void ToggleMinimap()
    {
        isMinimapVisible = !isMinimapVisible;
        minimapUI.SetActive(isMinimapVisible);
        RunStats.SetActive(isMinimapVisible);
        DI = FindObjectOfType<DungeonInfo>();
        if (DI != null) { DI.UpdateRunInfo(); }

        if (isMinimapVisible)
        {
            if (!minimapInitialized)
            {
                minimapInitialized = true;
                CreateMinimapIcons();
                if (playerIcon == null)
                {
                    CreatePlayerIcon();
                }
            }

            ResetZoomAndSpacing();
            UpdateIconVisibility();
        }
    }

    private void CreatePlayerIcon()
    {
        if (playerIconPrefab != null && playerTransform != null)
        {
            // Destroy any existing player icon first
            if (playerIcon != null)
            {
                Destroy(playerIcon);
            }
            playerIcon = Instantiate(playerIconPrefab, minimapCanvas);
        }
    }

    public void ClearMinimapIcons()
    {
        minimapIcons.Clear();
        GameObject[] mapIcons = GameObject.FindGameObjectsWithTag("MapIcon");
        foreach (GameObject mapIcon in mapIcons)
        {
            Destroy(mapIcon);
        }
        foreach (MinimapIcon minimapIcon in minimapIcons)
        {
            if (minimapIcon.icon != null)
            {
                Destroy(minimapIcon.icon);
            }
        }
        minimapIcons.Clear();

        if (playerIcon != null)
        {
            Destroy(playerIcon);
            playerIcon = null;
        }

        if (storeIcon != null)
        {
            Destroy(storeIcon);
            storeIcon = null;
        }

        if (bossIcon != null)
        {
            Destroy(bossIcon);
            bossIcon = null;
        }

        if (questionIcon != null)
        {
            Destroy(questionIcon);
            questionIcon = null;
        }

        if (waypointIcon != null)
        {
            Destroy(waypointIcon);
            waypointIcon = null;
        }

        visitedRooms.Clear();
        adjacentRooms.Clear();
        storeDiscovered = false;
        bossDiscovered = false;
        cauldronDiscovered = false;
        minimapInitialized = false;
    }

    private void CreateStoreIcon()
    {
        GameObject storeRoom = GameObject.FindGameObjectWithTag("Store");
        if (storeIconPrefab != null && storeRoom != null)
        {
            storeIcon = Instantiate(storeIconPrefab, minimapCanvas);
        }
    }

    private void CreateBossIcon()
    {
        GameObject bossRoom = GameObject.FindGameObjectWithTag("BossParent");
        if (bossIconPrefab != null && bossRoom != null)
        {
            bossIcon = Instantiate(bossIconPrefab, minimapCanvas);
        }
    }

    private void CreateQuestionMarkIcon()
    {
        GameObject questionObject = GameObject.FindGameObjectWithTag("Cauldron");
        if (questionMarkIconPrefab != null && questionObject != null)
        {
            questionIcon = Instantiate(questionMarkIconPrefab, minimapCanvas);
        }
    }

    private void CreateWaypoint()
    {
        if (waypointIcon != null)
        {
            Destroy(waypointIcon);
        }

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapCanvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            null,
            out mousePos
        );

        waypointIcon = Instantiate(waypointIconPrefab, minimapCanvas);
        waypointIcon.GetComponent<RectTransform>().anchoredPosition = mousePos;
    }

    private void UpdatePlayerIconPosition()
    {
        Vector2 playerMinimapPosition = new Vector2(playerTransform.position.x * baseIconSpacing + minimapOffset.x, playerTransform.position.y * baseIconSpacing + minimapOffset.y);
        playerIcon.GetComponent<RectTransform>().anchoredPosition = playerMinimapPosition;
    }

    private void UpdateStoreIconPosition()
    {
        GameObject storeRoom = GameObject.FindGameObjectWithTag("Store");
        if (storeRoom != null)
        {
            Vector2 storeMinimapPosition = new Vector2(storeRoom.transform.position.x * baseIconSpacing + minimapOffset.x, storeRoom.transform.position.y * baseIconSpacing + minimapOffset.y);
            storeIcon.GetComponent<RectTransform>().anchoredPosition = storeMinimapPosition;
        }
    }

    private void UpdateBossIconPosition()
    {
        GameObject bossRoom = GameObject.FindGameObjectWithTag("BossParent");
        if (bossIcon != null && bossRoom != null)
        {
            Vector2 bossMinimapPosition = new Vector2(bossRoom.transform.position.x * baseIconSpacing + minimapOffset.x, bossRoom.transform.position.y * baseIconSpacing + minimapOffset.y);
            bossIcon.GetComponent<RectTransform>().anchoredPosition = bossMinimapPosition;
        }
    }

    private void UpdateQuestionMarkIconPosition()
    {
        GameObject questionObject = GameObject.FindGameObjectWithTag("Cauldron");
        if (questionIcon != null && questionObject != null)
        {
            Vector2 questionMinimapPosition = new Vector2(questionObject.transform.position.x * baseIconSpacing + minimapOffset.x, questionObject.transform.position.y * baseIconSpacing + minimapOffset.y);
            questionIcon.GetComponent<RectTransform>().anchoredPosition = questionMinimapPosition;
        }
    }

    private void HandleZoom()
    {
        if (!isMinimapVisible) return;

        float scrollInput = Input.mouseScrollDelta.y;

        if (scrollInput != 0)
        {
            currentZoom = Mathf.Clamp(currentZoom - scrollInput * zoomSpeed, minZoom, maxZoom);
            minimapCanvas.localScale = new Vector3(currentZoom, currentZoom, 1f);
        }
    }

    private void HandleMouseDrag()
    {
        if (!isMinimapVisible) return;

        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = (currentMousePosition - lastMousePosition) / currentZoom;
            minimapOffset += delta;
            lastMousePosition = currentMousePosition;

            UpdateIconSpacingAndPositions();

            if (entryRoom != null)
            {
                entryRoom.GetComponent<RectTransform>().anchoredPosition = new Vector2(minimapOffset.x, minimapOffset.y);
            }

            if (waypointIcon != null)
            {
                Vector2 waypointPosition = waypointIcon.GetComponent<RectTransform>().anchoredPosition;
                waypointPosition += delta;
                waypointIcon.GetComponent<RectTransform>().anchoredPosition = waypointPosition;
            }
        }
    }

    private void ResetZoomAndSpacing()
    {
        currentZoom = 1f;
        minimapOffset = Vector2.zero;
        minimapCanvas.localScale = new Vector3(currentZoom, currentZoom, 1f);

        foreach (MinimapIcon minimapIcon in minimapIcons)
        {
            if (minimapIcon.icon != null)
            {
                UpdateIconPosition(minimapIcon.icon, minimapIcon.initialRoomPosition);
            }
        }

        if (entryRoom != null)
        {
            entryRoom.GetComponent<RectTransform>().anchoredPosition = new Vector2(minimapOffset.x, minimapOffset.y);
        }

        if (waypointIcon != null)
        {
            Destroy(waypointIcon);
            waypointIcon = null;
        }
    }

    private void UpdateIconSpacingAndPositions()
    {
        foreach (MinimapIcon minimapIcon in minimapIcons)
        {
            if (minimapIcon.icon != null)
            {
                Vector3 roomPosition = minimapIcon.initialRoomPosition;
                UpdateIconPosition(minimapIcon.icon, roomPosition);
            }
        }

        if (playerIcon != null)
        {
            UpdatePlayerIconPosition();
        }

        if (storeDiscovered && storeIcon != null)
        {
            UpdateStoreIconPosition();
        }

        if (bossDiscovered && bossIcon != null)
        {
            UpdateBossIconPosition();
        }

        if (cauldronDiscovered && questionIcon != null)
        {
            UpdateQuestionMarkIconPosition();
        }
    }
}

public class MinimapIcon
{
    public GameObject icon;
    public Vector3 initialRoomPosition;
    public Color originalColor;

    public MinimapIcon(GameObject icon, Vector3 initialRoomPosition)
    {
        this.icon = icon;
        this.initialRoomPosition = initialRoomPosition;
        this.originalColor = icon.GetComponent<Image>().color;
    }
}