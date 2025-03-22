using System.Collections;
using UnityEngine;

public class BossHitBox : MonoBehaviour
{
    public int BossId;
    private BoxCollider2D hitBox;
    private BossScript BS;

    private void Awake()
    {
        BS = GetComponentInParent<BossScript>();
    }

    public void ChooseBossHitBox(int BossId)
    {
        // Remove any existing collider first
        BoxCollider2D existingCollider = GetComponent<BoxCollider2D>();
        if (existingCollider != null)
        {
            Destroy(existingCollider);
        }

        hitBox = gameObject.AddComponent<BoxCollider2D>();
        hitBox.isTrigger = true;

        switch (BossId)
        {
            case 0: // Flesh Moth
                hitBox.size = new Vector2(3, 3);
                break;
            case 1: // Second Boss
                hitBox.size = new Vector2(4, 4);
                break;
            case 2: // Third Boss
                hitBox.size = new Vector2(5, 5);
                break;
            default:
                hitBox.size = new Vector2(3, 3);
                break;
        }
    }

    private Coroutine damageCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerState player = other.GetComponent<PlayerState>();
            if (player != null && damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(InflictDamage(player));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator InflictDamage(PlayerState player)
    {
        while (true)
        {
            player.TakeDamage(BS.AttackDamage);
            yield return new WaitForSeconds(1f);
        }
    }
}