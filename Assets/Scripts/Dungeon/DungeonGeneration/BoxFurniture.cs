using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxFurniture : MonoBehaviour
{
    public CollectibleCoin cc;
    public FishCollectible fc;
    public GameObject[] enemies;

    private GameManager manager;

    private void Awake()
    {
        
        fc = FindObjectOfType<FishCollectible>();
        cc = FindObjectOfType<CollectibleCoin>();
        manager = FindObjectOfType<GameManager>();
    }

    public void DestroyBox()
    { 
            int randomint = UnityEngine.Random.Range(-8, 51);
            if (randomint > 31 && randomint < 39)
            {
                fc.SpawnFish(1, gameObject.transform);
            }
            if (randomint >= 39 && randomint < 45)
            {
                Instantiate(enemies[UnityEngine.Random.Range(0, enemies.Length)], new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 0.5f, gameObject.transform.position.z), Quaternion.identity);
                return;
            }
            if (randomint >= 45 && randomint != 50)
            {
                cc.SpawnCoin(UnityEngine.Random.Range(1 * manager.PriceMultiplier, 5 * manager.PriceMultiplier), gameObject.transform);
            }
            if (randomint == 50)
            {
                cc.SpawnCoin(UnityEngine.Random.Range(5 * manager.PriceMultiplier, 10 * manager.PriceMultiplier), gameObject.transform);
            }
            else
            {
                return;
            }
        }
    }

