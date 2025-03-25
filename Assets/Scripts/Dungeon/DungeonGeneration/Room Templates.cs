using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.GraphicsBuffer;

public class RoomTemplates : MonoBehaviour
{
    public List<SpecialRoom> specialRooms = new List<SpecialRoom>();
    public int minimumRoomsToGenerate = 4;

    [SerializeField]
    public GameObject Hand;

    // Standard rooms (1x1)
    public GameObject[] bottomRooms;
    public GameObject[] topRooms;
    public GameObject[] leftRooms;
    public GameObject[] rightRooms;
    public GameObject[] closedRooms;

    // Large rooms (2x2)
    public GameObject[] largeRooms;
    [Range(0f, 1f)]
    public float largeRoomSpawnChance = 0.2f;
    [Range(1, 5)]
    public int maxLargeRoomsPerDungeon = 2;
    private int largeRoomsSpawned = 0;

    public GameObject EntryRoom;
    public GameObject boss;
    public GameObject chestRoom;
    public GameObject store;
    public GameObject Casino;
    public GameObject Cauldron;

    [HideInInspector]
    public GameObject[] EnemyRoom;

    [SerializeField]
    public EnemySpawnManager enemySpawnManager;

    public List<GameObject> rooms;
    public HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();
    public float waitTime;
    private bool isSettingUp;
    public bool spawnedBoss;
    private bool spawnedStore;
    private bool spawnedCasino;
    private bool spawnedChestRooms;
    public bool enemyRoomsComplete;
    private bool spawnedCauldron;
    private MinimapController minimapController;
    public PlayerState player;
    private OnDungeonEnterText DungeonText;
    private GameManager Game;
    public GameObject ObjectMenu;
    public GameObject WC;
    public GameObject WC_;
    private WeaponTypes WT;
    private SpecialPlayerStats SPS;
    private bool PrefabDungeon = false;
    private VoidBoundEvents VoidBinds;
    private int currentSpecialDungeonID = -1;

    public GameObject[] PrefabDungeons;

    private bool _disposed = false;

    public void Dispose()
    {
        if (!_disposed)
        {
            if (rooms != null)
                rooms.Clear();

            occupiedPositions.Clear();
            _disposed = true;
        }
    }

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerState>();
        Music music = FindObjectOfType<Music>();
        music.PlayRandomMusic(MusicContext.Exploration);
        WT = FindObjectOfType<WeaponTypes>();

