using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    Straight,
    Sine,
    Circular,
    Spiral,
    Homing,
    Bouncing,
    SpiralOut,
    ZigZag,
    Accelerating,
    Decelerating,
    SplitOnImpact,
    SplitAfterTime,
    RandomDirection,
    Burst
}

[System.Serializable]
public class SinWaveSettings
{
    [Range(0.1f, 10f)]
    public float amplitude = 1f;
    [Range(0.1f, 10f)]
    public float frequency = 1f;
}

[System.Serializable]
public class CircularSettings
{
    [Range(0.5f, 10f)]
    public float radius = 2f;
    [Range(-10f, 10f)]
    public float rotationSpeed = 2f;
    public bool clockwise = true;
    public bool useEnemyAsCenter = true;
}

[System.Serializable]
public class SpiralSettings
{
    [Range(0.001f, 50f)]
    public float expansionRate = 0.5f;
    [Range(0.001f, 100f)]
    public float rotationSpeed = 2f;
    public bool useEnemyAsCenter = true;
}

[System.Serializable]
public class HomingSettings
{
    [Range(0.1f, 10f)]
    public float trackingSpeed = 2f;
    [Range(0.1f, 5f)]
    public float maxTurnAngle = 1f;
    public float activationDelay = 0.5f;
    public float maxHomingDuration = 5f;
}

[System.Serializable]
public class BouncingSettings
{
    [Range(1, 10)]
    public int maxBounces = 3;
    [Range(0f, 1f)]
    public float bounceDamping = 0.8f;
}

[System.Serializable]
public class ZigZagSettings
{
    [Range(0.1f, 5f)]
    public float zigZagInterval = 1f;
    [Range(10f, 180f)]
    public float zigZagAngle = 45f;
}

[System.Serializable]
public class AccelerationSettings
{
    public float initialSpeed = 1f;
    public float finalSpeed = 10f;
    public float accelerationTime = 2f;
}

[System.Serializable]
public class SplitSettings
{
    [Range(2, 10)]
    public int splitCount = 3;
    [Range(0f, 360f)]
    public float spreadAngle = 120f;
    public float splitTimer = 2f;
}

[System.Serializable]
public class BurstSettings
{
    public float burstDelay = 1f;
    public float burstSpeed = 5f;
    public float burstDuration = 0.2f;
}

