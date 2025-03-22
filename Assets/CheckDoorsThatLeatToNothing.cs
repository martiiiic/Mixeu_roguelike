using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CheckDoorsThatLeadToNothing : MonoBehaviour
{
    public GameObject Wall;
    private TilemapRenderer Renderer;
    private bool wallDetected = false;
    private RoomTemplates roomTemplates;

    private void Start()
    {
        Renderer = GetComponent<TilemapRenderer>();
        if (Renderer != null)
            Renderer.enabled = false;

        roomTemplates = FindObjectOfType<RoomTemplates>();

        if (Wall != null)
            Wall.SetActive(true);

        StartCoroutine(CheckBossSpawned());
    }

    private IEnumerator CheckBossSpawned()
    {
        while (roomTemplates == null)
        {
            roomTemplates = FindObjectOfType<RoomTemplates>();
            yield return new WaitForSeconds(0.2f);
        }

        while (!roomTemplates.spawnedBoss)
        {
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.6f);

        // Perform a final check for walls if not already detected
        if (!wallDetected)
        {
            // Cast rays or use OverlapBox to check for walls
            wallDetected = CheckForWallsNearby();
        }

        // Apply the final decision
        if (!wallDetected)
        {
            if (Wall != null)
                Destroy(Wall);
        }
        else
        {
            if (Wall != null)
                Wall.SetActive(true);
            if (Renderer != null)
                Renderer.enabled = true;
        }
    }

    private bool CheckForWallsNearby()
    {
        string wallName = gameObject.name;

        if (wallName.Contains("Wall1"))
        {
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.5f);
            if (hitLeft.collider != null && (hitLeft.collider.CompareTag("Wall") || hitLeft.collider.CompareTag("BreakableWall")))
                return true;
        }
        else if (wallName.Contains("Wall2"))
        {
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 0.5f);
            if (hitRight.collider != null && (hitRight.collider.CompareTag("Wall") || hitRight.collider.CompareTag("BreakableWall")))
                return true;
        }
        else if (wallName.Contains("Wall3"))
        {
            RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, 0.5f);
            if (hitDown.collider != null && (hitDown.collider.CompareTag("Wall") || hitDown.collider.CompareTag("BreakableWall")))
                return true;
        }
        else if (wallName.Contains("Wall4"))
        {
            RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, 0.5f);
            if (hitUp.collider != null && (hitUp.collider.CompareTag("Wall") || hitUp.collider.CompareTag("BreakableWall")))
                return true;
        }
        else
        {
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 0.5f);
            if (hitRight.collider != null && (hitRight.collider.CompareTag("Wall") || hitRight.collider.CompareTag("BreakableWall")))
                return true;

            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.5f);
            if (hitLeft.collider != null && (hitLeft.collider.CompareTag("Wall") || hitLeft.collider.CompareTag("BreakableWall")))
                return true;

            RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, 0.5f);
            if (hitUp.collider != null && (hitUp.collider.CompareTag("Wall") || hitUp.collider.CompareTag("BreakableWall")))
                return true;

            RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, 0.5f);
            if (hitDown.collider != null && (hitDown.collider.CompareTag("Wall") || hitDown.collider.CompareTag("BreakableWall")))
                return true;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("BreakableWall"))
        {
            wallDetected = true;
        }
    }
}