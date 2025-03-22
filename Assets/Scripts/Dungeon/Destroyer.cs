using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    public float waitTime = 7f;

    private void Awake()
    {
        waitTime = 8f;
        Destroy(gameObject, waitTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        List<string> validTags = new List<string> {
    "RoomTrigger", "Cauldron", "Collectible", "PreFabDungeon", "EntryRoom",
    "Untagged", "ExitHand", "Bomb", "WallBreaker", "Arrow", "Hand",
    "BossParent", "Chest", "Player", "HotZone", "Weapon", "Boss",
    "Furniture", "Enemy", "Store"
    };

        if (other.CompareTag("ClosedRoom"))
        {
            Destroy(other.transform.root.gameObject);

        }
        if (!validTags.Contains(other.tag))
        {
            Destroy(other.gameObject);
        }
    }
}
