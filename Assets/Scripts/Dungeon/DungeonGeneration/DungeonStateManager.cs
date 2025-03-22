using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonStateManager : MonoBehaviour
{
    // State variables
    [HideInInspector] public bool isSettingUp = false;
    [HideInInspector] public bool enemyRoomsComplete = false;
    [HideInInspector] public bool isPrefabDungeon = false;
    [HideInInspector] public int largeRoomsSpawned = 0;
    [HideInInspector] public float waitTime = 2f;
    [HideInInspector] public bool spawnedBoss = false;

    // Special dungeon
    private int currentSpecialDungeonID = -1;
    public GameObject[] PrefabDungeons;

    // Managers
    private RoomTemplates roomTemplates;
    private SpecialRoomManager specialRoomManager;
    private EnemySpawnManager enemySpawnManager;
    private RoomSizeDetector roomSizeDetector;

    // External references
    private GameManager gameManager;
    private SpecialPlayerStats specialPlayerStats;
    private OnDungeonEnterText dungeonText;
    private WeaponTypes weaponTypes;
    private VoidBoundEvents voidBinds;

    // UI references
    public GameObject WC;
    public GameObject WC_;
    public GameObject Hand;
    public GameObject ObjectMenu;

    public void Initialize(RoomTemplates templates, SpecialRoomManager specialManager, EnemySpawnManager enemyManager)
    {
        roomTemplates = templates;
        specialRoomManager = specialManager;
        enemySpawnManager = enemyManager;

        roomSizeDetector = GetComponent<RoomSizeDetector>();
        if (roomSizeDetector == null)
        {
            roomSizeDetector = gameObject.AddComponent<RoomSizeDetector>();
        }

        // Find external references
        gameManager = FindObjectOfType<GameManager>();
        dungeonText = FindObjectOfType<OnDungeonEnterText>();
        weaponTypes = FindObjectOfType<WeaponTypes>();
        specialPlayerStats = FindObjectOfType<SpecialPlayerStats>();
        voidBinds = FindObjectOfType<VoidBoundEvents>();
    }

    public void InitializeState()
    {
        isSettingUp = false;
        enemyRoomsComplete = false;
        isPrefabDungeon = false;
        largeRoomsSpawned = 0;
        waitTime = 2f;

        if (dungeonText != null) dungeonText.ShowDungeonText(true, "");
    }

    public IEnumerator SetupDungeonAsync()
    {
        while (roomTemplates.rooms.Count < roomTemplates.minimumRoomsToGenerate)
        {
            yield return new WaitForSeconds(0.5f);
        }

        DungeonInfo dungeonInfo = FindObjectOfType<DungeonInfo>();
        if (dungeonInfo != null) dungeonInfo.UpdateRunInfo();

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        // Reset special room status
        specialRoomManager.ResetSpecialRoomStatus();

        // Spawn special rooms
        yield return StartCoroutine(specialRoomManager.SpawnSpecialRoomsAsync());

        // Spawn enemy rooms if not already complete
        if (!enemyRoomsComplete)
        {
            yield return StartCoroutine(SpawnEnemyRoomsAsync());
            enemyRoomsComplete = true;
        }

        // Add furniture to rooms
        FindObjectOfType<RoomFurnitureSpawner>()?.AddFurnitureToRooms();

        isSettingUp = false;
    }

    public void LoadPreMadeDungeon(int dungeonPrefabID)
{
    // Logic to load a pre-made dungeon based on ID
    isPrefabDungeon = true;
    // Load the appropriate dungeon prefab
    // Example implementation:
    GameObject dungeonPrefab = Resources.Load<GameObject>($"Dungeons/Dungeon_{dungeonPrefabID}");
    if (dungeonPrefab != null)
    {
        Instantiate(dungeonPrefab, Vector3.zero, Quaternion.identity);
    }
}

    public void ResetState(bool playerHasDied, bool preMadeDungeon = false, int dungeonPrefabID = 0)
    {
        // Reset state logic
        waitTime = 2f;
        isSettingUp = false;
        enemyRoomsComplete = false;
        largeRoomsSpawned = 0;
        isPrefabDungeon = preMadeDungeon;

        // Clear existing rooms
        foreach (var room in roomTemplates.rooms.ToList())
        {
            if (room != null && room.name != "EntryRoom")
            {
                Destroy(room);
            }
        }
        roomTemplates.rooms.Clear();
        roomTemplates.occupiedPositions.Clear();

        // Reset special rooms
        specialRoomManager.ResetSpecialRoomStatus();
    }

    private bool RoomContainsTaggedObject(Vector3 position, string tag)
    {
        return Physics2D.OverlapCircleAll(position, 1f).Any(collider => collider.CompareTag(tag));
    }
    private IEnumerator SpawnEnemyRoomsAsync()
    {
        List<int> availableRoomIndices = new List<int>();
        for (int i = 1; i < roomTemplates.rooms.Count - 1; i++)
        {
            GameObject room = roomTemplates.rooms[i];

            if (room != null &&
                roomSizeDetector.CanPlaceEnemiesInRoom(room) &&
                !RoomContainsTaggedObject(room.transform.position, "Store") &&
                !RoomContainsTaggedObject(room.transform.position, "Chest") &&
                !RoomContainsTaggedObject(room.transform.position, "BossParent") &&
                !RoomContainsTaggedObject(room.transform.position, "Cauldron"))
            {
                availableRoomIndices.Add(i);
            }
            yield return null;
        }

        Difficulty difficultyManager = FindObjectOfType<Difficulty>();

        int numEnemyRooms = Mathf.Clamp(availableRoomIndices.Count,
            1 + Mathf.FloorToInt((availableRoomIndices.Count / 3f) * Mathf.Log10(Mathf.Max(1, gameManager.DungeonNumber))),
            Mathf.CeilToInt((roomTemplates.rooms.Count / 2f) + Mathf.FloorToInt(Mathf.Log10(10 + gameManager.DungeonNumber))));

        availableRoomIndices = availableRoomIndices.OrderBy(x => UnityEngine.Random.value).ToList();

        for (int i = 0; i < numEnemyRooms; i++)
        {
            i = Mathf.Clamp(i, 0, availableRoomIndices.Count - 1);
            Vector3 roomPosition = roomTemplates.rooms[availableRoomIndices[i]].transform.position;

            bool isBossRoom = RoomContainsTaggedObject(roomPosition, "BossParent");
            bool isStoreRoom = RoomContainsTaggedObject(roomPosition, "Store");
            bool isChestRoom = RoomContainsTaggedObject(roomPosition, "Chest");
            bool isCauldronRoom = RoomContainsTaggedObject(roomPosition, "Cauldron");
            bool isSpecialRoom = specialRoomManager.specialRooms.Any(sr =>
                RoomContainsTaggedObject(roomPosition, sr.roomTag));

            List<GameObject> validEnemies = enemySpawnManager.GetValidEnemiesForRoom(
                roomPosition,
                isBossRoom,
                isStoreRoom,
                isChestRoom,
                isCauldronRoom,
                isSpecialRoom,
                currentSpecialDungeonID
            );

            if (validEnemies.Count == 0) continue;

            int enemyCount = UnityEngine.Random.Range(
                Mathf.CeilToInt(2 + (0.25f * gameManager.DungeonNumber)),
                Mathf.CeilToInt(4 + (0.5f * gameManager.DungeonNumber))
            );

            enemyCount = Mathf.Clamp(enemyCount, 2, 12);

            RoomSize roomSize = roomSizeDetector.GetRoomSize(roomTemplates.rooms[availableRoomIndices[i]]);
            if (roomSize == RoomSize.Large)
            {
                enemyCount = Mathf.CeilToInt(enemyCount * 1.5f);
            }

            float currentDifficulty = 1.0f;
            if (difficultyManager != null)
            {
                currentDifficulty = difficultyManager.Dificulty + 1.0f;
            }

            yield return StartCoroutine(enemySpawnManager.SpawnEnemiesInRoomAsync(
                roomTemplates.rooms[availableRoomIndices[i]], 
                validEnemies.Count > 0,                        
                enemyCount > 0,                          
                currentDifficulty > 1.0f                    
            ));

            yield return null;
        }
    }
}