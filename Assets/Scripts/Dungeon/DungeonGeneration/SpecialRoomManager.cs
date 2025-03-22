using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SpecialRoom
{
    public string roomName;
    public GameObject roomPrefab;
    public string roomTag;
    [Range(0f, 1f)]
    public float spawnProbability = 0.5f;
    public int minDungeonLevel = 1;
    public int minDungeonRooms = 5;
    public int minPerDungeon = 0;
    public int maxPerDungeon = 1;
    public int preferredLocation = -1; // -1 means no preference
    [HideInInspector]
    public bool hasSpawned = false;
}

public class SpecialRoomManager : MonoBehaviour
{
    [Header("Special Rooms")]
    public List<SpecialRoom> specialRooms = new List<SpecialRoom>();

    [Header("Default Special Rooms")]
    public GameObject boss;
    public GameObject chestRoom;
    public GameObject store;
    public GameObject Casino;
    public GameObject Cauldron;

    [HideInInspector]
    public bool spawnedBoss = false;
    [HideInInspector]
    public bool spawnedStore = false;
    [HideInInspector]
    public bool spawnedCasino = false;
    [HideInInspector]
    public bool spawnedChestRooms = false;
    [HideInInspector]
    public bool spawnedCauldron = false;

    private RoomTemplates roomTemplates;
    private RoomSizeDetector roomSizeDetector;
    private GameManager gameManager;

    public void Initialize(RoomTemplates templates)
    {
        roomTemplates = templates;
        gameManager = FindObjectOfType<GameManager>();
        roomSizeDetector = GetComponent<RoomSizeDetector>();

        if (roomSizeDetector == null)
        {
            roomSizeDetector = gameObject.AddComponent<RoomSizeDetector>();
        }

        ResetSpecialRoomStatus();
    }

    public void ResetSpecialRoomStatus()
    {
        spawnedBoss = false;
        spawnedStore = false;
        spawnedCasino = false;
        spawnedChestRooms = false;
        spawnedCauldron = false;

        foreach (var specialRoom in specialRooms)
        {
            specialRoom.hasSpawned = false;
        }
    }

    public IEnumerator SpawnSpecialRoomsAsync()
    {
        if (!spawnedBoss)
        {
            yield return SpawnBossRoom();
        }

        if (!spawnedStore)
        {
            yield return SpawnStoreRoom();
        }

        yield return SpawnCustomSpecialRoomsAsync();

        if (!spawnedCasino)
        {
            yield return AttemptSpawnCasinoAsync();
        }

        if (!spawnedChestRooms && gameManager.DungeonNumber > 2)
        {
            yield return SpawnChestRoomsAsync();
            spawnedChestRooms = true;
        }

        if (!spawnedCauldron && gameManager.DungeonNumber > 3)
        {
            yield return SpawnCauldronRoomAsync();
            spawnedCauldron = true;
        }
    }

    private IEnumerator SpawnBossRoom()
    {
        if (roomTemplates.rooms.Count > 0)
        {
            Instantiate(boss, roomTemplates.rooms[^1].transform.position, Quaternion.identity);
            spawnedBoss = true;
        }
        yield return null;
    }

    private IEnumerator SpawnStoreRoom()
    {
        if (gameManager.DungeonNumber > 4)
        {
            gameManager.PriceMultiplier += (roomTemplates.rooms.Count / 5);
        }

        // Find valid rooms for store
        List<int> validRoomIndices = new List<int>();

        for (int i = Mathf.RoundToInt(roomTemplates.rooms.Count * 0.25f); i < roomTemplates.rooms.Count - 1; i++)
        {
            GameObject room = roomTemplates.rooms[i];
            if (roomSizeDetector.CanPlaceSpecialRoomInRoom(room) && !IsRoomOccupied(room.transform.position))
            {
                validRoomIndices.Add(i);
            }
            yield return null;
        }

        if (validRoomIndices.Count > 0)
        {
            int storeRoomIndex = validRoomIndices[UnityEngine.Random.Range(0, validRoomIndices.Count)];
            Instantiate(store, roomTemplates.rooms[storeRoomIndex].transform.position, Quaternion.identity);
            spawnedStore = true;
        }

        yield return null;
    }