        if (enemySpawnManager == null)
        {
            enemySpawnManager = FindObjectOfType<EnemySpawnManager>();
            if (enemySpawnManager == null)
            {
                enemySpawnManager = gameObject.AddComponent<EnemySpawnManager>();
            }
        }
    }

    private void Start()
    {
        SPS = FindObjectOfType<SpecialPlayerStats>();
        WT = FindObjectOfType<WeaponTypes>();
        minimapController = FindObjectOfType<MinimapController>();
        DungeonText = FindObjectOfType<OnDungeonEnterText>();
        Game = FindFirstObjectByType<GameManager>();
        if (DungeonText != null) DungeonText.ShowDungeonText(true, "");
        waitTime = 2f;
        StartCoroutine(InitialRoomGeneration());
    }

    IEnumerator InitialRoomGeneration()
    {
        yield return new WaitForSeconds(0.1f);
        InstantiateNewEntryRoom();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            waitTime = 2f;
            DeleteAllRooms(false);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            PostRoomGeneration(rooms[^1].gameObject, "boss");
        }

        if (waitTime <= 0 && !enemyRoomsComplete && !isSettingUp && !PrefabDungeon)
        {
            isSettingUp = true;
            StartCoroutine(SetupDungeonAsync());
        }
        else
        {
            waitTime -= Time.deltaTime;
        }
    }

    private IEnumerator SetupDungeonAsync()
    {
        while (rooms.Count < minimumRoomsToGenerate)
        {
            yield return new WaitForSeconds(0.5f);
        }

        DungeonInfo DI = FindObjectOfType<DungeonInfo>();
        if (DI != null) DI.UpdateRunInfo();
        Game = FindFirstObjectByType<GameManager>();

        spawnedBoss = false;
        spawnedStore = false;
        spawnedCasino = false;
        spawnedChestRooms = false;
        spawnedCauldron = false;

        if (!spawnedBoss)
        {
            Instantiate(boss, rooms[^1].transform.position, Quaternion.identity);
            spawnedBoss = true;
            PostRoomGeneration(rooms[^1].gameObject, "boss");
            yield return null;
        }

        if (!spawnedStore)
        {
            if (Game.DungeonNumber > 4) { Game.PriceMultiplier += (rooms.Count / 5); }
            int storeRoomIndex = UnityEngine.Random.Range(Mathf.RoundToInt(rooms.Count * 0.25f), rooms.Count - 1);
            Instantiate(store, rooms[storeRoomIndex].transform.position, Quaternion.identity);
            spawnedStore = true;
            yield return null;
        }

        yield return StartCoroutine(SpawnSpecialRoomsAsync());

        if (!spawnedCasino)
        {
            yield return StartCoroutine(AttemptSpawnCasinoAsync());
        }

        if (!spawnedChestRooms && Game.DungeonNumber > 2)
        {
            yield return StartCoroutine(SpawnChestRoomsAsync());
            spawnedChestRooms = true;
        }

        if (!enemyRoomsComplete)
        {
            yield return StartCoroutine(SpawnEnemyRoomsAsync());
            enemyRoomsComplete = true;
        }

        if (!spawnedCauldron && Game.DungeonNumber > 3)
        {
            yield return StartCoroutine(SpawnCauldronRoomAsync());
            spawnedCauldron = true;
        }

        FindObjectOfType<RoomFurnitureSpawner>().AddFurnitureToRooms();
        isSettingUp = false;
        
    }

    private IEnumerator SpawnSpecialRoomsAsync()
    {
        foreach (var specialRoom in specialRooms)
        {
            if (specialRoom.hasSpawned || specialRoom.roomPrefab == null)
                continue;

            if (Game.DungeonNumber < specialRoom.minDungeonLevel || rooms.Count < specialRoom.minPerDungeon)
                continue;

            int spawnCount = 0;
            int maxSpawns = UnityEngine.Random.Range(specialRoom.minPerDungeon, specialRoom.maxPerDungeon + 1);

            if (UnityEngine.Random.value > specialRoom.spawnProbability)
                continue;

            List<int> availableRooms = new List<int>();
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                if (rooms[i] != null && !RoomContainsTaggedObject(rooms[i].transform.position, specialRoom.roomTag) &&
                    !IsRoomOccupied(rooms[i].transform.position))
                {
                    availableRooms.Add(i);
                }
                yield return null;
            }

            if (specialRoom.preferredLocation > 0 && specialRoom.preferredLocation < rooms.Count)
            {
                if (availableRooms.Contains(specialRoom.preferredLocation))
                {
                    Instantiate(specialRoom.roomPrefab, rooms[specialRoom.preferredLocation].transform.position, Quaternion.identity);
                    spawnCount++;
                    availableRooms.Remove(specialRoom.preferredLocation);
                }
            }

            availableRooms = availableRooms.OrderBy(x => UnityEngine.Random.value).ToList();

            for (int i = 0; i < maxSpawns - spawnCount && i < availableRooms.Count; i++)
            {
                Instantiate(specialRoom.roomPrefab, rooms[availableRooms[i]].transform.position, Quaternion.identity);
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
        for (int i = 4; i < rooms.Count - 1; i++)
        {
            if (rooms[i] != null && !RoomContainsTaggedObject(rooms[i].transform.position, "Store"))
            {
                availableRooms.Add(i);
            }
            yield return null;
        }

        if (availableRooms.Count > 0)
        {
            Instantiate(Casino, rooms[availableRooms[UnityEngine.Random.Range(0, availableRooms.Count)]].transform.position, Quaternion.identity);
            spawnedCasino = true;
        }
    }

    private IEnumerator SpawnChestRoomsAsync()
    {
        List<int> availableRooms = new List<int>();
        for (int i = 4; i < rooms.Count - 1; i++)
        {
            if (rooms[i] != null && !RoomContainsTaggedObject(rooms[i].transform.position, "Store") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "Chest") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "BossParent") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "Cauldron"))
            {
                availableRooms.Add(i);
            }
            yield return null;
        }

        int numChestRooms = Mathf.Clamp(UnityEngine.Random.Range(1, availableRooms.Count / 2), 1, 2);
        for (int i = 0; i < numChestRooms && availableRooms.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            Instantiate(chestRoom, rooms[availableRooms[randomIndex]].transform.position, Quaternion.identity);
            availableRooms.RemoveAt(randomIndex);
            yield return null;
        }
    }

    private IEnumerator SpawnCauldronRoomAsync()
    {
        List<int> availableRooms = new List<int>();
        for (int i = 4; i < rooms.Count - 1; i++)
        {
            if (rooms[i] != null && !RoomContainsTaggedObject(rooms[i].transform.position, "Store") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "Chest") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "BossParent") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "Cauldron"))
            {
                availableRooms.Add(i);
            }
            yield return null;
        }

        if (availableRooms.Count > 0)
        {
            Instantiate(Cauldron, rooms[availableRooms[UnityEngine.Random.Range(0, availableRooms.Count)]].transform.position, Quaternion.identity);
        }
    }

    public void PostRoomGeneration(GameObject room, string Tag)
    {
        WallChangeForCustomRooms wallChanger = room.GetComponentInChildren<WallChangeForCustomRooms>();
        if (wallChanger != null)
        {
            string roomTag = Tag;
            wallChanger.ApplyWallChangesForRoomType(roomTag);
        }
    }

    private IEnumerator SpawnEnemyRoomsAsync()
    {
        List<int> availableRoomIndices = new List<int>();
        for (int i = 1; i < rooms.Count - 1; i++)
        {
            if (rooms[i] != null && !RoomContainsTaggedObject(rooms[i].transform.position, "Store") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "Chest") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "BossParent") &&
               !RoomContainsTaggedObject(rooms[i].transform.position, "Cauldron"))
            {
                availableRoomIndices.Add(i);
            }
            yield return null;
        }

        Difficulty Diff = FindObjectOfType<Difficulty>();

        int numEnemyRooms = Mathf.Clamp(availableRoomIndices.Count, 1 + Mathf.FloorToInt((availableRoomIndices.Count / 3f) * Mathf.Log10(Mathf.Max(1, Game.DungeonNumber))), Mathf.CeilToInt((rooms.Count / 2f) + Mathf.FloorToInt(Mathf.Log10(10 + Game.DungeonNumber))));
        availableRoomIndices = availableRoomIndices.OrderBy(x => UnityEngine.Random.value).ToList();

        for (int i = 0; i < numEnemyRooms; i++)
        {
            i = Mathf.Clamp(i, 0, availableRoomIndices.Count - 1);
            Vector3 roomPosition = rooms[availableRoomIndices[i]].transform.position;

            bool isBossRoom = RoomContainsTaggedObject(roomPosition, "BossParent");
            bool isStoreRoom = RoomContainsTaggedObject(roomPosition, "Store");
            bool isChestRoom = RoomContainsTaggedObject(roomPosition, "Chest");
            bool isCauldronRoom = RoomContainsTaggedObject(roomPosition, "Cauldron");
            bool isSpecialRoom = specialRooms.Any(sr => RoomContainsTaggedObject(roomPosition, sr.roomTag));

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

            int enemyCount = UnityEngine.Random.Range(1, 2 + Mathf.RoundToInt(Mathf.Log(1 + Game.DungeonNumber)) + Mathf.RoundToInt(Diff.Dificulty * 0.5f));

            for (int j = 0; j < enemyCount; j++)
            {
                GameObject selectedEnemy = validEnemies[UnityEngine.Random.Range(0, validEnemies.Count)];
                int maxEnemies = enemySpawnManager.GetMaxEnemiesForConfig(selectedEnemy);

                if (j >= maxEnemies) break;

                Instantiate(
                    selectedEnemy,
                    roomPosition + new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-1.5f, 1.5f), 0f),
                    Quaternion.identity
                );

                yield return null;
            }
        }
    }

    bool RoomContainsTaggedObject(Vector3 position, string tag)
    {
        return Physics2D.OverlapCircleAll(position, 1f).Any(collider => collider.CompareTag(tag));
    }

    public void DeleteAllRooms(bool playerHasDied, bool preMadeDungeon = false, int dungeonPrefabID = 0)
    {
        Time.timeScale = 1;
        PrefabDungeon = false;
        player = FindFirstObjectByType<PlayerState>();
        SPS = FindObjectOfType<SpecialPlayerStats>();
        StopAllCoroutines();
        ResetDungeonState();

        if (FindObjectOfType<BossBar>() is BossBar bossBar) bossBar.HideBossBar();

        //Destroy(WC_);

        ClearAllGameObjects();

        if (!playerHasDied && !preMadeDungeon)
        {
            Game.NewDungeon();
            DungeonText?.ShowDungeonText();
        }

        Game ??= FindFirstObjectByType<GameManager>();

        if (playerHasDied)
        {
            ObjectMenu.GetComponent<ObjectStatsMenu>().ResetAllPowerUps();
            ResetGameState();
            SPS.ResetStats();
        }

        ResetPlayerPosition();

        if (!preMadeDungeon)
        {
            InstantiateNewEntryRoom();
            currentSpecialDungeonID = -1;
        }
        else
        {
            var existingEntry = GameObject.Find("EntryRoom");
            if (existingEntry) Destroy(existingEntry);

            LoadPreMadeDungeon(dungeonPrefabID);
            currentSpecialDungeonID = dungeonPrefabID;
        }

        VoidBoundEvents voidBoundEvents = FindObjectOfType<VoidBoundEvents>();
        if (!playerHasDied && voidBoundEvents != null && voidBoundEvents.HasActiveEffects() && !preMadeDungeon)
        {
            voidBoundEvents.OnDungeonEnterEffects();
        }

        ActivateHandAnimation();
        ResetEliteSystem();
    }

    public void ResetEliteSystem()
    {
        Difficulty difficultyManager = FindObjectOfType<Difficulty>();
        if (difficultyManager != null)
        {
            difficultyManager.OnNewDungeon();
        }
    }

    private void ResetDungeonState()
    {
        isSettingUp = false;
        enemyRoomsComplete = false;
        spawnedBoss = false;
        spawnedStore = false;
        spawnedCasino = false;
        spawnedChestRooms = false;
        spawnedCauldron = false;
        largeRoomsSpawned = 0;
        occupiedPositions.Clear();

        foreach (var specialRoom in specialRooms)
        {
            specialRoom.hasSpawned = false;
        }
    }

    private void ResetPlayerPosition()
    {
        player = FindAnyObjectByType<PlayerState>();
        player.transform.position = new Vector3(0, 0, player.transform.position.z);
        waitTime = 3.45f;
    }

    //private void SetupWeaponCollectible(bool playerHasDied)
    //{
    //   if (!playerHasDied)
    //   {
    // WC_ = Instantiate(WC, new Vector3(0, 1.5f, 0), Quaternion.identity);
    //}

    //if (WC_ != null)
    //      {
    //  WC_.GetComponent<WeaponCollectible>().SetWeapon(UnityEngine.Random.Range(0, WT.WeaponSprites.Length), false);
    // WC_.SetActive(false);
    //StartCoroutine(WeaponCollectilbeSetRandom());
    //}
    //}

    private void ActivateHandAnimation()
    {
        Hand.SetActive(true);
        Hand.GetComponent<HandScript>().ActivateAnimation();
    }

    private void LoadPreMadeDungeon(int dungeonPrefabID)
    {
        ClearAllGameObjects();
        Debug.Log(dungeonPrefabID);
        PrefabDungeon = true;
        Instantiate(PrefabDungeons[dungeonPrefabID], Vector2.zero, Quaternion.identity);
        switch (dungeonPrefabID)
        {
            case 0:
                DungeonText?.ShowDungeonText(false, "Void Binding Store");
                break;
        }
    }

    //private IEnumerator WeaponCollectilbeSetRandom()
    //{
    //yield return new WaitForSeconds(2f);
    //WC_.SetActive(true);
    //    WC_.GetComponent<WeaponCollectible>().SetWeapon(UnityEngine.Random.Range(0, WT.WeaponSprites.Length), false);
    //}

    private void ResetGameState()
    {
        VoidBinds = FindObjectOfType<VoidBoundEvents>();

        VoidBinds.ResetEvents();
        Game.DungeonNumber = 0;
        Game.PriceMultiplier = 1;
        Game.enemyDamageMultiplier = 1;
        Game.enemyHealthMultiplier = 1;
        Game.rangedEnemyDamageMultiplier = 1;
        FindObjectOfType<WeaponTypes>().CurrentWeaponId = 0;
        DungeonText.ShowDungeonText();
        PlayerPrefs.DeleteKey("VoidPortalSpawned");
    }

    private void ClearAllGameObjects()
    {
        foreach (var room in rooms.ToArray())
        {
            if (room != null)
            {
                Destroy(room);
                rooms.Remove(room);
            }
        }

        occupiedPositions.Clear();

        var tagsToClear = new[] {
            "BossParent", "Store", "Chest", "Cauldron",
            "Coin", "RedCoin", "BlueCoin", "TriangularCoin", "TriangularRedCoin", "SpecialRoom",
            "TriangularBlueCoin", "ClosedRoom", "Furniture", "MapIcon", "Room", "PreFabDungeon", "Portal"
        };

        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            if (obj != null && tagsToClear.Contains(obj.tag))
            {
                Destroy(obj);
            }
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy.transform.parent != null)
            {
                Destroy(enemy.transform.parent.gameObject);
            }
            else
            {
                Destroy(enemy);
            }
        }

        Transform[] allObjects = FindObjectsOfType<Transform>(true);
        List<GameObject> toDestroy = new List<GameObject>();

        foreach (Transform obj in allObjects)
        {
            if (obj.CompareTag("MapIcon"))
                toDestroy.Add(obj.gameObject);
        }

        foreach (GameObject icon in toDestroy)
        {
            Destroy(icon);
        }
    }

    private void InstantiateNewEntryRoom()
    {
        var existingEntry = GameObject.Find("EntryRoom");
        if (existingEntry != null) { Destroy(existingEntry); }

        rooms.Clear();
        occupiedPositions.Clear();
        var newEntry = Instantiate(EntryRoom, Vector2.zero, Quaternion.identity);
        newEntry.name = "EntryRoom";

        var chainsComponent = newEntry.GetComponent<EntryRoomChains>();
        if (chainsComponent != null)
        {
            chainsComponent.enabled = true;
            if (chainsComponent.Chains != null && !chainsComponent.Chains.activeSelf)
            {
                chainsComponent.Chains.SetActive(true);
            }
        }
        minimapController = FindAnyObjectByType<MinimapController>();
        if (minimapController != null)
        {
            minimapController.ClearMinimapIcons();
            minimapController.AddAdjacentRoomsToEntryRoom();
        }

        // Mark entry room position as occupied
        AddPositionAsOccupied(Vector2.zero);
    }

    public void AddRoom(GameObject room)
    {
        if (!rooms.Contains(room))
        {
            rooms.Add(room);
        }
    }

    // Add a method to mark positions as occupied (for large rooms)
    public void AddPositionAsOccupied(Vector2 position)
    {
        occupiedPositions.Add(position);
    }

    // Method to mark multiple positions as occupied (for large rooms)
    public void AddLargeRoomPositions(Vector2 centerPosition)
    {
        // Mark the 2x2 area as occupied
        AddPositionAsOccupied(centerPosition);
        AddPositionAsOccupied(centerPosition + new Vector2(1, 0));
        AddPositionAsOccupied(centerPosition + new Vector2(0, 1));
        AddPositionAsOccupied(centerPosition + new Vector2(1, 1));
    }

    // Check if the 2x2 area for a large room is available
    public bool IsAreaAvailableForLargeRoom(Vector2 position)
    {
        Vector2[] positionsToCheck = new Vector2[]
        {
            position,
            position + new Vector2(1, 0),
            position + new Vector2(0, 1),
            position + new Vector2(1, 1)
        };

        foreach (Vector2 pos in positionsToCheck)
        {
            if (occupiedPositions.Contains(pos) || IsRoomOccupied(pos))
            {
                return false;
            }
        }
        return true;
    }

    public bool CanSpawnLargeRoom(Vector2 position)
    {
        return largeRoomsSpawned < maxLargeRoomsPerDungeon &&
               UnityEngine.Random.value < largeRoomSpawnChance &&
               IsAreaAvailableForLargeRoom(position);
    }

    public void IncrementLargeRoomCount()
    {
        largeRoomsSpawned++;
    }

    public bool IsRoomOccupied(Vector3 roomPosition)
    {
        return occupiedPositions.Contains(new Vector2(roomPosition.x, roomPosition.y)) ||
               Physics2D.OverlapCircleAll(roomPosition, 1f).Any(collider =>
                   collider.CompareTag("Store") ||
                   collider.CompareTag("Cauldron") ||
                   collider.CompareTag("ClosedRoom") ||
                   collider.CompareTag("Enemy") ||
                   collider.CompareTag("Chest") ||
                   collider.CompareTag("BossParent"));
    }

    private bool HasValidRoomName(GameObject room)
    {
        return room.name.Length > 3;
    }

