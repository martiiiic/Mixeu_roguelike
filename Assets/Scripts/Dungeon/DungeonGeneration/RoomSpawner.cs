using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public int openingDirection;
    private RoomTemplates templates;
    private int rand;
    public bool spawned = false;
    private MinimapController minimapController;
    private BoxCollider2D boxCollider;

    private bool HasChecked;
    private bool isLargeRoom = false; // Flag to track if this is a large room spawn point

    private void Awake()
    {
        StartCoroutine(RecheckTrigger());
        HasChecked = false;
        templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomTemplates>();
        minimapController = FindObjectOfType<MinimapController>();
        boxCollider = GetComponent<BoxCollider2D>();

        Invoke("Spawn", 0.1f);
    }

    private void Spawn()
    {
        if (spawned) return;

        GameObject roomInstance = null;
        Vector2 position = new Vector2(transform.position.x, transform.position.y);

        // Check if we should spawn a large room
        if (ShouldSpawnLargeRoom(position))
        {
            roomInstance = SpawnLargeRoom(position);
            isLargeRoom = true;
        }
        else
        {
            // Regular room spawning logic
            roomInstance = SpawnRegularRoom();
        }

        if (roomInstance != null && minimapController != null)
        {
            CreateMinimapIcon(roomInstance);
        }

        spawned = true;
    }

    private bool ShouldSpawnLargeRoom(Vector2 position)
    {
        // Only consider large rooms for certain opening directions (e.g., bottom-right corner of a large room)
        // This prevents multiple attempts to spawn large rooms at the same location
        if (openingDirection != 1) return false; // Only consider from the right opening direction

        // Check if we're at a valid position to start a large room (not near edges or other limitations)
        if (templates.CanSpawnLargeRoom(position) && templates.IsAreaAvailableForLargeRoom(position))
        {
            // Use a random chance to actually spawn the large room
            return Random.value < templates.largeRoomSpawnChance;
        }

        return false;
    }

    private GameObject SpawnLargeRoom(Vector2 position)
    {
        if (templates.largeRooms.Length == 0)
            return null;

        // Choose a random large room from the template
        rand = Random.Range(0, templates.largeRooms.Length);
        GameObject largeRoomInstance = Instantiate(templates.largeRooms[rand], transform.position, Quaternion.identity);

        // Mark the 2x2 grid positions as occupied
        templates.AddLargeRoomPositions(position);
        templates.IncrementLargeRoomCount();

        return largeRoomInstance;
    }

    private GameObject SpawnRegularRoom()
    {
        int currentRoomCount = templates.rooms.Count;
        bool isFirstRoom = currentRoomCount <= 1;
        GameObject roomInstance = null;

        switch (openingDirection)
        {
            case 1: // Right opening
                if (isFirstRoom)
                {
                    List<GameObject> validRooms = templates.rightRooms
                        .Where(room => room.name.Length > 2)
                        .ToList();

                    if (validRooms.Count > 0)
                    {
                        rand = Random.Range(0, validRooms.Count);
                        roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                    }
                    else
                    {
                        rand = Random.Range(0, templates.rightRooms.Length);
                        roomInstance = Instantiate(templates.rightRooms[rand], transform.position, Quaternion.identity);
                    }
                }
                else
                {
                    rand = Random.Range(0, templates.rightRooms.Length);
                    roomInstance = Instantiate(templates.rightRooms[rand], transform.position, Quaternion.identity);
                }
                break;

            case 2: // Left opening
                if (isFirstRoom)
                {
                    List<GameObject> validRooms = templates.leftRooms
                        .Where(room => room.name.Length > 2)
                        .ToList();

                    if (validRooms.Count > 0)
                    {
                        rand = Random.Range(0, validRooms.Count);
                        roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                    }
                    else
                    {
                        rand = Random.Range(0, templates.leftRooms.Length);
                        roomInstance = Instantiate(templates.leftRooms[rand], transform.position, Quaternion.identity);
                    }
                }
                else
                {
                    rand = Random.Range(0, templates.leftRooms.Length);
                    roomInstance = Instantiate(templates.leftRooms[rand], transform.position, Quaternion.identity);
                }
                break;

            case 3: // Top opening
                if (isFirstRoom)
                {
                    List<GameObject> validRooms = templates.topRooms
                        .Where(room => room.name.Length > 2)
                        .ToList();

                    if (validRooms.Count > 0)
                    {
                        rand = Random.Range(0, validRooms.Count);
                        roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                    }
                    else
                    {
                        rand = Random.Range(0, templates.topRooms.Length);
                        roomInstance = Instantiate(templates.topRooms[rand], transform.position, Quaternion.identity);
                    }
                }
                else
                {
                    rand = Random.Range(0, templates.topRooms.Length);
                    roomInstance = Instantiate(templates.topRooms[rand], transform.position, Quaternion.identity);
                }
                break;

            case 4: // Bottom opening
                if (isFirstRoom)
                {
                    List<GameObject> validRooms = templates.bottomRooms
                        .Where(room => room.name.Length > 2)
                        .ToList();

                    if (validRooms.Count > 0)
                    {
                        rand = Random.Range(0, validRooms.Count);
                        roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                    }
                    else
                    {
                        rand = Random.Range(0, templates.bottomRooms.Length);
                        roomInstance = Instantiate(templates.bottomRooms[rand], transform.position, Quaternion.identity);
                    }
                }
                else
                {
                    rand = Random.Range(0, templates.bottomRooms.Length);
                    roomInstance = Instantiate(templates.bottomRooms[rand], transform.position, Quaternion.identity);
                }
                break;
        }

        // Mark this position as occupied in the templates
        templates.AddPositionAsOccupied(new Vector2(transform.position.x, transform.position.y));

        return roomInstance;
    }

    private void CreateMinimapIcon(GameObject roomInstance)
    {
        string roomType = "Normal";

        if (roomInstance.CompareTag("Store"))
            roomType = "Store";
        else if (roomInstance.CompareTag("Boss"))
            roomType = "Boss";
        else if (roomInstance.CompareTag("Cauldron"))
            roomType = "Cauldron";
        else if (roomInstance.CompareTag("Enemy"))
            roomType = "Enemy";
        else if (isLargeRoom)
            roomType = "Large"; // Special icon for large rooms

        minimapController.CreateMinimapIcon(roomInstance.transform.position, roomType);

        // If this is a large room, create additional minimap markers for each cell
        if (isLargeRoom)
        {
            Vector3 basePos = roomInstance.transform.position;
            // Create additional minimap icons for each cell of the 2x2 grid
            minimapController.CreateMinimapIcon(basePos + new Vector3(1, 0, 0), "LargeCell");
            minimapController.CreateMinimapIcon(basePos + new Vector3(0, 1, 0), "LargeCell");
            minimapController.CreateMinimapIcon(basePos + new Vector3(1, 1, 0), "LargeCell");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpawnPoint"))
        {
            RoomSpawner otherSpawner = other.gameObject.GetComponent<RoomSpawner>();
            if (otherSpawner != null)
            {
                if (!otherSpawner.spawned && !spawned && transform.position.x != 0 && transform.position.y != 0)
                {
                    if (templates != null && templates.closedRooms != null)
                    {
                        Vector2 position = new Vector2(transform.position.x, transform.position.y);

                        // Skip placing closed rooms if this position is part of a large room
                        if (!templates.IsRoomOccupied(transform.position))
                        {
                            Instantiate(templates.closedRooms[Random.Range(0, templates.closedRooms.Length)], transform.position, Quaternion.identity);
                            templates.AddPositionAsOccupied(position);
                            HasChecked = true;
                            StopCoroutine(RecheckTrigger());
                            Destroy(gameObject);
                        }
                        else
                        {
                            Debug.LogWarning("Room position is already occupied, skipping spawn.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Templates or closedRoom is not assigned!");
                    }
                }
                spawned = true;
            }
            else
            {
                Debug.Log("RoomSpawner component not found on the 'other' object.");
            }
        }
    }

    private IEnumerator RecheckTrigger()
    {
        if (boxCollider != null && boxCollider.isTrigger)
        {
            yield return new WaitForSeconds(5f);
            boxCollider.enabled = false;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            boxCollider.enabled = true;
        }
    }
}