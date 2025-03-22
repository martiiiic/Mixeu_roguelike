using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleEnemy : RangedEnemy
{
    #region Public Variables
    [Header("Turtle Settings")]
    public bool isTurtle = true;
    public float shellProtectionDuration = 3f;
    public float shellCooldown = 2f;
    public float movementSpeed = 1.5f;
    public GameObject laserBeamPrefab;
    public float maxDistanceFromSpawn = 10f;

    [Header("Shell Bounce Attack")]
    public float bounceSpeed = 8f;
    public float bounceDuration = 4f;
    public int bounceContactDamage = 2;
    public float bounceAttackCooldown = 5f;
    public LayerMask wallLayers;
    public AudioClip bounceSound;

    [Header("Player Detection")]
    public float playerTooCloseDistance = 3f;
    public float minRandomShellEntryTime = 3f;
    public float maxRandomShellEntryTime = 6f;

    [Header("Animation")]
    public string walkingAnimationParam = "IsWalking";
    public string shellAnimationParam = "InShell";
    public string bounceAnimationParam = "IsShellBouncing";

    [Header("Audio")]
    public AudioClip ShellSound;
    public float maxAudioDistance = 20f;
    public float audioRolloffStartDistance = 5f;

    [Header("Random Movement")]
    public float randomMovementChangeInterval = 2f;
    public float randomMovementSpeed = 1.2f;
    public float randomMovementMaxDistance = 4f;
    #endregion

    #region Private Variables
    private bool isInShell = false;
    private bool canEnterShell = true;
    private float lastShellTime = 0f;
    private AudioSource ShellSoundSource;
    private AudioSource BounceSoundSource;
    private bool isFacingRight = true;
    private float lastDamageTime = 0f;
    private float shellEntryDelay = 0.5f;
    private float lastShellCheckTime = 0f;
    private float shellCheckInterval = 2f;

    private bool isShellBouncing = false;
    private float lastBounceAttackTime = 0f;
    private Vector2 bounceDirection;
    private float bouncingTimeLeft = 0f;

    private Vector3 spawnPosition;
    private float nextRandomShellTime;

    private Vector2 randomMoveTarget;
    private float nextRandomMoveTime;
    private Vector2 currentMoveDirection;
    private float lastBouncePhysicsTime = 0f;
    private float bouncePhysicsUpdateInterval = 0.01f;
    #endregion

    #region Initialization
    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        Speed = movementSpeed;

        ShellSoundSource = gameObject.AddComponent<AudioSource>();
        ShellSoundSource.clip = ShellSound;
        ConfigureAudioSource(ShellSoundSource);

        BounceSoundSource = gameObject.AddComponent<AudioSource>();
        BounceSoundSource.clip = bounceSound;
        ConfigureAudioSource(BounceSoundSource);

        if (LaserSoundSource != null)
        {
            ConfigureAudioSource(LaserSoundSource);
        }

        projectilePrefab = laserBeamPrefab;

        FixRigidbody();
    }

    private void ConfigureAudioSource(AudioSource source)
    {
        if (source == null) return;

        source.spatialBlend = 1.0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = audioRolloffStartDistance;
        source.maxDistance = maxAudioDistance;
        source.dopplerLevel = 0.5f;
    }

    protected override void Start()
    {
        base.Start();

        spawnPosition = transform.position;

        nextRandomShellTime = Time.time + Random.Range(minRandomShellEntryTime, maxRandomShellEntryTime);

        SetNewRandomMoveTarget();
        nextRandomMoveTime = Time.time + randomMovementChangeInterval;

        if (animator != null)
        {
            animator.SetBool(walkingAnimationParam, false);
            animator.SetBool(shellAnimationParam, false);
            animator.SetBool(bounceAnimationParam, false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isTurtle) return;

        if (isInShell || isShellBouncing)
        {
            if (!isShellBouncing)
            {
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (canEnterShell && target != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);

            if (distanceToPlayer <= playerTooCloseDistance)
            {
                EnterShell();
            }
            else if (Time.time >= nextRandomShellTime)
            {
                EnterShell();
                nextRandomShellTime = Time.time + Random.Range(minRandomShellEntryTime, maxRandomShellEntryTime);
            }
        }

        UpdateSpriteDirection();

        if (animator != null)
        {
            animator.SetBool(walkingAnimationParam, rb.velocity.magnitude > 0.1f && !isInShell && !isShellBouncing);
            animator.SetBool(shellAnimationParam, isInShell && !isShellBouncing);
            animator.SetBool(bounceAnimationParam, isShellBouncing);
        }
    }

    private void FixedUpdate()
    {
        if (!isTurtle) return;

        if (isShellBouncing)
        {
            HandleBouncing();
        }
    }
    #endregion

    #region Shell Mechanics
    public override void TakeDamage(int damage, Vector2 knockbackForce)
    {
        lastDamageTime = Time.time;

        if (isTurtle)
        {
            if (isInShell || isShellBouncing)
            {
                // Reduce damage by half when in shell
                damage = Mathf.CeilToInt(damage / 2f);

                // Reset knockback force in shell
                knockbackForce = Vector2.zero;

                if (isInShell && !isShellBouncing && Time.time > lastBounceAttackTime + bounceAttackCooldown)
                {
                    if (Random.value < 0.7f)
                    {
                        StartCoroutine(StartShellBounce(0.2f));
                    }
                }
            }
            else if (canEnterShell && !isInShell)
            {
                StartCoroutine(EnterShellAfterDelay(shellEntryDelay));
            }
        }

        // Call base class TakeDamage to handle the flash effect and health reduction
        base.TakeDamage(damage, knockbackForce);
    }

    private IEnumerator EnterShellAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnterShell();
    }

    private void EnterShell()
    {
        if (!isTurtle || !canEnterShell || isInShell || isShellBouncing || Dead) return;

        isInShell = true;
        canEnterShell = false;
        lastShellTime = Time.time;

        if (ShellSoundSource != null && ShellSound != null)
        {
            ShellSoundSource.Play();
        }

        rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool(walkingAnimationParam, false);
            animator.SetBool(shellAnimationParam, true);
            animator.SetBool(bounceAnimationParam, false);
        }

        float choice = Random.value;
        if (choice < 0.6f && Time.time > lastBounceAttackTime + bounceAttackCooldown)
        {
            StartCoroutine(StartShellBounce(1.0f));
        }
        else
        {
            StartCoroutine(ShootFromShell());
        }

        StartCoroutine(ShellCooldown());
    }

    private IEnumerator ShellCooldown()
    {
        yield return new WaitForSeconds(shellProtectionDuration);

        if (!isShellBouncing)
        {
            isInShell = false;

            if (animator != null)
            {
                animator.SetBool(shellAnimationParam, false);
            }
        }

        yield return new WaitForSeconds(shellCooldown);
        canEnterShell = true;

        if (target != null)
        {
            nextRandomShellTime = Time.time + Random.Range(minRandomShellEntryTime, maxRandomShellEntryTime);
        }
    }

    private IEnumerator ShootFromShell()
    {
        yield return new WaitForSeconds(0.5f);

        int shootCount = Random.Range(5, 11);
        for (int i = 0; i < shootCount; i++)
        {
            if (Dead || isShellBouncing) yield break;

            if (target != null && !IsWallBetweenEnemyAndPlayer())
            {
                PredictAndShootAtPlayer();
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
    #endregion

    #region Shell Bounce Attack
    private IEnumerator StartShellBounce(float startDelay)
    {
        if (!isTurtle) yield break;

        yield return new WaitForSeconds(startDelay);

        if (Dead) yield break;

        isShellBouncing = true;
        isInShell = true;
        lastBounceAttackTime = Time.time;
        bouncingTimeLeft = bounceDuration;

        if (animator != null)
        {
            animator.SetBool(walkingAnimationParam, false);
            animator.SetBool(shellAnimationParam, false);
            animator.SetBool(bounceAnimationParam, true);
        }

        if (BounceSoundSource != null && bounceSound != null)
        {
            BounceSoundSource.Play();
        }

        if (target != null)
        {
            Vector2 awayFromPlayer = (transform.position - target.position).normalized;
            float randomAngle = Random.Range(-30f, 30f);
            bounceDirection = Quaternion.Euler(0, 0, randomAngle) * awayFromPlayer;
        }
        else
        {
            float angle = Random.Range(0, 360);
            bounceDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        rb.velocity = bounceDirection * bounceSpeed;

        StartCoroutine(EndBounceAfterDuration());
    }

    private IEnumerator EndBounceAfterDuration()
    {
        yield return new WaitForSeconds(bounceDuration);

        if (isShellBouncing)
        {
            isShellBouncing = false;
            rb.velocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool(bounceAnimationParam, false);
            }

            if (Time.time < lastShellTime + shellProtectionDuration)
            {
                isInShell = true;
                if (animator != null)
                {
                    animator.SetBool(shellAnimationParam, true);
                }
            }
            else
            {
                isInShell = false;
                if (animator != null)
                {
                    animator.SetBool(shellAnimationParam, false);
                }
            }
        }
    }

    private void HandleBouncing()
    {
        if (!isShellBouncing) return;

        bouncingTimeLeft -= Time.fixedDeltaTime;
        if (bouncingTimeLeft <= 0)
        {
            isShellBouncing = false;
            rb.velocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool(bounceAnimationParam, false);
                animator.SetBool(shellAnimationParam, isInShell);
            }
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, bounceDirection, 0.6f, wallLayers);
        if (hit.collider != null)
        {
            Vector2 reflection = Vector2.Reflect(bounceDirection, hit.normal).normalized;
            float randomDeviation = Random.Range(-3f, 3f);
            bounceDirection = Quaternion.Euler(0, 0, randomDeviation) * reflection;
            rb.velocity = bounceDirection * bounceSpeed;

            if (BounceSoundSource != null && bounceSound != null && !BounceSoundSource.isPlaying)
            {
                BounceSoundSource.Play();
            }
        }

        if (Random.value < 0.05f)
        {
            float randomAngle = Random.Range(-2f, 2f);
            bounceDirection = Quaternion.Euler(0, 0, randomAngle) * bounceDirection;
            bounceDirection = bounceDirection.normalized;
        }

        if (rb.velocity.magnitude < bounceSpeed * 0.9f)
        {
            rb.velocity = bounceDirection * bounceSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isTurtle || !isShellBouncing) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerState Player_s = collision.gameObject.GetComponent<PlayerState>();
            if (Player_s != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                Player_s.TakeDamage(bounceContactDamage);

                float randomAngle = Random.Range(-5f, 5f);
                bounceDirection = Quaternion.Euler(0, 0, randomAngle) * -knockbackDir;
                rb.velocity = bounceDirection * bounceSpeed;

                if (BounceSoundSource != null && bounceSound != null)
                {
                    BounceSoundSource.Play();
                }
            }
        }
        else if ((wallLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 reflection = Vector2.Reflect(bounceDirection, contact.normal).normalized;
            float randomDeviation = Random.Range(-3f, 3f);
            bounceDirection = Quaternion.Euler(0, 0, randomDeviation) * reflection;
            rb.velocity = bounceDirection * bounceSpeed;

            if (BounceSoundSource != null && bounceSound != null)
            {
                BounceSoundSource.Play();
            }
        }
    }
    #endregion

    #region Overridden Methods
    protected override void HandleEnemyBehavior()
    {
        if (!isTurtle)
        {
            base.HandleEnemyBehavior();
            return;
        }

        if (isInShell || isShellBouncing)
        {
            if (!isShellBouncing)
            {
                rb.velocity = Vector2.zero;
            }
            return;
        }

        if (target != null)
        {
            MoveAwayFromPlayer();
        }
        else
        {
            HandleRandomMovement();
        }
    }

    private void HandleRandomMovement()
    {
        if (isInShell || isShellBouncing) return;

        if (Time.time >= nextRandomMoveTime)
        {
            SetNewRandomMoveTarget();
            nextRandomMoveTime = Time.time + randomMovementChangeInterval;
        }

        float distanceToTarget = Vector2.Distance(transform.position, randomMoveTarget);

        if (distanceToTarget < 0.5f)
        {
            SetNewRandomMoveTarget();
            nextRandomMoveTime = Time.time + randomMovementChangeInterval;
        }

        currentMoveDirection = (randomMoveTarget - (Vector2)transform.position).normalized;

        rb.velocity = currentMoveDirection * randomMovementSpeed;
    }

    private void SetNewRandomMoveTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(1f, randomMovementMaxDistance);

        randomMoveTarget = (Vector2)spawnPosition + (randomDirection * randomDistance);

        float distanceFromSpawn = Vector2.Distance(transform.position, spawnPosition);
        if (distanceFromSpawn > maxDistanceFromSpawn * 0.8f)
        {
            Vector2 toSpawn = ((Vector2)spawnPosition - (Vector2)transform.position).normalized;
            randomMoveTarget = (Vector2)transform.position + (toSpawn * randomDistance);
        }
    }

    private void MoveAwayFromPlayer()
    {
        if (!isTurtle || isInShell || isShellBouncing || target == null) return;

        Vector2 direction = (transform.position - target.position).normalized;

        float randomAngle = Random.Range(-20f, 20f);
        direction = Quaternion.Euler(0, 0, randomAngle) * direction;

        float distanceFromSpawn = Vector2.Distance(transform.position, spawnPosition);

        if (distanceFromSpawn > maxDistanceFromSpawn)
        {
            Vector2 directionToSpawn = ((Vector2)spawnPosition - (Vector2)transform.position).normalized;

            float blendFactor = Mathf.Clamp01((distanceFromSpawn - maxDistanceFromSpawn) / 2f);
            direction = Vector2.Lerp(direction, directionToSpawn, blendFactor);
        }

        rb.velocity = direction * movementSpeed;
    }

    private void FixRigidbody()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    protected override void MaintainDistanceFromPlayer()
    {
        if (isTurtle)
        {
            MoveAwayFromPlayer();
        }
        else
        {
            base.MaintainDistanceFromPlayer();
        }
    }

    protected override void CheckForStuckState()
    {
        if (isTurtle && (isInShell || isShellBouncing)) return;
        base.CheckForStuckState();
    }

    protected override void StartRandomMovement()
    {
        if (isTurtle && (isInShell || isShellBouncing)) return;
        base.StartRandomMovement();
    }

    protected override void FollowPlayer()
    {
        if (isTurtle)
        {
            if (isInShell || isShellBouncing) return;
            MoveAwayFromPlayer();
        }
        else
        {
            base.FollowPlayer();
        }
    }
    #endregion

    #region Helper Methods
    private void UpdateSpriteDirection()
    {
        if (!isTurtle) return;
        if (isInShell || isShellBouncing) return;
        if (rb.velocity.magnitude < 0.1f) return;

        if (rb.velocity.x > 0.1f && !isFacingRight)
        {
            FlipSprite();
        }
        else if (rb.velocity.x < -0.1f && isFacingRight)
        {
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected override void PredictAndShootAtPlayer()
    {
        if (target == null) return;
        if (IsWallBetweenEnemyAndPlayer()) return;
        if (projectilePrefab == null) return;

        Rigidbody2D playerRb = target.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        Vector2 playerPosition = target.position;
        Vector2 playerVelocity = playerRb.velocity * 0.8f;
        Vector2 enemyPosition = transform.position;
        float bulletSpeed = 10f;

        Vector2 predictedPosition = CalculateInterceptPosition(playerPosition, playerVelocity, enemyPosition, bulletSpeed);

        float offsetX = Random.Range(-0.3f, 0.3f);
        float offsetY = Random.Range(-0.3f, 0.3f);
        predictedPosition += new Vector2(offsetX, offsetY);

        Vector2 direction = (predictedPosition - enemyPosition).normalized;
        GameObject projectile = Instantiate(projectilePrefab, playerPosition, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();

        if (projectileRb != null)
        {
            projectileRb.velocity = direction * bulletSpeed;

            if (LaserSoundSource != null)
            {
                LaserSoundSource.Play();
            }

            AudioSource projectileAudio = projectile.GetComponent<AudioSource>();
            if (projectileAudio != null)
            {
                ConfigureAudioSource(projectileAudio);
            }
        }
    }
    #endregion
}