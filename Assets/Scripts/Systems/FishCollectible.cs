using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FishCollectible : MonoBehaviour
{
    private PlayerStats stats;
    private CoinSystem coin;
    private GameManager gameManager;
    public GameObject Fish;

    private VoidBoundEvents VoidBind;

    private void Awake()
    {
        VoidBind = FindObjectOfType<VoidBoundEvents>();
        gameManager = FindObjectOfType<GameManager>();
        stats = FindObjectOfType<PlayerStats>();
    }
    public void SpawnFish(int Amount, Transform Position)
    {
        Vector2 spawnPosition = new Vector2(
                Position.position.x + Random.Range(-1f, 1f),
                Position.position.y + Random.Range(-1f, 1f)
                );

        for (int i = 0; i < Amount; i++)
        {
            GameObject spawnedFish = Instantiate(Fish, spawnPosition, Quaternion.identity);
            StartCoroutine(DispawnCollectible(spawnedFish, 15f));
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        VoidBind = FindObjectOfType<VoidBoundEvents>();
            if (other.CompareTag("Player") && stats.CurrentHealth != stats.MaxHealth && !VoidBind.IsEffectActive("Kidney"))
            {
                stats.CurrentHealth += 1;
                Destroy(gameObject);
            }
            else if (other.CompareTag("Player") && stats.CurrentHealth == stats.MaxHealth)
            {
                coin = FindObjectOfType<CoinSystem>();
                coin.AddCoins(gameManager.PriceMultiplier);
                Destroy(gameObject);
            }
        
    }
    private IEnumerator DispawnCollectible(GameObject Fish, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (this.Fish != null && this.CompareTag("Collectible"))
        {
            Destroy(Fish);
        }
        else
        {
            this.StopAllCoroutines();
        }
    }

}