public class ProjectileDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int baseDamage = 1;
    public float damageMultiplier = 1f;
    public bool penetratesEnemies = false;
    [Range(0, 10)]
    public int maxPenetrations = 0;
    public bool damagesWalls = false;
    public bool penetratesWalls = false;

    [Header("Lifetime")]
    public float lifeTime = 5f;
    public bool useDistanceLimit = true;
    public float maxDistance = 20f;
    [ColorUsage(true, true)]
    public Color bulletColor = Color.white;

    [Header("Visual Effects")]
    public bool leavesTrail = false;
    public float trailDuration = 0.5f;
    public Color trailColor = Color.white;
    public float bulletSize = 1f;
    public bool pulseSize = false;
    [Range(0.1f, 2f)]
    public float pulseRange = 0.2f;
    [Range(0.1f, 5f)]
    public float pulseSpeed = 1f;

    [Header("Projectile Movement")]
    public MovementType movementType = MovementType.Straight;
    public float baseSpeed = 5f;
    public Vector2 initialDirection = Vector2.right;

    [Header("Movement Settings")]
    public SinWaveSettings sineSettings;
    public CircularSettings circularSettings;
    public SpiralSettings spiralSettings;
    public HomingSettings homingSettings;
    public BouncingSettings bounceSettings;
    public ZigZagSettings zigZagSettings;
    public AccelerationSettings accelerationSettings;
    public SplitSettings splitSettings;
    public BurstSettings burstSettings;

    [Header("Sound Effects")]
    public bool playSoundOnFire = false;
    public bool playSoundOnImpact = false;
    public float pitchVariation = 0.1f;

    [Header("References")]
    private Vector2 startPosition;
    private Transform spawnerTransform;
    private Transform target;
    private PlayerState player;
    private GameManager manager;
    private int currentPenetrations = 0;
    private Rigidbody2D rb;
    private float elapsedTime = 0f;
    private Vector2 currentDirection;
    private float homingTimeRemaining;
    private int bounceCount = 0;
    private float currentSpeed;
    private bool isBursting = false;
    public bool hasInitialized = false;
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Vector2 lastPosition;
    private float circleAngle = 0f;
    private float spiralAngle = 0f;

    private void Awake()
    {
        player = FindObjectOfType<PlayerState>();
        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        spawnerTransform = transform.parent;

        if (spawnerTransform == null)
        {
            spawnerTransform = transform;
        }

        if (!rb)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.isKinematic = true;
    }

    private void Start()
    {
        InitializeProjectile();
    }

    public void InitializeProjectile()
    {
        player = FindObjectOfType<PlayerState>();
        if (hasInitialized) return;
        hasInitialized = true;

        startPosition = transform.position;
        lastPosition = startPosition;
        currentDirection = initialDirection.normalized;
        currentSpeed = baseSpeed;
        homingTimeRemaining = homingSettings.maxHomingDuration;
        circleAngle = 0f;
        spiralAngle = 0f;

        if (useDistanceLimit == false)
            maxDistance = float.MaxValue;

        if (movementType == MovementType.Homing)
            target = player.transform;

        if (leavesTrail && trailRenderer)
        {
            trailRenderer.time = trailDuration;
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0);
        }

        if (movementType == MovementType.Accelerating || movementType == MovementType.Decelerating)
            currentSpeed = movementType == MovementType.Accelerating ? accelerationSettings.initialSpeed : accelerationSettings.finalSpeed;

        spriteRenderer.color = bulletColor;
        transform.localScale = Vector3.one * bulletSize;

        Invoke("DestroyAfterLifetime", lifeTime);
    }

    private void Update()
    {
        if (!hasInitialized)
            InitializeProjectile();

        elapsedTime += Time.deltaTime;
        lastPosition = transform.position;

        ProcessMovement();

        if (pulseSize)
            ProcessPulse();

        CheckDistanceLimit();

        if (movementType == MovementType.SplitAfterTime && elapsedTime >= splitSettings.splitTimer)
            SplitProjectile();
    }

    private void ProcessMovement()
    {
        Vector2 movement = Vector2.zero;

        switch (movementType)
        {
            case MovementType.Straight:
                movement = currentDirection * currentSpeed;
                break;

            case MovementType.Sine:
                float sideOffset = Mathf.Sin(elapsedTime * sineSettings.frequency) * sineSettings.amplitude;
                Vector2 sideDir = Vector2.Perpendicular(currentDirection);
                movement = (currentDirection * currentSpeed) + (sideDir * sideOffset * currentSpeed * Time.deltaTime);
                break;

            case MovementType.Circular:
                float rotationDirection = circularSettings.clockwise ? -1 : 1;
                circleAngle += circularSettings.rotationSpeed * Time.deltaTime * rotationDirection;

                Vector2 centerPoint = circularSettings.useEnemyAsCenter && spawnerTransform != null ?
                    (Vector2)spawnerTransform.position : startPosition;

                Vector2 orbitPosition = centerPoint + new Vector2(
                    Mathf.Cos(circleAngle) * circularSettings.radius,
                    Mathf.Sin(circleAngle) * circularSettings.radius
                );

                transform.position = orbitPosition;
                return;

            case MovementType.Spiral:
                spiralAngle += spiralSettings.rotationSpeed * Time.deltaTime;
                float radius = elapsedTime * spiralSettings.expansionRate;

                Vector2 spiralCenter = spiralSettings.useEnemyAsCenter && spawnerTransform != null ?
                    (Vector2)spawnerTransform.position : startPosition;

                Vector2 spiralPos = spiralCenter + new Vector2(
                    Mathf.Cos(spiralAngle) * radius,
                    Mathf.Sin(spiralAngle) * radius
                );

                transform.position = spiralPos;
                return;

            case MovementType.SpiralOut:
                float currentRadius = spiralSettings.expansionRate * elapsedTime;
                float spiralOutAngle = elapsedTime * spiralSettings.rotationSpeed;

                Vector2 spiralOutCenter = spiralSettings.useEnemyAsCenter && spawnerTransform != null ?
                    (Vector2)spawnerTransform.position : startPosition;

                Vector2 newPos = spiralOutCenter + new Vector2(
                    Mathf.Cos(spiralOutAngle) * currentRadius,
                    Mathf.Sin(spiralOutAngle) * currentRadius
                );

                transform.position = newPos;
                return;

            case MovementType.Homing:
                if (elapsedTime > homingSettings.activationDelay && homingTimeRemaining > 0 && target != null)
                {
                    Vector2 targetDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
                    float angleToTarget = Vector2.SignedAngle(currentDirection, targetDir);
                    float maxTurn = homingSettings.maxTurnAngle * Time.deltaTime * homingSettings.trackingSpeed;
                    float turnAmount = Mathf.Clamp(angleToTarget, -maxTurn, maxTurn);
                    currentDirection = Quaternion.Euler(0, 0, turnAmount) * currentDirection;
                    homingTimeRemaining -= Time.deltaTime;
                }
                movement = currentDirection * currentSpeed;
                break;

            case MovementType.ZigZag:
                if (elapsedTime % (zigZagSettings.zigZagInterval * 2) < zigZagSettings.zigZagInterval)
                {
                    movement = Quaternion.Euler(0, 0, zigZagSettings.zigZagAngle) * currentDirection * currentSpeed;
                }
                else
                {
                    movement = Quaternion.Euler(0, 0, -zigZagSettings.zigZagAngle) * currentDirection * currentSpeed;
                }
                break;

            case MovementType.Accelerating:
                float t = Mathf.Clamp01(elapsedTime / accelerationSettings.accelerationTime);
                currentSpeed = Mathf.Lerp(accelerationSettings.initialSpeed, accelerationSettings.finalSpeed, t);
                movement = currentDirection * currentSpeed;
                break;

            case MovementType.Decelerating:
                float t2 = Mathf.Clamp01(elapsedTime / accelerationSettings.accelerationTime);
                currentSpeed = Mathf.Lerp(accelerationSettings.finalSpeed, accelerationSettings.initialSpeed, t2);
                movement = currentDirection * currentSpeed;
                break;

            case MovementType.Burst:
                if (!isBursting && elapsedTime >= burstSettings.burstDelay)
                {
                    StartCoroutine(Burst());
                }
                movement = currentDirection * currentSpeed;
                break;

            case MovementType.RandomDirection:
                if (elapsedTime % 1 < 0.05f)
                {
                    float randomAngle = Random.Range(0f, 360f);
                    currentDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
                }
                movement = currentDirection * currentSpeed;
                break;
        }

        transform.position = (Vector2)transform.position + (movement * Time.deltaTime);

        if (movement != Vector2.zero)
            transform.up = movement;
    }

    private void ProcessPulse()
    {
        float pulseValue = Mathf.Sin(elapsedTime * pulseSpeed * Mathf.PI) * pulseRange + 1f;
        transform.localScale = Vector3.one * bulletSize * pulseValue;
    }

    private void CheckDistanceLimit()
    {
        if (useDistanceLimit && player != null)
        {
            if (Vector2.Distance(transform.position, player.transform.position) > maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other != null && player != null && !player.Invulnerable)
        {
            int calculatedDamage = Mathf.RoundToInt(baseDamage * damageMultiplier * manager.rangedEnemyDamageMultiplier);
            player.TakeDamage(calculatedDamage);

            if (playSoundOnImpact)
                PlayImpactSound();

            if (movementType == MovementType.SplitOnImpact)
                SplitProjectile();

            Destroy(gameObject);
        }
        else if (other.CompareTag("Enemy") && penetratesEnemies)
        {
            currentPenetrations++;
            if (currentPenetrations > maxPenetrations)
                Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("BlockWall") || other.CompareTag("BreakableWall"))
        {
            if (penetratesWalls)
                return;

            if (damagesWalls && other.CompareTag("BreakableWall"))
            {
                SecretRoomBreakableWalls wall = other.GetComponent<SecretRoomBreakableWalls>();
                if (wall != null)
                    wall.DestroyWallOnImpact();
            }

            if (movementType == MovementType.Bouncing && bounceCount < bounceSettings.maxBounces)
            {
                Bounce(other);
            }
            else
            {
                if (movementType == MovementType.SplitOnImpact)
                    SplitProjectile();

                Destroy(gameObject);
            }
        }
    }

    private void Bounce(Collider2D surface)
    {
        bounceCount++;
        currentSpeed *= bounceSettings.bounceDamping;

        Vector2 surfaceNormal = GetSurfaceNormal(surface);
        currentDirection = Vector2.Reflect(currentDirection, surfaceNormal);

        if (playSoundOnImpact)
            PlayImpactSound();
    }

    private Vector2 GetSurfaceNormal(Collider2D surface)
    {
        Vector2 hitPoint = surface.ClosestPoint(transform.position);
        Vector2 centerPoint = surface.bounds.center;

        return (hitPoint - centerPoint).normalized;
    }

    private void SplitProjectile()
    {
        if (splitSettings.splitCount <= 0)
            return;

        float angleStep = splitSettings.spreadAngle / (splitSettings.splitCount - 1);
        float startAngle = -splitSettings.spreadAngle / 2;

        for (int i = 0; i < splitSettings.splitCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * currentDirection;

            ProjectileDamage newProjectile = Instantiate(this, transform.position, Quaternion.identity);

            newProjectile.initialDirection = direction;
            newProjectile.movementType = MovementType.Straight;
            newProjectile.elapsedTime = 0;
            newProjectile.baseSpeed = baseSpeed * 0.8f;
            newProjectile.lifeTime = lifeTime / 2;
            newProjectile.bulletSize = bulletSize * 0.7f;
            newProjectile.hasInitialized = false;
        }

        Destroy(gameObject);
    }

    private IEnumerator Burst()
    {
        isBursting = true;
        float originalSpeed = currentSpeed;
        currentSpeed = burstSettings.burstSpeed;

        yield return new WaitForSeconds(burstSettings.burstDuration);

        currentSpeed = originalSpeed;
        isBursting = false;
    }

    private void PlayImpactSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.Play();
        }
    }

    private void DestroyAfterLifetime()
    {
        Destroy(gameObject);
    }
}