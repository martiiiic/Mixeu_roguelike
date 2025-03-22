using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyPrefabConfig
{
    public GameObject enemyPrefab;
    public int minDungeonLevel = 0;
    public float spawnProbability = 0.5f;
    public bool spawnInBossRooms = false;
    public bool spawnInStoreRooms = false;
    public bool spawnInChestRooms = false;
    public bool spawnInCauldronRooms = false;
    public bool spawnInSpecialRooms = false;
    public int maxEnemiesPerRoom = 3;
    public List<int> compatibleSpecialDungeons = new List<int>();
    public List<string> incompatibleEnemyTags = new List<string>();
}

public class EnemySpawnManager : MonoBehaviour
{
    public List<EnemyPrefabConfig> enemyConfigs = new List<EnemyPrefabConfig>();
    private GameManager gameManager;
    private Difficulty difficultyManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        difficultyManager = FindObjectOfType<Difficulty>();
    }

    public IEnumerator SpawnEnemiesInRoomAsync(GameObject room, bool isBossRoom = false, bool isStoreRoom = false,
    bool isChestRoom = false, bool isCauldronRoom = false, bool isSpecialRoom = false, int specialDungeonID = -1)
    {
        // Skip if room is null
        if (room == null) yield break;

        Vector3 roomPosition = room.transform.position;

        // Get a list of valid enemies for this room type
        List<GameObject> validEnemies = GetValidEnemiesForRoom(
            roomPosition,
            isBossRoom,
            isStoreRoom,
            isChestRoom,
            isCauldronRoom,
            isSpecialRoom,
            specialDungeonID
        );

        // If no valid enemies, exit
        if (validEnemies.Count == 0) yield break;

        // Check if room size is appropriate for spawning enemies
        RoomSizeDetector roomSizeDetector = FindObjectOfType<RoomSizeDetector>();
        if (roomSizeDetector != null && !roomSizeDetector.CanPlaceEnemiesInRoom(room))
        {
            yield break;
        }

        // Select a random enemy type from valid ones
        GameObject enemyPrefab = validEnemies[UnityEngine.Random.Range(0, validEnemies.Count)];

        // Determine how many enemies to spawn
        int maxEnemies = GetMaxEnemiesForConfig(enemyPrefab);
        int enemyCount = UnityEngine.Random.Range(1, maxEnemies + 1);

        // Apply difficulty scaling if available
        if (difficultyManager != null)
        {
            enemyCount = Mathf.RoundToInt(enemyCount * difficultyManager.GetEnemyCountMultiplier());
        }

        // Get room dimensions to calculate spawn positions
        float roomWidth = 8f;
        float roomHeight = 8f;

        // If room has a collider, use its bounds
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        if (roomCollider != null)
        {
            Bounds bounds = roomCollider.bounds;
            roomWidth = bounds.size.x * 0.8f; // Use 80% of room width to keep enemies away from walls
            roomHeight = bounds.size.y * 0.8f;
        }

        // Spawn enemies
        for (int i = 0; i < enemyCount; i++)
        {
            // Calculate random position within room
            float offsetX = UnityEngine.Random.Range(-roomWidth / 2, roomWidth / 2);
            float offsetY = UnityEngine.Random.Range(-roomHeight / 2, roomHeight / 2);
            Vector3 spawnPos = new Vector3(
                roomPosition.x + offsetX,
                roomPosition.y + offsetY,
                roomPosition.z
            );

            // Spawn the enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Optional: Tag the enemy for future reference
            enemy.tag = "Enemy";

            // Small delay between spawns to prevent performance hitches
            yield return new WaitForSeconds(0.05f);
        }
    }

    public List<GameObject> GetValidEnemiesForRoom(Vector3 roomPosition, bool isBossRoom, bool isStoreRoom, bool isChestRoom, bool isCauldronRoom, bool isSpecialRoom, int specialDungeonID = -1)
    {
        List<GameObject> validEnemies = new List<GameObject>();
        foreach (var config in enemyConfigs)
        {
            if (config.enemyPrefab == null) continue;
            if (gameManager != null && gameManager.DungeonNumber < config.minDungeonLevel) continue;
            if ((isBossRoom && !config.spawnInBossRooms) ||
                (isStoreRoom && !config.spawnInStoreRooms) ||
                (isChestRoom && !config.spawnInChestRooms) ||
                (isCauldronRoom && !config.spawnInCauldronRooms) ||
                (isSpecialRoom && !config.spawnInSpecialRooms))
                continue;
            if (specialDungeonID >= 0 && !config.compatibleSpecialDungeons.Contains(specialDungeonID))
                continue;
            if (UnityEngine.Random.value <= config.spawnProbability)
            {
                bool hasIncompatibleEnemy = false;
                foreach (string tag in config.incompatibleEnemyTags)
                {
                    if (Physics2D.OverlapCircleAll(roomPosition, 5f).Length > 0 &&
                        Array.Exists(Physics2D.OverlapCircleAll(roomPosition, 5f),
                            collider => collider.CompareTag(tag)))
                    {
                        hasIncompatibleEnemy = true;
                        break;
                    }
                }
                if (!hasIncompatibleEnemy)
                {
                    validEnemies.Add(config.enemyPrefab);
                }
            }
        }
        return validEnemies;
    }

    public int GetMaxEnemiesForConfig(GameObject enemyPrefab)
    {
        foreach (var config in enemyConfigs)
        {
            if (config.enemyPrefab == enemyPrefab)
            {
                return config.maxEnemiesPerRoom;
            }
        }
        return 3;
    }
}