using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class HotZone : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Vector2 detectionSize = new Vector2(5, 5);
    private BoxCollider2D zoneCollider;
    private GameManager manager;
    private bool playerInZone = false;
    private Transform playerTransform;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>();
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.size = detectionSize;
        if (enemy == null)
        {
            enemy = transform.parent.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError("No Enemy component found for HotZone: " + gameObject.name);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && enemy != null)
        {
            playerInZone = true;
            playerTransform = other.transform;
            zoneCollider.size = detectionSize * 1.5f;
            StartCoroutine(PlayerDetectionRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && enemy != null)
        {
            playerInZone = false;
            enemy.target = null;
            playerTransform = null;
            zoneCollider.size = detectionSize;
            StopAllCoroutines();
        }
    }

    private IEnumerator PlayerDetectionRoutine()
    {
        float detectionInterval = 0.8f / Mathf.Log10(10 + manager.DungeonNumber);
        if (detectionInterval < 0.1f) { detectionInterval = 0.1f; }

        while (playerInZone)
        {
            yield return new WaitForSeconds(detectionInterval);

            if (enemy == null || playerTransform == null)
                yield break;

            if (!enemy.IsWallBetweenEnemyAndPlayer())
            {
                enemy.target = playerTransform;
            }
            else
            {
                enemy.target = null;
            }
        }
    }
}