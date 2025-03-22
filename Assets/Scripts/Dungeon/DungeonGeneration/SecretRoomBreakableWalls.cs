using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class SecretRoomBreakableWalls : MonoBehaviour
{
    public int WallId;
    private Vector3 position;
    public bool Destroyable;
    private BoxCollider2D Collider;
    public GameObject SubWall;

    public Tilemap Tiles;

    public void DestroyWallOnImpact()
    {
        if(Destroyable && SubWall != null)
        {
            Destroy(gameObject);
            Destroy(SubWall);
        }
    }

    private void Awake()
    {
        Collider = GetComponent<BoxCollider2D>();
        position = gameObject.transform.position;
        Destroyable = false;
        StartCoroutine(SetColliderToFalse(1.25f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Wall"))
        {
            StopAllCoroutines();
            Collider.enabled = false;
            Destroyable = true;
        }
        if(other.CompareTag("WallBreaker") && Destroyable == true)
        {
            Destroy(gameObject);
            Destroy(SubWall);
        }
    }

    private IEnumerator SetColliderToFalse(float time)
    {
        yield return new WaitForSeconds(time);
        Collider.enabled = false;

        if (SubWall != null)
        {
            Tilemap tileMap = SubWall.GetComponent<Tilemap>();
            if (tileMap != null)
            {
                Vector3Int tilePos = Vector3Int.zero;

                switch (WallId)
                {
                    case 1: tilePos = new Vector3Int(0, 0, 0); break;
                    case 2: tilePos = new Vector3Int(1, 1, 0); break;
                    case 3: tilePos = new Vector3Int(3, 1, 0); break;
                    case 4: tilePos = new Vector3Int(2, 1, 0); break;
                }
                tileMap.SetTile(tilePos, null);
                tileMap.RefreshTile(tilePos);

                tileMap.RefreshAllTiles();
            }
        }

        gameObject.transform.position = position;
    }
}