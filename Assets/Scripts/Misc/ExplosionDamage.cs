using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    private PlayerState Player;
    private PlayerStats stats;
    private GameManager GameManager;
    private SecretRoomBreakableWalls BreakableWall;

    public AudioClip discovery;

    public float knockbackForce = 10f;
    public float explosionRadius = 3f;
    public int explosionDamage = 5;

    private void Awake()
    {
        StartCoroutine(StopExplosionRadius(0.35f));
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameManager = FindObjectOfType<GameManager>();
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = other.GetComponentInParent<Enemy>();
            }
            if (enemy == null)
            {
                enemy = other.GetComponentInChildren<Enemy>();
            }

            if (enemy != null)
            {
                float distance = Vector3.Distance(enemy.transform.position, gameObject.transform.position);
                distance = Mathf.Abs(distance);
                enemy.TakeDamage(Mathf.RoundToInt(explosionDamage / distance) * GameManager.enemyHealthMultiplier, Vector2.zero);
            }
        }
        if (other.CompareTag("BreakableWall"))
        {
            BreakableWall = other.GetComponent<SecretRoomBreakableWalls>();
            if (BreakableWall != null && BreakableWall.Destroyable)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.clip = discovery;
                source.Play();
                BreakableWall.DestroyWallOnImpact();
            }
        }

        if (other.CompareTag("Player"))
        {
            stats = other.GetComponent<PlayerStats>();
            Player = other.GetComponent<PlayerState>();
            if (Player != null && stats != null)
            {
                Player.TakeDamage(Mathf.RoundToInt(stats.Defense));
            }
        }
    }

    private IEnumerator StopExplosionRadius(float time)
    {
        yield return new WaitForSeconds(0.05f);
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
