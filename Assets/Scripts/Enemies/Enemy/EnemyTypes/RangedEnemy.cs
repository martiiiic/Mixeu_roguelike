using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    #region Public Variables
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float shootingCooldown = 2f;
    public float shootingRange = 5f;

    [Header("Ranged Enemy Behavior")]
    public float rangedAvoidanceRadius = 2f;
    public float unpredictableMovementRange = 1.5f;

    [Header("Audio")]
    public AudioClip LaserSound;
    #endregion

    #region Private Variables
    protected AudioSource LaserSoundSource;
    protected bool hasStartedShooting = false;
    protected float lastShotTime = 0f;
    protected float actualCooldown;
    #endregion

    #region Initialization
    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        LaserSoundSource = gameObject.AddComponent<AudioSource>();
        LaserSoundSource.clip = LaserSound;

        AdjustCooldownBasedOnDifficulty();
    }

    private void AdjustCooldownBasedOnDifficulty()
    {
        int difficulty = Difficulty.Diff;

        switch (difficulty)
        {
            case 0: // Easy
                actualCooldown = shootingCooldown * 1.2f;
                break;
            case 1: // Normal
                actualCooldown = shootingCooldown;
                break;
            case 2: // Hard
                actualCooldown = shootingCooldown * 0.9f;
                break;
            case 3: // Insane
                actualCooldown = shootingCooldown * 0.8f;
                break;
            case 4: // Mixeu's Torment
                actualCooldown = shootingCooldown * 0.7f;
                break;
            default:
                actualCooldown = shootingCooldown;
                break;
        }
    }
    #endregion

    #region Overridden Methods
    protected override void HandleEnemyBehavior()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer > shootingRange)
        {
            FollowPlayer();
        }
        else
        {
            if (!hasStartedShooting)
            {
                StartCoroutine(DelayedStartShooting());
            }
            else if (Time.time > lastShotTime + shootingCooldown)
            {
                if (!IsWallBetweenEnemyAndPlayer())
                {
                    PredictAndShootAtPlayer();
                    lastShotTime = Time.time + Random.Range(-0.5f, 0.4f);
                }
            }

            MaintainDistanceFromPlayer();

            if (distanceToPlayer > shootingRange * 0.75f)
            {
                AddUnpredictableMovement();
            }
        }

        AvoidOtherRangedEnemies();
    }
    #endregion

    #region Shooting
    protected virtual void PredictAndShootAtPlayer()
    {
        if (IsWallBetweenEnemyAndPlayer()) { target = null; return; }
        if (projectilePrefab == null || target == null) return;

        Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        Vector2 playerPosition = target.position;
        Vector2 playerVelocity = playerRb.velocity;

        Vector2 enemyPosition = transform.position;
        float bulletSpeed = 10f;

        Vector2 predictedPosition = CalculateInterceptPosition(playerPosition, playerVelocity, enemyPosition, bulletSpeed);

        float offsetX = Random.Range(-0.25f, 0.25f) / (manager.rangedEnemyDamageMultiplier * 0.12f);
        float offsetY = Random.Range(-0.25f, 0.25f) / (manager.rangedEnemyDamageMultiplier * 0.12f);
        predictedPosition += new Vector2(offsetX, offsetY);

        Vector2 direction = (predictedPosition - enemyPosition).normalized;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();

        if (projectileRb != null)
        {
            projectileRb.velocity = direction * bulletSpeed;
            LaserSoundSource.Play();
        }
    }

    protected Vector2 CalculateInterceptPosition(Vector2 playerPos, Vector2 playerVel, Vector2 enemyPos, float bulletSpeed)
    {
        Vector2 displacement = playerPos - enemyPos;
        float playerSpeedSquared = playerVel.sqrMagnitude;
        float bulletSpeedSquared = bulletSpeed * bulletSpeed;

        float a = playerSpeedSquared - bulletSpeedSquared;
        float b = 2 * Vector2.Dot(displacement, playerVel);
        float c = displacement.sqrMagnitude;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return playerPos;
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2 * a);
        float t2 = (-b - sqrtDiscriminant) / (2 * a);
        float t = Mathf.Max(t1, t2);

        return playerPos + playerVel * t;
    }
    #endregion

    #region Movement
    protected virtual void MaintainDistanceFromPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        const float tolerance = 0.1f;

        if (distanceToPlayer < shootingRange / 3f)
        {
            Vector2 direction = (transform.position - target.position).normalized;
            rb.velocity = direction * Speed;
        }
        else if (distanceToPlayer > shootingRange / 2f)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * Speed;
        }
        else if (Mathf.Abs(distanceToPlayer - shootingRange / 2f) < tolerance)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            Vector2 randomOffset = new Vector2(
                Random.Range(-0.9f, 0.9f),
                Random.Range(-0.9f, 0.9f)
            ).normalized;
            Vector2 finalDirection = (direction + randomOffset).normalized;
            float impulseStrength = Random.Range(Speed * 0.2f, Speed * 0.5f);
            rb.AddForce(finalDirection * impulseStrength, ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    protected virtual void AvoidOtherRangedEnemies()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, rangedAvoidanceRadius);

        foreach (Collider2D enemy in nearbyEnemies)
        {
            if (enemy != GetComponent<Collider2D>() && enemy.GetComponent<RangedEnemy>() != null)
            {
                Vector2 avoidanceDirection = (transform.position - enemy.transform.position).normalized;
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                float avoidanceStrength = Mathf.Clamp01((rangedAvoidanceRadius - distance) / rangedAvoidanceRadius);
                rb.velocity += avoidanceDirection * Speed * avoidanceStrength;
            }
        }
    }

    protected virtual void AddUnpredictableMovement()
    {
        float randomX = Random.Range(-unpredictableMovementRange, unpredictableMovementRange);
        float randomY = Random.Range(-unpredictableMovementRange, unpredictableMovementRange);
        Vector2 randomOffset = new Vector2(randomX, randomY) * 0.2f;

        rb.velocity += randomOffset * Time.deltaTime;
    }
    #endregion

    #region Coroutines
    protected IEnumerator DelayedStartShooting()
    {
        hasStartedShooting = true;
        yield return new WaitForSeconds(1.75f);
    }
    #endregion
}
