using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerArrow : MonoBehaviour
{
    public GameObject DamageIndicator;

    private Enemy Enemy;
    private SpriteRenderer SpriteRenderer;
    public Sprite[] ArrowsSprites;
    private WeaponTypes WeaponTypes;
    private PlayerStats Stats;

    private float currentSpeed = 0f;
    public float maxSpeed = 7f;
    public float accelerationTime = 0.5f; // Time to reach max speed
    public float decelerationTime = 0.5f; // Time to slow down after reaching max speed

    private Rigidbody2D rb;

    private void Awake()
    {
        Stats = FindObjectOfType<PlayerStats>();
        WeaponTypes = FindObjectOfType<WeaponTypes>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SpriteRenderer.sprite = ArrowsSprites[WeaponTypes.SpecialArrows];
        if (SpriteRenderer.sprite == ArrowsSprites[2]) { gameObject.transform.localScale *= 0.25f; }
        else { gameObject.transform.localScale = new Vector3(3, 3, 3); }

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector3 distanceToPlayer = this.gameObject.transform.position - Stats.transform.position;
        float distance = distanceToPlayer.magnitude;
        if (Mathf.Abs(distance) > 10)
        {
            Destroy(gameObject);
        }

        // Initially accelerate fast based on Stats.RangedSpeed
        if (currentSpeed < maxSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed * Stats.RangedSpeed, Time.deltaTime / accelerationTime);
        }
        else
        {
            // After reaching max speed, apply deceleration
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime / decelerationTime);
        }

        if(rb.velocity.magnitude == 0)
        {
            Destroy(gameObject);
        }

        // Apply the velocity to the Rigidbody2D
        rb.velocity = rb.velocity.normalized * currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (WeaponTypes.MeleeWeapon) { return; }

        if (other.CompareTag("Enemy") && other != null)
        {
            Enemy = other.GetComponent<Enemy>();

            if (Enemy == null)
            {
                Enemy = other.GetComponentInChildren<Enemy>();

                if (Enemy == null)
                {
                    Enemy = other.GetComponentInParent<Enemy>();
                }
            }

            if (Enemy.Health <= 0) { return; }

            Enemy.TakeDamage(Mathf.RoundToInt((2 + Stats.AttackDamage * (Stats.RangedSpeed * 0.45f)) * WeaponTypes.DamageMultiplier), Vector3.zero);

            GameObject DNumber = Instantiate(DamageIndicator, Enemy.gameObject.transform.position + new Vector3(0, 2, 0), Quaternion.identity);
            Debug.Log("Spawning Damage Indicator");

            TextMeshPro Text = DNumber.GetComponent<TextMeshPro>();
            if (Text != null)
            {
                Text.text = (Mathf.RoundToInt((2 + Stats.AttackDamage * (Stats.RangedSpeed * 0.45f)) * WeaponTypes.DamageMultiplier).ToString());
            }
            else if (Text != null && Enemy.invulnerable)
            {
                Text.text = "0";
            }
            else
            {
                Debug.LogError("TextMeshPro component not found on DamageIndicator");
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") && other != null)
        {
            Destroy(gameObject);
        }
    }
}
