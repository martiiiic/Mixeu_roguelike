using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    public int RangedDamage;
    private PlayerState Player;
    private GameManager manager;

    private void Awake()
    {
        RangedDamage = 1;
        manager = FindFirstObjectByType<GameManager>();
        Player = FindObjectOfType<PlayerState>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other != null && !Player.Invulnerable)
        {
            Player.TakeDamage(RangedDamage * manager.rangedEnemyDamageMultiplier);
            Destroy(this.gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Mathf.Abs(this.gameObject.transform.position.x - Player.transform.position.x) > 20 || Mathf.Abs(this.gameObject.transform.position.y - Player.transform.position.y) > 20)
        {
            Destroy(this.gameObject);
        }
    }
}