    private IEnumerator SpawnCustomSpecialRoomsAsync()
    {
        foreach (var specialRoom in specialRooms)
        {
            if (specialRoom.hasSpawned || specialRoom.roomPrefab == null)
                continue;

            if (gameManager.DungeonNumber < specialRoom.minDungeonLevel ||
                roomTemplates.rooms.Count < specialRoom.minDungeonRooms)
                continue;

            if (UnityEngine.Random.value > specialRoom.spawnProbability)
                continue;

            int spawnCount = 0;
            int maxSpawns = UnityEngine.Random.Range(specialRoom.minPerDungeon, specialRoom.maxPerDungeon + 1);

            List<int> availableRooms = new List<int>();
            for (int i = 0; i < roomTemplates.rooms.Count - 1; i++)
            {
                if (roomTemplates.rooms[i] != null &&
                    roomSizeDetector.CanPlaceSpecialRoomInRoom(roomTemplates.rooms[i]) &&
                    !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, specialRoom.roomTag) &&
                    !IsRoomOccupied(roomTemplates.rooms[i].transform.position))
                {
                    availableRooms.Add(i);
                }
                yield return null;
            }

            // Handle preferred location if specified
            if (specialRoom.preferredLocation > 0 && specialRoom.preferredLocation < roomTemplates.rooms.Count)
            {
                if (availableRooms.Contains(specialRoom.preferredLocation))
                {
                    Instantiate(specialRoom.roomPrefab,
                                roomTemplates.rooms[specialRoom.preferredLocation].transform.position,
                                Quaternion.identity);
                    spawnCount++;
                    availableRooms.Remove(specialRoom.preferredLocation);
                }
            }

            // Shuffle available rooms using Fisher-Yates algorithm instead of OrderBy
            for (int i = availableRooms.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = availableRooms[i];
                availableRooms[i] = availableRooms[j];
                availableRooms[j] = temp;
            }

            // Spawn the remaining rooms up to maxSpawns
            for (int i = 0; i < maxSpawns - spawnCount && i < availableRooms.Count; i++)
            {
                Instantiate(specialRoom.roomPrefab,
                            roomTemplates.rooms[availableRooms[i]].transform.position,
                            Quaternion.identity);
                yield return null;
            }

            specialRoom.hasSpawned = true;
        }
    }

    private IEnumerator AttemptSpawnCasinoAsync()
    {
        if (spawnedCasino) yield break;

        int chance = UnityEngine.Random.Range(0, 4);
        if (chance != 0) yield break;

        List<int> availableRooms = new List<int>();
        for (int i = 4; i < roomTemplates.rooms.Count - 1; i++)
        {
            if (roomTemplates.rooms[i] != null &&
                roomSizeDetector.CanPlaceSpecialRoomInRoom(roomTemplates.rooms[i]) &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "Store"))
            {
                availableRooms.Add(i);
            }
            yield return null;
        }

        if (availableRooms.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            Instantiate(Casino, roomTemplates.rooms[availableRooms[randomIndex]].transform.position, Quaternion.identity);
            spawnedCasino = true;
        }
    }

    private IEnumerator SpawnChestRoomsAsync()
    {
        List<int> availableRooms = new List<int>();
        for (int i = 4; i < roomTemplates.rooms.Count - 1; i++)
        {
            if (roomTemplates.rooms[i] != null &&
                roomSizeDetector.CanPlaceSpecialRoomInRoom(roomTemplates.rooms[i]) &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "Store") &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "Chest") &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "BossParent") &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "Cauldron"))
            {
                availableRooms.Add(i);
            }
            yield return null;
        }

        int numChestRooms = Mathf.Clamp(UnityEngine.Random.Range(1, availableRooms.Count / 2), 1, 2);
        for (int i = 0; i < numChestRooms && availableRooms.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            Instantiate(chestRoom, roomTemplates.rooms[availableRooms[randomIndex]].transform.position, Quaternion.identity);
            availableRooms.RemoveAt(randomIndex);
            yield return null;
        }
    }

    private IEnumerator SpawnCauldronRoomAsync()
    {
        List<int> availableRooms = new List<int>();
        for (int i = 4; i < roomTemplates.rooms.Count - 1; i++)
        {
            if (roomTemplates.rooms[i] != null &&
                roomSizeDetector.CanPlaceSpecialRoomInRoom(roomTemplates.rooms[i]) &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "Store") &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "Chest") &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "BossParent") &&
                !RoomContainsTaggedObject(roomTemplates.rooms[i].transform.position, "Cauldron"))
            {
                availableRooms.Add(i);
            }
            yield return null;
        }

        if (availableRooms.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            Instantiate(Cauldron, roomTemplates.rooms[availableRooms[randomIndex]].transform.position, Quaternion.identity);
            spawnedCauldron = true;
        }
    }

    private bool RoomContainsTaggedObject(Vector3 position, string tag)
    {
        return Physics2D.OverlapCircleAll(position, 1f).Any(collider => collider.CompareTag(tag));
    }

    private bool IsRoomOccupied(Vector3 roomPosition)
    {
        return Physics2D.OverlapCircleAll(roomPosition, 1f).Any(collider =>
            collider.CompareTag("Store") ||
            collider.CompareTag("Cauldron") ||
            collider.CompareTag("ClosedRoom") ||
            collider.CompareTag("Enemy") ||
            collider.CompareTag("Chest") ||
            collider.CompareTag("BossParent"));
    }
}