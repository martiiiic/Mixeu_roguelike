using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFurnitureSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FurnitureSet
    {
        public string categoryName;
        public GameObject[] prefabs;
        [Range(0f, 1f)]
        public float spawnProbability = 0.7f;
        [Range(1, 5)]
        public int maxItemsPerRoom = 3;
    }

    public FurnitureSet[] furnitureSets;

    [Header("Spawn Settings")]
    [Range(0f, 5f)]
    public float spawnRadius = 3f;

    [Range(0f, 1f)]
    public float overlapAllowance = 0.3f;

    private RoomTemplates roomTemplates;
    private List<Vector3> spawnedPositions = new List<Vector3>();

    private void Start()
    {
        roomTemplates = FindObjectOfType<RoomTemplates>();
    }

    public void AddFurnitureToRooms()
    {
        if (roomTemplates == null || roomTemplates.rooms == null)
        {
            Debug.LogError("RoomTemplates reference is null!");
            return;
        }

        foreach (GameObject room in roomTemplates.rooms)
        {
            if (room == null || !IsEligibleForFurniture(room)) continue;

            // Reset spawn positions for each room
            spawnedPositions.Clear();

            // Iterate through furniture sets
            foreach (FurnitureSet set in furnitureSets)
            {
                // Determine how many items to spawn based on probability
                int itemsToSpawn = Random.value < set.spawnProbability
                    ? Random.Range(1, set.maxItemsPerRoom + 1)
                    : 0;

                for (int i = 0; i < itemsToSpawn; i++)
                {
                    if (set.prefabs.Length == 0) continue;

                    GameObject furniturePrefab = set.prefabs[Random.Range(0, set.prefabs.Length)];
                    if (furniturePrefab == null) continue;

                    Vector3 spawnPosition = GetUniqueSpawnPosition(room);
                    if (spawnPosition != Vector3.zero)
                    {
                        Instantiate(furniturePrefab, spawnPosition, Quaternion.identity);
                    }
                }
            }
        }
    }

    private Vector3 GetUniqueSpawnPosition(GameObject room)
    {
        Vector3 roomCenter = room.transform.position;
        int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate random position within spawn radius
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 potentialPosition = roomCenter + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Check if position is too close to existing spawns
            bool isTooClose = false;
            foreach (Vector3 existingPosition in spawnedPositions)
            {
                if (Vector3.Distance(potentialPosition, existingPosition) < spawnRadius * overlapAllowance)
                {
                    isTooClose = true;
                    break;
                }
            }

            // Raycast to ensure not spawning in walls
            RaycastHit2D wallCheck = Physics2D.Raycast(potentialPosition, Vector2.zero, 0.1f, LayerMask.GetMask("Wall"));

            if (!isTooClose && wallCheck.collider == null)
            {
                spawnedPositions.Add(potentialPosition);
                return potentialPosition;
            }
        }

        return Vector3.zero;
    }

    private bool IsEligibleForFurniture(GameObject room)
    {
        if (room == null) return false;

        // Exclude specific room types
        string[] excludedTags = { "Store", "Boss", "Casino", "Chest", "Cauldron" };
        foreach (string tag in excludedTags)
        {
            if (room.CompareTag(tag)) return false;
        }

        // Additional room eligibility checks can be added here
        return true;
    }
}