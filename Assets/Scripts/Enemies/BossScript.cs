using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BossScript : MonoBehaviour
{
    public int BaseHealth;
    public int MaxHealth;
    public int Health;
    public int BaseDamage;
    public int AttackDamage;
    public float ShootSpeed;
    public int[] attackPatterns;
    public Sprite[] BossSprites;
    public int BossId;
    public int BossDifficulty;
    public int LocalDifficulty;
    public string BossName;
    public GameObject BossChains;
    public AudioClip teleportSound;
    public AudioClip projectileSound;
    public AudioClip spawnEnemySound;
    private AudioSource audioSource;

    public bool Invulnerable;

    private GameManager manager;
    private SpriteRenderer spriteRenderer;

    public Material normalMaterial;
    public Material invulnerableMaterial;

    private PlayerState player;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;
    private bool playerDetected = false;

    public GameObject ExitHand;
    public GameObject VoidPortal;
    public GameObject projectilePrefab;
    public GameObject laserWarningPrefab;
    public GameObject laserBeamPrefab;
    public GameObject enemyPrefab;
    public GameObject Chains;
    public float projectileSpeed = 5f;
    public float rotationSpeed = 120f;

    private BossBar Bb;
    private RoomTemplates roomTemplates;
    private BossHitBox hitBox;
    private Vector3 originalPosition;

    public float initialAttackDelay = 3f;
    public float contactDamageCooldown = 1f;
    private float lastContactDamageTime;

    private LayerMask wallLayer;

    private Animator animator;

    [System.Serializable]
    public class BossAnimationSet
    {
        public int bossId;
        public string idleAnimationTrigger = "Idle";
        public string hurtAnimationTrigger = "Hurt";
        public string deathAnimationTrigger = "Death";
        public Dictionary<int, string> attackAnimationTriggers = new Dictionary<int, string>();
    }

    [System.Serializable]
    public class BossAttackAnimation
    {
        public int bossId;
        public int attackId;
        public string animationTrigger;
    }

    public List<BossAttackAnimation> bossAttackAnimations = new List<BossAttackAnimation>();
    private Dictionary<int, BossAnimationSet> bossAnimations = new Dictionary<int, BossAnimationSet>();

    [System.Serializable]
    public class AttackCombo
    {
        public List<int> attackIds = new List<int>();
        public float comboChance = 0.6f;
    }

    public List<AttackCombo> attackCombos = new List<AttackCombo>();
    public bool useAttackCombos = true;

    public float followPlayerSpeed = 2.5f;

    private GameObject Chains_;
    void Awake()
    {
        Chains_ = Instantiate(Chains,gameObject.transform.position,Quaternion.identity);
        Bb = FindObjectOfType<BossBar>();
        roomTemplates = FindObjectOfType<RoomTemplates>();
        Instantiate(BossChains, transform.position, Quaternion.identity);
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitBox = GetComponentInChildren<BossHitBox>();
        audioSource = gameObject.GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        originalPosition = transform.position;

        normalMaterial = spriteRenderer.material;

        manager = FindObjectOfType<GameManager>();
        player = FindObjectOfType<PlayerState>();
        LocalDifficulty = manager.DungeonNumber + manager.enemyDamageMultiplier + manager.enemyHealthMultiplier;

        wallLayer = LayerMask.GetMask("Wall");

        InitializeAnimationSets();
        PickBoss(Random.Range(0, BossSprites.Length));

        SetupDefaultAttackCombos();
    }

    private void InitializeAnimationSets()
    {
        for (int i = 0; i < BossSprites.Length; i++)
        {
            BossAnimationSet animSet = new BossAnimationSet();
            animSet.bossId = i;
            bossAnimations[i] = animSet;
        }

        foreach (BossAttackAnimation anim in bossAttackAnimations)
        {
            if (bossAnimations.ContainsKey(anim.bossId))
            {
                bossAnimations[anim.bossId].attackAnimationTriggers[anim.attackId] = anim.animationTrigger;
            }
        }
    }

    private void PlayAnimation(string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    private void PlayAttackAnimation(int attackPattern)
    {
        if (animator != null && bossAnimations.ContainsKey(BossId))
        {
            BossAnimationSet animSet = bossAnimations[BossId];
            if (animSet.attackAnimationTriggers.ContainsKey(attackPattern))
            {
                string triggerName = animSet.attackAnimationTriggers[attackPattern];
                animator.SetTrigger(triggerName);
            }
        }
    }

    private void SetupDefaultAttackCombos()
    {
        if (attackCombos.Count == 0)
        {
            AttackCombo moveAndSpawn = new AttackCombo();
            moveAndSpawn.attackIds.Add(1);
            moveAndSpawn.attackIds.Add(5);
            moveAndSpawn.comboChance = 0.3f;
            attackCombos.Add(moveAndSpawn);

            AttackCombo laserAndBarrage = new AttackCombo();
            laserAndBarrage.attackIds.Add(4);
            laserAndBarrage.attackIds.Add(2);
            laserAndBarrage.comboChance = 0.25f;
            attackCombos.Add(laserAndBarrage);
        }
    }

    private void SetInvulnerability(bool isInvulnerable)
    {
        Invulnerable = isInvulnerable;

        if (isInvulnerable && invulnerableMaterial != null)
        {
            spriteRenderer.material = invulnerableMaterial;
        }
        else
        {
            spriteRenderer.material = normalMaterial;
        }
    }

    private IEnumerator PerformAttack(int attackPattern)
    {
        isAttacking = true;

        PlayAttackAnimation(attackPattern);

        switch (attackPattern)
        {
            case 0:
                yield return SpiralAttack();
                break;
            case 1:
                yield return MoveAndDamagePlayer();
                break;
            case 2:
                yield return CircularBurstAttack();
                break;
            case 3:
                yield return RandomProjectileBarrage();
                break;
            case 4:
                yield return LaserBeamAttack();
                break;
            case 5:
                yield return SpawnEnemiesAttack();
                break;
        }
        isAttacking = false;
    }

    public void PickBoss(int BossID)
    {
        Invulnerable = false;
        BossId = BossID;

        if (animator != null && bossAnimations.ContainsKey(BossId))
        {
            animator.SetTrigger(bossAnimations[BossId].idleAnimationTrigger);
        }

        switch (BossId)
        {
            case 0:
                BossName = "Flesh Moth";
                spriteRenderer.sprite = BossSprites[BossID];
                BaseHealth = 200;
                BaseDamage = 1;
                BossDifficulty = 0;
                attackPatterns = new int[] { 2, 4 };
                break;
            case 1:
                BossName = "Shadow Fiend";
                spriteRenderer.sprite = BossSprites[BossID];
                BaseHealth = 225;
                BaseDamage = 1;
                CalculateBossDifficulty();
                attackPatterns = new int[] { 1, 3, 5 };
                break;
            case 2:
                BossName = "Void Devourer";
                spriteRenderer.sprite = BossSprites[BossID];
                BaseHealth = 250;
                BaseDamage = 2;
                CalculateBossDifficulty();
                attackPatterns = new int[] { 0, 2, 4, 5 };
                break;
        }

        if (LocalDifficulty < BossDifficulty)
        {
            PickBoss(Random.Range(0, BossSprites.Length));
        }
        else
        {
            MaxHealth = manager.DungeonNumber < 4 ? BaseHealth : BaseHealth * Mathf.RoundToInt(manager.enemyHealthMultiplier * 0.75f);
            Health = MaxHealth;
            AttackDamage = manager.DungeonNumber < 4 ? 1 : BaseDamage * manager.enemyDamageMultiplier;
        }

        if (Bb != null)
        {
            Bb.easeSlider.maxValue = MaxHealth;
            Bb.slider.maxValue = MaxHealth;
            Bb.maxHealth = MaxHealth;
            Bb.Health = Health;
            Bb.BossName = BossName;
        }

        if (hitBox != null)
        {
            hitBox.ChooseBossHitBox(BossId);
        }
    }

    private void StartAttackSequence()
    {
        if (attackCoroutine == null)
        {
            StartCoroutine(DelayedAttackSequence());
        }
    }

    private IEnumerator DelayedAttackSequence()
    {
        SetInvulnerability(true);
        yield return new WaitForSeconds(initialAttackDelay);
        SetInvulnerability(false);
        attackCoroutine = StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        while (true)
        {
            bool usedSpecialAttack = false;

            foreach (int pattern in attackPatterns)
            {
                if (pattern == 0)
                {
                    SetInvulnerability(true);
                    yield return PerformAttack(pattern);
                    SetInvulnerability(false);

                    if (Vector3.Distance(transform.position, originalPosition) > 0.5f)
                    {
                        yield return ReturnToOriginalPosition();
                    }

                    yield return new WaitForSeconds(2f);
                    usedSpecialAttack = true;
                }
                else if (useAttackCombos && attackCombos.Count > 0 && Random.value <= GetHighestComboChance() && !usedSpecialAttack)
                {
                    AttackCombo selectedCombo = GetRandomCombo();
                    if (selectedCombo != null && selectedCombo.attackIds.Count > 0 && !selectedCombo.attackIds.Contains(0))
                    {
                        yield return PerformAttackCombo(selectedCombo);
                        usedSpecialAttack = true;
                    }
                    else
                    {
                        if (pattern == 0)
                        {
                            SetInvulnerability(true);
                        }

                        yield return PerformAttack(pattern);

                        SetInvulnerability(false);

                        if (Vector3.Distance(transform.position, originalPosition) > 0.5f)
                        {
                            yield return ReturnToOriginalPosition();
                        }

                        yield return new WaitForSeconds(0.5f);
                    }
                }
                else
                {
                    if (pattern == 0)
                    {
                        SetInvulnerability(true);
                    }

                    yield return PerformAttack(pattern);

                    SetInvulnerability(false);

                    if (Vector3.Distance(transform.position, originalPosition) > 0.5f)
                    {
                        yield return ReturnToOriginalPosition();
                    }

                    yield return new WaitForSeconds(2f);
                }
            }
        }
    }

    private float GetHighestComboChance()
    {
        float highestChance = 0f;
        foreach (AttackCombo combo in attackCombos)
        {
            if (combo.comboChance > highestChance)
            {
                highestChance = combo.comboChance;
            }
        }
        return highestChance;
    }

    private AttackCombo GetRandomCombo()
    {
        List<AttackCombo> validCombos = new List<AttackCombo>();
        foreach (AttackCombo combo in attackCombos)
        {
            if (Random.value <= combo.comboChance && !combo.attackIds.Contains(0))
            {
                validCombos.Add(combo);
            }
        }

        if (validCombos.Count > 0)
        {
            return validCombos[Random.Range(0, validCombos.Count)];
        }

        return null;
    }

    private IEnumerator PerformAttackCombo(AttackCombo combo)
    {
        SetInvulnerability(true);

        List<Coroutine> activeAttacks = new List<Coroutine>();

        foreach (int attackId in combo.attackIds)
        {
            if (attackId != 0)
            {
                activeAttacks.Add(StartCoroutine(PerformAttack(attackId)));
                yield return new WaitForSeconds(0.5f);
            }
        }

        foreach (Coroutine attack in activeAttacks)
        {
            yield return attack;
        }

        SetInvulnerability(false);

        if (Vector3.Distance(transform.position, originalPosition) > 0.5f)
        {
            yield return ReturnToOriginalPosition();
        }

        yield return new WaitForSeconds(3f);
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        float moveSpeed = 4f;
        float distanceThreshold = 0.1f;

        while (Vector3.Distance(transform.position, originalPosition) > distanceThreshold)
        {
            Vector3 direction = (originalPosition - transform.position).normalized;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, moveSpeed * Time.deltaTime, wallLayer);
            if (hit.collider == null)
            {
                transform.position = Vector3.MoveTowards(transform.position, originalPosition, moveSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 alternativeDirection = new Vector3(direction.y, -direction.x, 0);
                transform.position += alternativeDirection * moveSpeed * Time.deltaTime;
            }

            yield return null;
        }

        transform.position = originalPosition;
    }

    private IEnumerator SpiralAttack()
    {
        return TeleportationBarrage();
    }

    private IEnumerator TeleportationBarrage()
    {
        int teleportCount = 8;
        float attackDuration = 8f;

        Vector3 originalPos = transform.position;

        for (int i = 0; i < teleportCount; i++)
        {
            Color originalColor = spriteRenderer.color;
            for (int flash = 0; flash < 2; flash++)
            {
                spriteRenderer.color = new Color(0.8f, 0.2f, 0.8f, 0.7f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }

            Vector2 teleportOffset = Random.insideUnitCircle.normalized * Random.Range(3f, 7f);
            Vector2 teleportPosition = (Vector2)originalPos + teleportOffset;

            RaycastHit2D hit = Physics2D.Raycast(originalPos, teleportOffset.normalized, teleportOffset.magnitude, wallLayer);
            if (hit.collider != null)
            {
                teleportPosition = hit.point - teleportOffset.normalized * 1.5f;
            }

            if (teleportSound != null)
            {
                audioSource.PlayOneShot(teleportSound);
            }

            transform.position = teleportPosition;

            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;

            int burstCount = Random.Range(4, 9);
            float angleStep = 360f / burstCount;

            for (int j = 0; j < burstCount; j++)
            {
                float randomAngle = j * angleStep + Random.Range(-15f, 15f);
                Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
                Vector2 spawnPosition = (Vector2)transform.position + direction * 1.2f;

                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>() ?? projectile.AddComponent<Rigidbody2D>();
                rb.velocity = direction * projectileSpeed * 1.2f;
                rb.angularVelocity = rotationSpeed;

                if (projectileSound != null)
                {
                    audioSource.PlayOneShot(projectileSound, 0.5f);
                }

                Destroy(projectile, 5f);
            }

            if (i % 2 == 1 && player != null)
            {
                Vector2 playerDirection = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                Vector2 playerSpawnPos = (Vector2)transform.position + playerDirection * 1.2f;

                GameObject playerTargetedProjectile = Instantiate(projectilePrefab, playerSpawnPos, Quaternion.identity);
                Rigidbody2D rb = playerTargetedProjectile.GetComponent<Rigidbody2D>() ?? playerTargetedProjectile.AddComponent<Rigidbody2D>();
                rb.velocity = playerDirection * projectileSpeed * 1.4f;
                rb.angularVelocity = rotationSpeed;

                if (projectileSound != null)
                {
                    audioSource.pitch = 1.2f;
                    audioSource.PlayOneShot(projectileSound, 0.7f);
                    audioSource.pitch = 1.0f;
                }

                Destroy(playerTargetedProjectile, 5f);
            }

            yield return new WaitForSeconds(attackDuration / teleportCount - 0.4f);
        }

        if (teleportSound != null)
        {
            audioSource.PlayOneShot(teleportSound, 0.8f);
        }

        transform.position = originalPos;
    }

    private IEnumerator CircularBurstAttack()
    {
        int burstCount = 3;

        for (int burst = 0; burst < burstCount; burst++)
        {
            int projectileCount = 8;
            float angleStep = 360f / projectileCount;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = i * angleStep;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector2 spawnPosition = (Vector2)transform.position + direction * 1.5f;

                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>() ?? projectile.AddComponent<Rigidbody2D>();
                rb.velocity = direction * projectileSpeed;
                rb.angularVelocity = rotationSpeed;

                if (projectileSound != null)
                {
                    audioSource.PlayOneShot(projectileSound, 0.4f);
                }

                Destroy(projectile, 5f);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator RandomProjectileBarrage()
    {
        int barrageCount = 50;

        for (int i = 0; i < barrageCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle.normalized * 5f;
            Vector2 targetPosition = player ? (Vector2)player.transform.position : Random.insideUnitCircle * 5f;

            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>() ?? projectile.AddComponent<Rigidbody2D>();
            Vector2 direction = (targetPosition - spawnPosition).normalized;
            rb.velocity = direction * projectileSpeed * 1.5f;
            rb.angularVelocity = rotationSpeed;

            if (projectileSound != null)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(projectileSound, 0.3f);
                audioSource.pitch = 1.0f;
            }

            Destroy(projectile, 5f);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator LaserBeamAttack()
    {
        if (laserWarningPrefab == null || laserBeamPrefab == null)
        {
            Debug.LogWarning("LaserWarningPrefab or LaserBeamPrefab is not assigned!");
            yield break;
        }

        int laserCount = 50;

        int waves = 5;
        float timeBetweenWaves = 0.8f;

        for (int wave = 0; wave < waves; wave++)
        {
            for (int i = 0; i < laserCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 8f;
                Vector2 targetPosition;

                if (player && Random.value < 0.7f)
                {
                    targetPosition = (Vector2)player.transform.position + Random.insideUnitCircle * 3f;
                }
                else
                {
                    targetPosition = randomOffset;
                }

                GameObject warning = Instantiate(laserWarningPrefab, targetPosition, Quaternion.identity);

                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnEnemiesAttack()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyPrefab is not assigned!");
            yield break;
        }

        int enemyCount = 2 + Mathf.FloorToInt(manager.DungeonNumber / 3f);
        enemyCount = Mathf.Clamp(enemyCount, 2, 5);

        SetInvulnerability(true);
        spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        for (int pulse = 0; pulse < 3; pulse++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }

        for (int i = 0; i < enemyCount; i++)
        {
            float angle = i * (360f / enemyCount);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 spawnPosition = (Vector2)transform.position + direction * 3f;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 3f, wallLayer);
            if (hit.collider != null)
            {
                spawnPosition = (Vector2)transform.position + direction * (hit.distance - 0.5f);
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            if (spawnEnemySound != null)
            {
                audioSource.PlayOneShot(spawnEnemySound, 0.7f);
            }

            Enemy enemyAI = enemy.GetComponent<Enemy>();
            if (enemyAI != null)
            {
                enemyAI.Health = Mathf.CeilToInt(enemyAI.Health * 0.7f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        SetInvulnerability(false);
    }

    private void CalculateBossDifficulty()
    {
        BossDifficulty = (BaseHealth / 5) + (2 * BaseDamage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = true;

            if (!isAttacking)
            {
                isAttacking = true;
                StartAttackSequence();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (Invulnerable) return;

        Bb.TakeDamage(damage);
        Health -= damage;

        StartCoroutine(DamageFlash());

        if (Health <= 0) Die();
    }

    private IEnumerator DamageFlash()
    {
        if (animator != null && bossAnimations.ContainsKey(BossId))
        {
            PlayAnimation(bossAnimations[BossId].hurtAnimationTrigger);
        }

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    public void Die()
    {
        if (animator != null && bossAnimations.ContainsKey(BossId))
        {
            PlayAnimation(bossAnimations[BossId].deathAnimationTrigger);
            StartCoroutine(DieAfterAnimation());
        }
        else
        {
            FinalizeDeath();
        }
    }

    private IEnumerator DieAfterAnimation()
    {
        float animLength = 1.0f;
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animLength = stateInfo.length;
        }

        yield return new WaitForSeconds(animLength);
        FinalizeDeath();
    }

    private void FinalizeDeath()
    {
        if (roomTemplates.rooms != null && roomTemplates.rooms.Count > 0)
        {
            GameObject exit = Instantiate(ExitHand, roomTemplates.rooms[^1].transform.position, Quaternion.identity, roomTemplates.rooms[^1].gameObject.transform);

            if (!PlayerPrefs.HasKey("VoidPortalSpawned"))
            {
                float portalProbability = CalculateVoidPortalProbability(manager.DungeonNumber);

                if (Random.value <= portalProbability)
                {
                    Instantiate(VoidPortal, roomTemplates.rooms[^1].transform.position + new Vector3(-3, 3, 0), Quaternion.identity, roomTemplates.rooms[^1].gameObject.transform);
                    PlayerPrefs.SetInt("VoidPortalSpawned", 1);
                    PlayerPrefs.Save();
                }
            }
        }
        else
        {
            roomTemplates.DeleteAllRooms(false);
        }
        Destroy(Chains_,1.5f);
        Destroy(GameObject.Find("BossChains(Clone)"));
        Bb.HideBossBar();
        Destroy(gameObject);
    }

    private float CalculateVoidPortalProbability(int currentDungeonNumber)
    {
        if (currentDungeonNumber < 1) return 0f;
        if (currentDungeonNumber >= 14) return 1f;
        float probability = (currentDungeonNumber - 1) / 13f;
        return probability;
    }

    private void DealDamage()
    {
        PlayerState p = FindObjectOfType<PlayerState>();
        if (p != null)
        {
            p.TakeDamage(AttackDamage);
        }
    }

    private IEnumerator MoveAndDamagePlayer()
    {
        Vector3 startPosition = transform.position;
        float duration = 8f;
        float elapsedTime = 0f;
        bool playerDamaged = false;

        while (elapsedTime < duration && player != null)
        {
            Vector3 playerPosition = player.transform.position;
            Vector3 direction = (playerPosition - transform.position).normalized;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, followPlayerSpeed * Time.deltaTime, wallLayer);

            if (hit.collider == null)
            {
                transform.position += direction * followPlayerSpeed * Time.deltaTime;

                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer < 1.5f && !playerDamaged)
                {
                    DealDamage();
                    playerDamaged = true;

                    yield return new WaitForSeconds(1f);
                    playerDamaged = false;
                }
            }
            else
            {
                Vector3 alternativeDirection = new Vector3(direction.y, -direction.x, 0);
                transform.position += alternativeDirection * (followPlayerSpeed * 0.5f) * Time.deltaTime;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return ReturnToOriginalPosition();
    }

    private void ShootFourProjectiles(float angleStep, bool clockwise)
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = angleStep + i * 90;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 spawnPosition = (Vector2)transform.position + direction * 2f;
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>() ?? projectile.AddComponent<Rigidbody2D>();
            rb.velocity = direction * projectileSpeed;
            rb.angularVelocity = rotationSpeed;
            if (clockwise)
            {
                rb.angularVelocity = rotationSpeed;
            }
            else
            {
                rb.angularVelocity = -rotationSpeed;
            }

            if (projectileSound != null)
            {
                audioSource.PlayOneShot(projectileSound, 0.5f);
            }

            Destroy(projectile, 5f);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastContactDamageTime + contactDamageCooldown)
            {
                DealDamage();
                lastContactDamageTime = Time.time;
            }
        }
    }

    void Update()
    {
        if (playerDetected && !isAttacking && attackCoroutine == null)
        {
            StartAttackSequence();
        }

        if (Health > 0 && Bb != null)
        {
            Bb.Health = Health;
        }
    }

    public int GetCurrentHealth()
    {
        return Health;
    }

    public int GetMaxHealth()
    {
        return MaxHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)Health / MaxHealth;
    }

    public void AddHealth(int amount)
    {
        Health = Mathf.Min(Health + amount, MaxHealth);
        if (Bb != null)
        {
            Bb.Health = Health;
        }
    }

    public void SetHealth(int newHealth)
    {
        Health = Mathf.Clamp(newHealth, 0, MaxHealth);
        if (Bb != null)
        {
            Bb.Health = Health;
        }
    }

    public bool IsInvulnerable()
    {
        return Invulnerable;
    }

    void OnDestroy()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }
}