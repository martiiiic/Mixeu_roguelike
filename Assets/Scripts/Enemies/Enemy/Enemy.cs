using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    private float activationDistance = 15f;
    private Transform playerTransform;
    private bool isActive = true;

    [Header("Enemy Type Selection")]
    public bool isMeleeEnemy = true;
    public bool isRangedEnemy = false;
    public bool isCircleShooter = false;
    public bool isWeepingAngel = false;
    public bool isCucMaruc = false;

    #region Public Variables
    [Header("Base Stats")]
    public float Speed;
    public int MaxHealth;
    public int Health;
    public int RandomMovementProbability;
    public bool invulnerable = false;

    [Header("Audio")]
    public AudioClip DeathSound;
    #endregion

    #region Protected Variables
    protected Rigidbody2D rb;
    protected GameManager manager;
    protected PlayerState Player;
    protected BoxCollider2D HotZone;
    protected SpriteRenderer spriteRenderer;
    protected AudioSource DeathSoundSource;
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;

    public Transform target;
    protected Vector3 originalPosition;
    protected Vector3 lastValidPosition;
    protected Vector2 Size;

    protected Color originalColor;

    public bool Dead = false;
    protected bool isRandomMoving = false;
    protected float stuckThreshold = 0.1f;
    protected float randomMovementTime = 0.1f;
    protected float randomMovementCooldown = 0.5f;

    public CollectibleCoin Coins;
    #endregion

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Enemy))]
    public class EnemyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Enemy enemy = (Enemy)target;

            serializedObject.Update();

            string[] options = { "Melee Enemy", "Ranged Enemy", "Circle Shooter", "Weeping Angel", "Cuc Maruc" };
            int selectedIndex = enemy.isMeleeEnemy ? 0 :
                               enemy.isRangedEnemy ? 1 :
                               enemy.isCircleShooter ? 2 :
                               enemy.isWeepingAngel ? 3 :
                               enemy.isCucMaruc ? 4 : 0;

            int newIndex = UnityEditor.EditorGUILayout.Popup("Enemy Type", selectedIndex, options);

            enemy.isMeleeEnemy = (newIndex == 0);
            enemy.isRangedEnemy = (newIndex == 1);
            enemy.isCircleShooter = (newIndex == 2);
            enemy.isWeepingAngel = (newIndex == 3);
            enemy.isCucMaruc = (newIndex == 4);

            DrawPropertiesExcluding(serializedObject, new string[] {
                "isMeleeEnemy", "isRangedEnemy", "isCircleShooter",
                "isWeepingAngel", "isCucMaruc"
            });

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                UnityEditor.EditorUtility.SetDirty(target);
            }
        }
    }
