using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimizer : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();
    private RoomTemplates roomTemplates;

    // Cache these to avoid allocations
    private Enemy[] enemyCache = new Enemy[100];

    // Add variables to control how often we perform the optimization
    [SerializeField] private float optimizationInterval = 2.0f;
    private float nextOptimizationTime = 0f;

    private void Start()
    {
        // Find this only once
        roomTemplates = FindObjectOfType<RoomTemplates>();

        enemyCache = new Enemy[100];

    }

    private void Update()
    {
        // Only perform optimization at specific intervals
        if (Time.time >= nextOptimizationTime)
        {
            OptimizeEnemies();
            nextOptimizationTime = Time.time + optimizationInterval;
        }
    }
    public void RegisterEnemy(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    private void OptimizeEnemies()
    {
        // Don't find the room templates every frame
        if (roomTemplates == null)
        {
            roomTemplates = FindObjectOfType<RoomTemplates>();
            if (roomTemplates == null) return;
        }

        // Remove any null references from the list
        enemies.RemoveAll(e => e == null);

        // Calculate maximum allowed enemies
        int maxAllowedEnemies = 30 + roomTemplates.rooms.Count;

        // Remove excess enemies
        while (enemies.Count > maxAllowedEnemies)
        {
            Enemy excessEnemy = enemies[enemies.Count - 1];
            enemies.RemoveAt(enemies.Count - 1);
            if (excessEnemy != null && excessEnemy.gameObject != null)
            {
                Destroy(excessEnemy.gameObject);
            }
        }
    }
}