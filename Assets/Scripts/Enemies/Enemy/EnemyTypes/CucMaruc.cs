using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CucMarucEnemy : Enemy
{
    #region Public Variables
    [Header("CucMaruc Settings")]
    public float playerDetectionRange = 10f;  // Range to detect player and hide
    public float undergroundRange = 10f;      // How far it can move underground
    public float surfaceDistance = 5f;        // Distance to surface from player
    public GameObject projectilePrefab;
    public AudioClip LaserSound;
    #endregion

    #region Private Variables
    private bool isUnderground = false;
    private bool isMovingUnderground = false;
    private bool hasJustSurfaced = false;
    private float surfacedShootDelay = 1f;
    private Vector2 undergroundDirection;
    private float directionChangeTimer = 0f;
    private float directionChangeInterval = 2f;
    private float surfaceTimer = 0f;
    private float surfaceTime = 3f;           // How long to stay on surface
    private AudioSource LaserSoundSource;
    #endregion

    #region Initialization
    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        SetRandomDirection();
        LaserSoundSource = gameObject.AddComponent<AudioSource>();
        LaserSoundSource.clip = LaserSound;

        // Start underground by default
        animator.SetBool("Hiding", true);
        isUnderground = true;
        invulnerable = true;
    }
    #endregion

    #region Overridden Methods
    protected override void HandleEnemyBehavior()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (isUnderground)
        {
            // Enemy is underground
            UndergroundMovement();

            // Surface if player is far enough
            if (distanceToPlayer > surfaceDistance && !hasJustSurfaced)
            {
                SurfaceFromUnderground();
            }
        }
        else
        {
            // Enemy is on the surface
            surfaceTimer -= Time.deltaTime;

            // If player gets too close, hide underground
            if (distanceToPlayer < playerDetectionRange)
            {
                GoUnderground();
                return;
            }

            // Shoot at player if just surfaced
            if (hasJustSurfaced)
            {
                surfacedShootDelay -= Time.deltaTime;
                if (surfacedShootDelay <= 0 && !IsWallBetweenEnemyAndPlayer())
                {
                    ShootAtPlayer();
                    hasJustSurfaced = false;
                }
            }

            // Go back underground after surface time expires
            if (surfaceTimer <= 0)
            {
                GoUnderground();
            }
        }
    }
    #endregion

    #region Underground Behavior
    private void GoUnderground()
    {
        animator.SetBool("Hiding", true);
        isUnderground = true;
        invulnerable = true;  // Enemy is invulnerable underground

        // Reset the underground movement variables
        SetRandomDirection();
        directionChangeTimer = directionChangeInterval;
    }

    private void SurfaceFromUnderground()
    {
        animator.SetBool("Hiding", false);
        isUnderground = false;
        invulnerable = false;  // Enemy is vulnerable on surface
        hasJustSurfaced = true;
        surfacedShootDelay = 1f;
        surfaceTimer = surfaceTime;
    }

    private void UndergroundMovement()
    {
        // Only move when underground
        Vector2 moveDirection = undergroundDirection * Speed * Time.deltaTime;

        // Cast a ray to check for walls
        RaycastHit2D hit = Physics2D.Raycast(transform.position, undergroundDirection, Speed * Time.deltaTime, LayerMask.GetMask("Wall"));

        if (hit.collider == null)
        {
            // No wall, move normally
            rb.MovePosition(rb.position + moveDirection);
        }
        else
        {
            // Hit a wall, change direction
            undergroundDirection = Vector2.Reflect(undergroundDirection, hit.normal);
            directionChangeTimer = directionChangeInterval;
        }

        // Occasionally change direction
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            SetRandomDirection();
            directionChangeTimer = directionChangeInterval;
        }
    }

    private void SetRandomDirection()
    {
        undergroundDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
    #endregion

    #region Shooting
    private void ShootAtPlayer()
    {
        if (projectilePrefab == null || target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();

        if (projectileRb != null)
        {
            projectileRb.velocity = direction * 10f;
            LaserSoundSource.Play();
        }
    }

    private bool IsWallBetweenEnemyAndPlayer()
    {
        if (target == null) return true;

        Vector2 direction = (target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, LayerMask.GetMask("Wall"));
        return hit.collider != null;
    }
    #endregion
}