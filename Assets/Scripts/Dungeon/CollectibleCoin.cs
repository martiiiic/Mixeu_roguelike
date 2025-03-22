using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleCoin : MonoBehaviour, IDisposable
{
    public GameObject Coin;
    public GameObject RedCoin;
    public GameObject BlueCoin;
    public GameObject TriangularCoin;
    public GameObject TriangularRedCoin;
    public GameObject TriangularBlueCoin;
    private Rigidbody2D rb;
    private CoinSystem coinSystem;
    private bool disposed = false;
    public static Vector3 originalCoinPosition = new Vector3(1000f, 0f, 0f);

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        coinSystem = FindObjectOfType<CoinSystem>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            string coinTag = transform.parent.gameObject.tag;
            Debug.Log($"Collided with tag: {coinTag}");
            if (coinTag == "Coin")
            {
                coinSystem.AddCoins(1);
            }
            else if (coinTag == "RedCoin")
            {
                coinSystem.AddCoins(10);
            }
            else if (coinTag == "BlueCoin")
            {
                coinSystem.AddCoins(100);
            }
            else if (coinTag == "TriangularCoin")
            {
                coinSystem.AddCoins(1000);
            }
            else if (coinTag == "TriangularRedCoin")
            {
                coinSystem.AddCoins(10000);
            }
            else if (coinTag == "TriangularBlueCoin")
            {
                coinSystem.AddCoins(100000);
            }
            if (transform.position != originalCoinPosition)
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }

    public void SpawnCoin(int totalAmount, Transform position)
    {
        int numTriangularBlueCoins = totalAmount / 100000;
        totalAmount %= 100000;
        int numTriangularRedCoins = totalAmount / 10000;
        totalAmount %= 10000;
        int numTriangularCoins = totalAmount / 1000;
        totalAmount %= 1000;
        int numBlueCoins = totalAmount / 100;
        totalAmount %= 100;
        int numRedCoins = totalAmount / 10;
        totalAmount %= 10;
        SpawnSpecificCoins(TriangularBlueCoin, numTriangularBlueCoins, position);
        SpawnSpecificCoins(TriangularRedCoin, numTriangularRedCoins, position);
        SpawnSpecificCoins(TriangularCoin, numTriangularCoins, position);
        SpawnSpecificCoins(BlueCoin, numBlueCoins, position);
        SpawnSpecificCoins(RedCoin, numRedCoins, position);
        SpawnSpecificCoins(Coin, totalAmount, position);
    }

    private void SpawnSpecificCoins(GameObject coinPrefab, int amount, Transform position)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 spawnPosition = new Vector2(
                position.position.x + UnityEngine.Random.Range(-1f, 1f),
                position.position.y + UnityEngine.Random.Range(-1f, 1f)
            );
            GameObject spawnedCoin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DispawnCoinAfterDelay(spawnedCoin, 20f + (amount * 0.01f)));
        }
    }

    private IEnumerator DispawnCoinAfterDelay(GameObject coin, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (coin != null && coin.transform.position != originalCoinPosition)
        {
            Destroy(coin);
        }
    }

    // IDisposable implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                StopAllCoroutines();
            }

            // Clean up unmanaged resources
            // None in this case, but you would put that cleanup here

            disposed = true;
        }
    }

    // Finalizer
    ~CollectibleCoin()
    {
        Dispose(false);
    }

    // Unity's OnDestroy can call Dispose
    private void OnDestroy()
    {
        Dispose();
    }
}