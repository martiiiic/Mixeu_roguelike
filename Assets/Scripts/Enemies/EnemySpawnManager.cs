using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyPrefabConfig
{
    public GameObject enemyPrefab;
    public int minDungeonLevel = 1;
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

    public List<GameObject> GetValidEnemiesForRoom(Vector3 roomPosition, bool isBossRoom, bool isStoreRoom, bool isChestRoom, bool isCauldronRoom, bool isSpecialRoom, int specialDungeonID = -1)
    {
        List<GameObject> validEnemies = new List<GameObject>();

        foreach (var config in enemyConfigs)
        {
            if (config.enemyPrefab == null) continue;

            if (gameManager.DungeonNumber < config.minDungeonLevel) continue;

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

        return 3; // Default value
    }
}