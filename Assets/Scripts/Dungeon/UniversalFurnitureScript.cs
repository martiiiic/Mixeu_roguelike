using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalFurnitureScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private AudioSource Source;
    public AudioClip[] Sounds;


    private Color OriginalColor;
    private int Health;
    public int MaxHealth;

    void Awake()
    {
        Source = gameObject.AddComponent<AudioSource>();
        Health = MaxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        OriginalColor = spriteRenderer.color;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (spriteRenderer != null && playerTransform != null)
        {
            float relativeY = transform.position.y - playerTransform.position.y;
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-relativeY * 100);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            Health--;
            StartCoroutine(FlashWhite(1));
        }
        else if (collision.GetComponent<ProjectileDamage>() != null || collision.GetComponent<PlayerArrow>() != null)
        {
            Health--;
            StartCoroutine(FlashWhite(1));
            Destroy(collision.gameObject);
        }
        else if (collision.GetComponent<ExplosionDamage>() != null)
        {
            Health--;
            StartCoroutine(FlashWhite(1));
        }
    }

    private IEnumerator FlashWhite(int damage)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSecondsRealtime(0.05f);


        spriteRenderer.color = OriginalColor;

        if(Health <= 0)
        {
            if(gameObject.GetComponent<BoxFurniture>() != null)
            {
                BoxFurniture B = GetComponent<BoxFurniture>();
                B.DestroyBox();
            }
            Destroy(gameObject);
        }
    }
}