#if UNITY_EDITOR
    // Implementation of the custom editor
    [CustomEditor(typeof(RoomTemplates))]
    public class RoomTemplatesEditor : Editor
    {
        private SerializedProperty bottomRoomsProperty;
        private SerializedProperty topRoomsProperty;
        private SerializedProperty leftRoomsProperty;
        private SerializedProperty rightRoomsProperty;
        private SerializedProperty closedRoomsProperty;
        private SerializedProperty largeRoomsProperty;
        private SerializedProperty largeRoomSpawnChanceProperty;
        private SerializedProperty maxLargeRoomsPerDungeonProperty;
        private SerializedProperty entryRoomProperty;
        private SerializedProperty bossProperty;
        private SerializedProperty chestRoomProperty;
        private SerializedProperty storeProperty;
        private SerializedProperty casinoProperty;
        private SerializedProperty cauldronProperty;
        private SerializedProperty specialRoomsProperty;
        private SerializedProperty minimumRoomsToGenerateProperty;
        private SerializedProperty enemySpawnManagerProperty;

        private bool showRoomSettings = true;
        private bool showLargeRoomSettings = true;
        private bool showSpecialRoomSettings = true;
        private bool showEnemySettings = true;

        public Editor enemySpawnManagerEditor;

        private void OnEnable()
        {
            bottomRoomsProperty = serializedObject.FindProperty("bottomRooms");
            topRoomsProperty = serializedObject.FindProperty("topRooms");
            leftRoomsProperty = serializedObject.FindProperty("leftRooms");
            rightRoomsProperty = serializedObject.FindProperty("rightRooms");
            closedRoomsProperty = serializedObject.FindProperty("closedRooms");
            largeRoomsProperty = serializedObject.FindProperty("largeRooms");
            largeRoomSpawnChanceProperty = serializedObject.FindProperty("largeRoomSpawnChance");
            maxLargeRoomsPerDungeonProperty = serializedObject.FindProperty("maxLargeRoomsPerDungeon");
            entryRoomProperty = serializedObject.FindProperty("EntryRoom");
            bossProperty = serializedObject.FindProperty("boss");
            chestRoomProperty = serializedObject.FindProperty("chestRoom");
            storeProperty = serializedObject.FindProperty("store");
            casinoProperty = serializedObject.FindProperty("Casino");
            cauldronProperty = serializedObject.FindProperty("Cauldron");
            specialRoomsProperty = serializedObject.FindProperty("specialRooms");
            minimumRoomsToGenerateProperty = serializedObject.FindProperty("minimumRoomsToGenerate");
            enemySpawnManagerProperty = serializedObject.FindProperty("enemySpawnManager");

            RoomTemplates roomTemplates = (RoomTemplates)target;
            if (roomTemplates.enemySpawnManager != null)
            {
                // Use CreateEditor with the component type, not the editor type
                enemySpawnManagerEditor = CreateEditor(roomTemplates.enemySpawnManager);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RoomTemplates roomTemplates = (RoomTemplates)target;

            EditorGUILayout.Space();

            showRoomSettings = EditorGUILayout.Foldout(showRoomSettings, "Room Generation Settings", true, EditorStyles.foldoutHeader);
            if (showRoomSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(minimumRoomsToGenerateProperty);
                EditorGUILayout.PropertyField(entryRoomProperty);
                EditorGUILayout.PropertyField(bottomRoomsProperty, true);
                EditorGUILayout.PropertyField(topRoomsProperty, true);
                EditorGUILayout.PropertyField(leftRoomsProperty, true);
                EditorGUILayout.PropertyField(rightRoomsProperty, true);
                EditorGUILayout.PropertyField(closedRoomsProperty, true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Ensure this section is properly drawn
            showLargeRoomSettings = EditorGUILayout.Foldout(showLargeRoomSettings, "Large Room Settings", true, EditorStyles.foldoutHeader);
            if (showLargeRoomSettings)
            {
                EditorGUI.indentLevel++;
                // Make sure these properties exist and are correctly initialized
                EditorGUILayout.PropertyField(largeRoomsProperty, new GUIContent("Large Rooms"), true);
                EditorGUILayout.PropertyField(largeRoomSpawnChanceProperty, new GUIContent("Spawn Chance"));
                EditorGUILayout.PropertyField(maxLargeRoomsPerDungeonProperty, new GUIContent("Max Per Dungeon"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showSpecialRoomSettings = EditorGUILayout.Foldout(showSpecialRoomSettings, "Special Room Settings", true, EditorStyles.foldoutHeader);
            if (showSpecialRoomSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(specialRoomsProperty, true);
                EditorGUILayout.PropertyField(bossProperty);
                EditorGUILayout.PropertyField(storeProperty);
                EditorGUILayout.PropertyField(chestRoomProperty);
                EditorGUILayout.PropertyField(casinoProperty);
                EditorGUILayout.PropertyField(cauldronProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showEnemySettings = EditorGUILayout.Foldout(showEnemySettings, "Enemy Spawn Settings", true, EditorStyles.foldoutHeader);
            if (showEnemySettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(enemySpawnManagerProperty);

                if (roomTemplates.enemySpawnManager != null)
                {
                    if (enemySpawnManagerEditor == null)
                    {
                        enemySpawnManagerEditor = CreateEditor(roomTemplates.enemySpawnManager);
                    }

                    EditorGUI.BeginChangeCheck();
                    enemySpawnManagerEditor.OnInspectorGUI();
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(roomTemplates.enemySpawnManager);
                    }
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            if (enemySpawnManagerEditor != null)
            {
                DestroyImmediate(enemySpawnManagerEditor);
            }
        }
    }
#endif
}

[Serializable]
public class SpecialRoom
{
    public GameObject roomPrefab;        // The prefab for this special room
    public string roomTag;               // Tag to identify this room type
    [Range(0f, 1f)]
    public float spawnProbability = 0.5f; // Probability of this room type spawning (0-1)
    public int minPerDungeon = 0;        // Minimum number of this room to spawn
    public int maxPerDungeon = 1;        // Maximum number of this room to spawn
    public int minDungeonLevel = 0;      // Minimum dungeon level required for this room to spawn
    [Tooltip("0 = Anywhere, 1 = Early Dungeon, 2 = Mid Dungeon, 3 = Late Dungeon")]
    public int preferredLocation = 0;    // Where in the dungeon this room should spawn
    [HideInInspector]
    public bool hasSpawned = false;      // Track if this room has already spawned
}