#endif

    #region Unity Lifecycle Methods

    private void RegisterWithEliteManager()
    {
        EliteEnemyManager eliteManager = FindObjectOfType<EliteEnemyManager>();
        if (eliteManager != null)
        {
            eliteManager.RegisterEnemy(this);
        }
    }

    protected virtual void Start()
    {
        PlayerState playerState = FindObjectOfType<PlayerState>();
        if (playerState != null)
        {
            playerTransform = playerState.transform;
        }
        RegisterWithEliteManager();
    }

    protected virtual void Awake()
    {
        FindObjectOfType<Optimizer>()?.RegisterEnemy(this);
        InitializeComponents();
        InitializeSettings();
    }

    private void OnDestroy()
    {
        FindObjectOfType<Optimizer>()?.UnregisterEnemy(this);
    }

    protected virtual void InitializeEnemyType()
    {
        if (GetComponent<RangedEnemy>()) Destroy(GetComponent<RangedEnemy>());
        if (GetComponent<CircleShooterEnemy>()) Destroy(GetComponent<CircleShooterEnemy>());
        if (GetComponent<WeepingAngelEnemy>()) Destroy(GetComponent<WeepingAngelEnemy>());
        if (GetComponent<CucMarucEnemy>()) Destroy(GetComponent<CucMarucEnemy>());

        if (isRangedEnemy) gameObject.AddComponent<RangedEnemy>();
        else if (isCircleShooter) gameObject.AddComponent<CircleShooterEnemy>();
        else if (isWeepingAngel) gameObject.AddComponent<WeepingAngelEnemy>();
        else if (isCucMaruc) gameObject.AddComponent<CucMarucEnemy>();
        else gameObject.AddComponent<RangedEnemy>(); // Default
    }

    protected virtual void Update()
    {
        Player = FindObjectOfType<PlayerState>();

        if (Player == null) return;

        if (Player.gameObject.activeSelf == false) return;

        if (playerTransform == null)
        {
            playerTransform = Player.transform;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > activationDistance && isActive)
        {
            DeactivateEnemy();
        }
        else if (distanceToPlayer <= activationDistance && !isActive)
        {
            ActivateEnemy();
        }

        if (!isActive || Dead) return;

        if (IsWallBetweenEnemyAndPlayer() || (Player != null && Player.Dead))
        {
            ReturnToOriginalPosition();
            target = null;
            return;
        }

        if (target != null)
        {
            HandleEnemyBehavior();
            HotZone.size = Size * 1.5f;
            CheckForStuckState();
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    private void DeactivateEnemy()
    {
        isActive = false;

        if (navMeshAgent != null) navMeshAgent.enabled = false;

        rb.Sleep();
        rb.simulated = false;
    }

    private void ActivateEnemy()
    {
        isActive = true;

        if (navMeshAgent != null) navMeshAgent.enabled = true;

        rb.WakeUp();
        rb.simulated = true;
    }

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        HandleCollisions(other);
    }
    #endregion

    #region Initialization
    protected virtual void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        Player = FindObjectOfType<PlayerState>();
        originalPosition = transform.position;

        DeathSoundSource = gameObject.AddComponent<AudioSource>();
        DeathSoundSource.clip = DeathSound;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInParent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        manager = FindObjectOfType<GameManager>();
        Coins = FindFirstObjectByType<CollectibleCoin>();

        HotZone = transform.Find("HotZone").GetComponent<BoxCollider2D>();
        lastValidPosition = transform.position;
        navMeshAgent = GetComponent<NavMeshAgent>();
        Size = new Vector2(HotZone.size.x, HotZone.size.y);
    }

    protected virtual void InitializeSettings()
    {
        if (manager.enemyHealthMultiplier != 0)
        {
            MaxHealth = MaxHealth * manager.enemyHealthMultiplier;
        }

        Health = MaxHealth;
        Dead = false;
    }
    #endregion

    #region Movement and Behavior
    protected abstract void HandleEnemyBehavior();

    protected virtual void ReturnToOriginalPosition()
    {
        if (Vector2.Distance(transform.position, originalPosition) > 0.1f)
        {
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && target == null)
            {
                navMeshAgent.SetDestination(originalPosition);
            }
            else if (target == null)
            {
                Vector2 direction = (originalPosition - transform.position).normalized;
                rb.velocity = direction * Speed;
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        HotZone.size = Size;
    }

    protected virtual void CheckForStuckState()
    {
        if (rb.velocity.magnitude < stuckThreshold)
        {
            Vector2 moveAway = (transform.position - lastValidPosition).normalized;
            rb.velocity = moveAway * Speed;
        }
        else
        {
            lastValidPosition = transform.position;
        }
    }

    protected virtual void StartRandomMovement()
    {
        if (isRandomMoving) return;

        isRandomMoving = true;
        HotZone.enabled = false;

        Vector2 randomDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
        rb.AddForce(randomDirection * Speed, ForceMode2D.Impulse);

        randomMovementTime = Time.time + randomMovementCooldown;

        StartCoroutine(StopRandomMovementAfterDelay(1f));
    }

    protected virtual void FollowPlayer()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * Speed;
    }

    protected virtual void StopMoving()
    {
        rb.velocity = Vector2.zero;
    }
    #endregion

    #region Combat
    public virtual void TakeDamage(int damage, Vector2 knockbackForce)
    {
        if (invulnerable) { return; }
        target = FindFirstObjectByType<PlayerState>().gameObject.transform;
        Health -= damage;

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashWhite(damage));
        }
    }

    public virtual void Die()
    {
        if (Dead) { return; }

        rb.velocity = Vector2.zero;
        gameObject.tag = "Untagged";
        invulnerable = true;
        DeathSoundSource.Play();

        Coins = FindFirstObjectByType<CollectibleCoin>();
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        RoomTemplates RT = FindObjectOfType<RoomTemplates>();
        FishCollectible Fish = FindObjectOfType<FishCollectible>();

        this.HotZone.enabled = false;
        this.target = null;

        // Calculate coin drop amount
        int totalCoins = UnityEngine.Random.Range(
            1 + manager.DungeonNumber + Mathf.RoundToInt(0.25f * manager.enemyHealthMultiplier),
            2 + MaxHealth + Mathf.RoundToInt((0.5f * manager.enemyHealthMultiplier) + (0.5f * manager.PriceMultiplier)) + manager.enemyDamageMultiplier - Mathf.RoundToInt(RT.rooms.Count * 0.05f)
        );

        Coins.SpawnCoin(totalCoins, this.transform);

        BoxCollider2D hitbox = GetComponent<BoxCollider2D>();
        if (hitbox != null) { hitbox.enabled = false; }

        this.spriteRenderer.enabled = false;
        SpriteRenderer[] subSpriterenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in subSpriterenderers)
        {
            renderer.enabled = false;
        }

        int rand = UnityEngine.Random.Range(0, 9);
        if (stats.CurrentHealth == 1) { rand -= UnityEngine.Random.Range(0, 3); }
        if (Fish != null && rand < 1) { Fish.SpawnFish(1, this.transform); }

        transform.position = new Vector3(-1000, 1000, -5000);
        StartCoroutine(DeathSFX());
    }
    #endregion

    #region Detection and Utility
    public virtual bool IsWallBetweenEnemyAndPlayer()
    {
        if (target == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer < 1.7f)
            return false;

        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, directionToPlayer, distanceToPlayer);

        Debug.DrawLine(transform.position, target.position, Color.red);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.gameObject.name == "Blocks")
            {
                return true;
            }
        }

        return false;
    }

    protected virtual void HandleCollisions(Collision2D other)
    {
    }
    #endregion

    #region Coroutines
    protected virtual IEnumerator FlashWhite(int damage)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSecondsRealtime(0.05f);

        float healthPercentage = (float)damage / MaxHealth;
        float freezeTime = Mathf.Max(healthPercentage * 0.05f, 0.01f);
        freezeTime = Mathf.Min(freezeTime, 0.05f);

        Time.timeScale = 0.01f;
        yield return new WaitForSecondsRealtime(freezeTime);
        Time.timeScale = 1f;

        if (Health <= 0)
        {
            invulnerable = true;
            Die();
        }

        spriteRenderer.color = originalColor;
    }

    protected virtual IEnumerator DeathSFX()
    {
        Dead = true;
        yield return new WaitForSeconds(DeathSound.length);
        Destroy(this.gameObject);
    }

    protected virtual IEnumerator StopRandomMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isRandomMoving = false;
        HotZone.enabled = true;
    }
    #endregion
}


// Ranged Enemy class


// Circle Shooter Enemy class


// Weeping Angel Enemy class


// CucMaruc Enemy class
