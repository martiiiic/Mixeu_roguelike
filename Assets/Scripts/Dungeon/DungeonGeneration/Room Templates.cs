using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RoomTemplates : MonoBehaviour
{
    [Header("Room Generation Settings")]
    public int minimumRoomsToGenerate = 15;
    public GameObject EntryRoom;

    [Header("Room Templates")]
    public GameObject[] bottomRooms;
    public GameObject[] topRooms;
    public GameObject[] leftRooms;
    public GameObject[] rightRooms;
    public GameObject[] closedRooms;
    public GameObject[] corridorRooms;

    [Header("Large Room Settings")]
    public GameObject[] largeRooms;
    [Range(0f, 1f)]
    public float largeRoomSpawnChance = 0.2f;
    public int maxLargeRoomsPerDungeon = 2;

    [Header("Manager References")]
    public EnemySpawnManager enemySpawnManager;
    public SpecialRoomManager specialRoomManager;
    public DungeonStateManager dungeonStateManager;
    public GameObject player;
    public GameObject WC_; // For WeaponCollectible reference

    // Runtime properties - these need to be public for the editor
    [HideInInspector]
    public bool spawnedBoss = false;
    [HideInInspector]
    public bool enemyRoomsComplete = false;
    [HideInInspector]
    public List<GameObject> rooms = new List<GameObject>();
    [HideInInspector]
    public HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();
    [HideInInspector]
    public int largeRoomsSpawned = 0;

    private void Awake()
    {
        // Initialize managers if not already set
        if (dungeonStateManager == null)
        {
            dungeonStateManager = GetComponent<DungeonStateManager>();
            if (dungeonStateManager == null)
            {
                dungeonStateManager = gameObject.AddComponent<DungeonStateManager>();
            }
        }

        if (specialRoomManager == null)
        {
            specialRoomManager = GetComponent<SpecialRoomManager>();
            if (specialRoomManager == null)
            {
                specialRoomManager = gameObject.AddComponent<SpecialRoomManager>();
            }
        }

        if (enemySpawnManager == null)
        {
            enemySpawnManager = GetComponent<EnemySpawnManager>();
            if (enemySpawnManager == null)
            {
                enemySpawnManager = gameObject.AddComponent<EnemySpawnManager>();
            }
        }

        // Initialize the managers
        InitializeManagers();
    }

    public void InitializeManagers()
    {
        dungeonStateManager.Initialize(this, specialRoomManager, enemySpawnManager);
        specialRoomManager.Initialize(this);
        // Assuming EnemySpawnManager has similar Initialize method
        if (enemySpawnManager != null)
        {
            // This would depend on your EnemySpawnManager implementation
            // enemySpawnManager.Initialize(this);
        }
    }

    public void StartDungeonGeneration()
    {
        // Reset room collections
        rooms.Clear();
        occupiedPositions.Clear();
        spawnedBoss = false;
        enemyRoomsComplete = false;
        largeRoomsSpawned = 0;

        // Reset manager states
        dungeonStateManager.InitializeState();

        // Add entry room if it exists
        if (EntryRoom != null)
        {
            rooms.Add(EntryRoom);
            occupiedPositions.Add(new Vector2(EntryRoom.transform.position.x, EntryRoom.transform.position.y));
        }

        // Start dungeon setup
        StartCoroutine(SetupDungeonAsync());
    }

    private IEnumerator SetupDungeonAsync()
    {
        // Wait for room generation to complete (handled by RoomSpawner scripts)
        yield return new WaitUntil(() => rooms.Count >= minimumRoomsToGenerate);

        // Let the dungeon state manager handle the setup
        dungeonStateManager.isSettingUp = true;
        yield return StartCoroutine(dungeonStateManager.SetupDungeonAsync());
    }

    // Add a room to the dungeon
    public void AddRoom(GameObject room, Vector2 position)
    {
        if (!occupiedPositions.Contains(position))
        {
            rooms.Add(room);
            occupiedPositions.Add(position);
        }
    }

    // Check if a position is occupied
    public bool IsPositionOccupied(Vector2 position)
    {
        return occupiedPositions.Contains(position);
    }

    // Get a random room based on direction
    public GameObject GetRandomRoom(int direction)
    {
        switch (direction)
        {
            case 1: // Bottom opening (need top room)
                return topRooms[Random.Range(0, topRooms.Length)];
            case 2: // Top opening (need bottom room)
                return bottomRooms[Random.Range(0, bottomRooms.Length)];
            case 3: // Left opening (need right room)
                return rightRooms[Random.Range(0, rightRooms.Length)];
            case 4: // Right opening (need left room)
                return leftRooms[Random.Range(0, leftRooms.Length)];
            default:
                return null;
        }
    }

    // Try to spawn a large room
    public bool TrySpawnLargeRoom(Vector2 position)
    {
        if (largeRoomsSpawned >= maxLargeRoomsPerDungeon ||
            Random.value > largeRoomSpawnChance ||
            largeRooms.Length == 0)
        {
            return false;
        }

        // Check if all surrounding positions are free (assuming 2x2 large room)
        bool canSpawnLargeRoom = true;
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector2 checkPos = position + new Vector2(x * 10f, y * 10f);
                if (IsPositionOccupied(checkPos))
                {
                    canSpawnLargeRoom = false;
                    break;
                }
            }
        }

        if (canSpawnLargeRoom)
        {
            // Spawn the large room
            GameObject largeRoom = Instantiate(largeRooms[Random.Range(0, largeRooms.Length)],
                                              new Vector3(position.x, position.y, 0),
                                              Quaternion.identity);

            // Mark all positions as occupied
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Vector2 occupyPos = position + new Vector2(x * 10f, y * 10f);
                    occupiedPositions.Add(occupyPos);
                }
            }

            rooms.Add(largeRoom);
            largeRoomsSpawned++;
            return true;
        }

        return false;
    }

    // Helper method to find and get a room at a specific position
    public GameObject GetRoomAtPosition(Vector2 position)
    {
        foreach (GameObject room in rooms)
        {
            if (room != null)
            {
                Vector2 roomPos = new Vector2(room.transform.position.x, room.transform.position.y);
                if (roomPos == position)
                {
                    return room;
                }
            }
        }
        return null;
    }

    // Reset the dungeon
    public void ResetDungeon(bool playerDied = false)
    {
        if (dungeonStateManager != null)
        {
            dungeonStateManager.ResetState(playerDied);
        }

        // Clear rooms
        foreach (GameObject room in rooms)
        {
            if (room != null && room != EntryRoom)
            {
                Destroy(room);
            }
        }

        rooms.Clear();
        occupiedPositions.Clear();
        largeRoomsSpawned = 0;

        // Only keep entry room
        if (EntryRoom != null)
        {
            rooms.Add(EntryRoom);
            occupiedPositions.Add(new Vector2(EntryRoom.transform.position.x, EntryRoom.transform.position.y));
        }

        spawnedBoss = false;
        enemyRoomsComplete = false;
    }

    // METHODS WITH PROPER OVERLOADS TO MAINTAIN BACKWARD COMPATIBILITY

    // Base method with no parameters (for backward compatibility)
    public void DeleteAllRooms()
    {
        DeleteAllRooms(false, false, false);
    }

    // Method with one parameter (for backward compatibility)
    public void DeleteAllRooms(bool playerDied)
    {
        DeleteAllRooms(playerDied, false, false);
    }

    // Method with three parameters (for PowerUpStoreUI.cs)
    public void DeleteAllRooms(bool playerDied, bool resetPlayerPosition, bool resetGameState)
    {
        // Call the base reset functionality
        ResetDungeon(playerDied);

        // Handle player position reset if needed
        if (resetPlayerPosition && player != null)
        {
            // Reset player to starting position (entry room or default position)
            if (EntryRoom != null)
            {
                player.transform.position = EntryRoom.transform.position;
            }
            else
            {
                player.transform.position = Vector3.zero;
            }
        }

        // Handle additional game state reset if needed
        if (resetGameState)
        {
            // Additional reset logic for complete game state reset
            // This may include resetting player stats, inventory, etc.
            // depending on your game implementation

            // If you have a GameManager or similar, you might call its reset method here
            // GameManager.Instance.ResetGameState();

            // For now, ensure all managers are reset
            if (dungeonStateManager != null)
            {
                dungeonStateManager.InitializeState();
            }

            if (specialRoomManager != null)
            {
                specialRoomManager.Initialize(this);
            }

            if (enemySpawnManager != null)
            {
                // Reset enemy spawn manager if it has such a method
                // enemySpawnManager.ResetState();
            }
        }
    }

    // Special overload to handle the int parameter in PowerUpStoreUI.cs
    public void DeleteAllRooms(bool playerDied, bool resetPlayerPosition, int resetGameStateAsInt)
    {
        // Convert int to bool (0 = false, anything else = true)
        bool resetGameState = resetGameStateAsInt != 0;
        DeleteAllRooms(playerDied, resetPlayerPosition, resetGameState);
    }

    // Methods for checking large room placement (used in RoomSpawner.cs)
    public bool CanSpawnLargeRoom(Vector2 position)
    {
        return largeRoomsSpawned < maxLargeRoomsPerDungeon &&
               largeRooms.Length > 0 &&
               Random.value <= largeRoomSpawnChance;
    }

    public bool IsAreaAvailableForLargeRoom(Vector2 position)
    {
        // Check if a 2x2 area is free for a large room
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector2 checkPos = position + new Vector2(x * 10f, y * 10f);
                if (IsPositionOccupied(checkPos))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Method to add large room positions (used in RoomSpawner.cs)
    public void AddLargeRoomPositions(Vector2 position)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector2 occupyPos = position + new Vector2(x * 10f, y * 10f);
                occupiedPositions.Add(occupyPos);
            }
        }
    }

    // Method to increment large room count (used in RoomSpawner.cs)
    public void IncrementLargeRoomCount()
    {
        largeRoomsSpawned++;
    }

    // Method to add a position as occupied (used in RoomSpawner.cs)
    public void AddPositionAsOccupied(Vector2 position)
    {
        if (!occupiedPositions.Contains(position))
        {
            occupiedPositions.Add(position);
        }
    }

    // Method to check if a room is occupied (used in RoomSpawner.cs)
    public bool IsRoomOccupied(Vector2 position)
    {
        return occupiedPositions.Contains(position);
    }
}