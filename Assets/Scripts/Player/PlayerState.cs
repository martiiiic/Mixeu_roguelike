using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    enum EstatDelJugador { idle, Move, dead }

    bool StateComplete;
    bool InputPressed;
    bool finalXinputPressed;

    public bool Dead;
    public bool Rolling;
    public bool Invulnerable;

    EstatDelJugador state;

    public float xInput = 0;
    public float yInput = 0;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;

    public Sprite[] CatSprites;
    public GameObject[] Particles;

    private PlayerStats Stats;
    private AngelDeath Angel;
    private RoomTemplates Dungeon;

    [Header("Dodge System")]
    public float DashSpeed = 15f;
    public float DashDuration = 0.05f;
    public float DashCooldown = 0.75f;
    public float AfterImageFrequency = 0.05f;
    private bool CanDash = true;
    private float lastDashTime = -10f;

    public bool EnableAfterImages = true;
    public GameObject AfterImagePrefab;

    [Header("Damage System")]
    public float DamageCooldownDuration = 1f;
    public bool DamageCooldown = true;
    public float PerfectDodgeWindow = 0.5f;
    public float LastEnemyAttackTime = -10f;

    [Header("Perfect Dodge Effects")]
    public GameObject PerfectDodgeExplosionPrefab;

    private int damageAbsorbed;
    private float defenseResetTimer = 10f;
    private float lastDamageTime;

    [Header("Audio")]
    public AudioClip[] SFX;
    public AudioClip PerfectDodgeSFX;
    private AudioSource Audio;

    [Header("Visual Effects")]
    public Material WhiteMaterial;
    public Material DefaultMaterial;
    public Material DodgeMaterial;
    public GameObject DodgeEffectPrefab;


    DeathScreenScript DS;

    private void Awake()
    {
        Audio = GetComponent<AudioSource>();
        transform.position = new Vector3(0, 0, -1);
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Angel = GetComponentInChildren<AngelDeath>();
        Stats = GetComponent<PlayerStats>();
        Dungeon = FindObjectOfType<RoomTemplates>();
        DefaultMaterial = spriteRenderer.material;

        if (DodgeMaterial == null)
            DodgeMaterial = WhiteMaterial;
    }

    private void Start()
    {
        Audio = GetComponent<AudioSource>();
        transform.position = new Vector3(0, 0, -1);
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Angel = GetComponentInChildren<AngelDeath>();
        Stats = GetComponent<PlayerStats>();
        Dungeon = FindObjectOfType<RoomTemplates>();
        DefaultMaterial = spriteRenderer.material;
    }

    private void Update()
    {
        if (Time.timeScale <= 0.01) { state = EstatDelJugador.idle; rb.velocity = Vector2.zero; SelectState(); StateComplete = true; return; }

        CheckInputs();

        if (Stats.CurrentHealth <= 0 && !Dead) { Die(); }

        if (StateComplete)
        {
            SelectState();
        }
        UpdateState();

        if (Time.time - lastDamageTime >= defenseResetTimer)
        {
            damageAbsorbed = 0;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && CanDash && !Rolling && !Dead)
        {
            PerformDash();
        }
    }

    public void RegisterEnemyAttack()
    {
        LastEnemyAttackTime = Time.time;
    }

    private void PerformDash()
    {
        Vector2 dashDirection = new Vector2(xInput, yInput).normalized;

        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = finalXinputPressed ? Vector2.right : Vector2.left;
        }

        float dashSpeedMultiplier = Stats.DashSpeed + DashSpeed;
        rb.velocity = dashDirection * dashSpeedMultiplier;

        bool isPerfectDodge = (Time.time - LastEnemyAttackTime) <= PerfectDodgeWindow;

        Invulnerable = true;
        Rolling = true;
        CanDash = false;
        lastDashTime = Time.time;

        DynamicCamera camera = FindObjectOfType<DynamicCamera>();
        if (camera != null)
        {
            camera.ApplyShake(camera.dashShakeIntensity);
        }

        if (isPerfectDodge && !perfectDodgeEffectSpawned)
        {
            perfectDodgeEffectSpawned = true;

            if (PerfectDodgeSFX != null)
            {
                Audio.clip = PerfectDodgeSFX;
                Audio.Play();
            }

            SpawnPerfectDodgeEffects();

            Invoke("ResetPerfectDodgeFlag", DashDuration + 0.5f);

            StartCoroutine(ExtendInvulnerability(DashDuration + 0.3f));
        }
        else
        {
            Audio.clip = SFX[UnityEngine.Random.Range(3, 5)];
            Audio.Play();
        }

        RollAnimation();
        spriteRenderer.material = DodgeMaterial;

        if (EnableAfterImages && AfterImagePrefab != null)
        {
            StartCoroutine(CreateAfterImages());
        }

        Invoke("StopRolling", DashDuration);
        Invoke("ResetDash", DashCooldown);
        StateComplete = true;
    }

    private void SpawnPerfectDodgeEffects()
    {
        if (PerfectDodgeExplosionPrefab != null)
        {
            Instantiate(PerfectDodgeExplosionPrefab, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator CreateAfterImages()
    {
        float endTime = Time.time + DashDuration;

        while (Time.time < endTime && Rolling)
        {
            GameObject afterImage = Instantiate(AfterImagePrefab, transform.position, transform.rotation);
            SpriteRenderer imageSR = afterImage.GetComponent<SpriteRenderer>();

            if (imageSR != null)
            {
                imageSR.sprite = spriteRenderer.sprite;
                imageSR.flipX = spriteRenderer.flipX;

                Destroy(afterImage, 0.3f);
            }

            yield return new WaitForSeconds(AfterImageFrequency);
        }
    }

    private bool perfectDodgeEffectSpawned = false;

    private IEnumerator ExtendInvulnerability(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!Rolling)
        {
            Invulnerable = false;
        }
    }

    public void TakeDamage(int Damage)
    {
        if (Time.time - lastDashTime <= PerfectDodgeWindow + DashDuration && !perfectDodgeEffectSpawned)
        {
            perfectDodgeEffectSpawned = true;
            SpawnPerfectDodgeEffects();
            if (PerfectDodgeSFX != null)
            {
                Audio.PlayOneShot(PerfectDodgeSFX);
            }
            Invoke("ResetPerfectDodgeFlag", 0.5f);
            return;
        }
        if (DamageCooldown)
        {
            if (Rolling || Invulnerable) { return; }
            lastDamageTime = Time.time;
            LastEnemyAttackTime = Time.time;
            bool anyDamageBlocked = false;
            int actualDamage = 0;

            // Position-based block chances for each hit with logarithmic scaling
            for (int i = 0; i < Damage; i++)
            {
                float blockChance = 0f;

                // Only calculate block chance if player has defense
                if (Stats.Defense > 0)
                {
                    // Calculate base chance using logarithmic function
                    float baseBlockChance = 0.8f * (1f - 1f / (1f + 0.3f * Mathf.Log(1f + Stats.Defense)));

                    // Add position bonus
                    float positionBonus = i * 0.1f; // 10% per hit position
                    blockChance = baseBlockChance + positionBonus;

                    // Cap at 90% to prevent 100% block chance
                    blockChance = Mathf.Min(0.9f, blockChance);
                }

                if (UnityEngine.Random.value < blockChance)
                {
                    anyDamageBlocked = true;
                }
                else
                {
                    actualDamage++;
                }
            }

            ShieldDamageAbsorb SDA = GetComponentInChildren<ShieldDamageAbsorb>();
            if (anyDamageBlocked)
            {
                SDA.ShowShield();
                if (SFX.Length > 5 && SFX[5] != null)
                {
                    Audio.clip = SFX[5];
                    Audio.Play();
                }
            }
            if (actualDamage > 0)
            {
                Stats.CurrentHealth -= actualDamage;
                Audio.clip = SFX[0];
                Audio.Play();
                StartCoroutine(PauseGame(0.075f));
                StartCoroutine(FlashColor(Color.red, 0.1f));
            }
            CanDash = false;
            DamageCooldown = false;
            StartCoroutine(DamageCooldownTimer());
            Invoke("ResetDash", DashCooldown);
            if (Stats.CurrentHealth < 0)
            {
                Stats.CurrentHealth = 0;
            }
        }
    }

    public string GetDefenseChance()
    {
        float blockChance = 0f;

        if (Stats.Defense > 0)
        {
            // Calculate only the first hit block chance to display
            blockChance = 0.8f * (1f - 1f / (1f + 0.3f * Mathf.Log(1f + Stats.Defense)));

            // Cap at 90%
            blockChance = Mathf.Min(0.9f, blockChance);
        }

        float percentage = blockChance * 100f;
        return percentage.ToString("F1") + "%";
    }

    public void Die()
    {
        DS = FindObjectOfType<DeathScreenScript>();
        PowerUpStoreUI PS = FindObjectOfType<PowerUpStoreUI>();
        if (PS != null)
        {
            PS.CloseMenu();
        }
        WeaponTypes WT = FindObjectOfType<WeaponTypes>();
        WT.Wrarity = "Common";
        Particles[0].SetActive(false);
        rb.velocity = Vector3.zero;
        Dead = true;
        StateComplete = true;
        Angel.AngelBehaviour();
        DS.StartCoroutine(DS.ShowDeathScreen());
        StartCoroutine(DeathToGame());
    }

    private IEnumerator DeathToGame()
    {
        yield return new WaitForSeconds(5);
        spriteRenderer.sprite = CatSprites[0];
        Stats.ResetStats();
        Dead = false;
        StateComplete = true;
        Dungeon.DeleteAllRooms(true);
    }

    private IEnumerator DamageCooldownTimer()
    {
        yield return new WaitForSeconds(DamageCooldownDuration);
        DamageCooldown = true;
    }

    private IEnumerator FlashColor(Color color, float duration)
    {
        spriteRenderer.color = color;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = Color.white;
    }

    private IEnumerator PauseGame(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    void UpdateState()
    {
        switch (state)
        {
            case EstatDelJugador.idle:
                UpdateIdle();
                break;
            case EstatDelJugador.Move:
                UpdateRun();
                break;
            case EstatDelJugador.dead:
                UpdateDead();
                break;
        }
    }

    void SelectState()
    {
        StateComplete = false;

        if (!Dead)
        {
            if (InputPressed)
            {
                state = EstatDelJugador.Move;
                StartMoving();
            }
            else
            {
                Particles[0].SetActive(false);
                state = EstatDelJugador.idle;
                StartIdle();
            }
        }
        else if (Dead)
        {
            state = EstatDelJugador.dead;
            StartDead();
        }
    }

    void UpdateIdle()
    {
        spriteRenderer.flipX = finalXinputPressed;
        if (InputPressed)
        {
            StateComplete = true;
        }
    }

    void StartIdle()
    {

    }

    public float speedMultiplier;

    void UpdateRun()
    {
        if (InputPressed)
        {
            bool xInput_ = xInput > 0;
            spriteRenderer.flipX = xInput_;

            if (xInput_)
            {
                Vector3 newPosition = transform.position;
                newPosition.y -= 0.35f;
                newPosition.x -= 0.35f;
                Particles[0].transform.position = newPosition;

                Vector3 currentRotation = Particles[0].transform.rotation.eulerAngles;
                currentRotation.y = -90f;
                Particles[0].transform.rotation = Quaternion.Euler(currentRotation);
            }
            else
            {
                Vector3 newPosition = transform.position;
                newPosition.y -= 0.35f;
                newPosition.x += 0.35f;
                Particles[0].transform.position = newPosition;

                Vector3 currentRotation = Particles[0].transform.rotation.eulerAngles;
                currentRotation.y = 90f;
                Particles[0].transform.rotation = Quaternion.Euler(currentRotation);
            }

            finalXinputPressed = xInput_;

            bool yInput_ = yInput > 0;
            if (yInput_)
            {
                spriteRenderer.sprite = CatSprites[1];
            }
            else
            {
                spriteRenderer.sprite = CatSprites[0];
            }
        }

        

        WeaponTypes WT = FindObjectOfType<WeaponTypes>();
        if (!Rolling)
        {
            if (Time.timeScale <= 0) { return; }
            float moveSpeed = 50 * Stats.Speed * WT.PlayerSpeedMultiplier * speedMultiplier * Time.fixedDeltaTime;
            rb.velocity = new Vector2(xInput * moveSpeed, yInput * moveSpeed);

            Particles[0].SetActive(true);
            ParticleSystem RunParticles = Particles[0].GetComponent<ParticleSystem>();
            ParticleSystem.NoiseModule noiseModule = RunParticles.noise;
            noiseModule.sizeAmount = Stats.Speed * 0.25f;
        }

        if (!InputPressed)
        {
            spriteRenderer.flipX = finalXinputPressed;
            StateComplete = true;
        }
    }

    void RollAnimation()
    {
        spriteRenderer.sprite = CatSprites[3];
    }

    void StopRolling()
    {
        Rolling = false;
        Invoke("DisableInvulnerability", 0.1f);
        spriteRenderer.material = DefaultMaterial;
    }

    void DisableInvulnerability()
    {
        Invulnerable = false;
    }

    void ResetDash()
    {
        CanDash = true;
        spriteRenderer.material = DefaultMaterial;
    }

    void StartMoving()
    {

    }

    void UpdateDead()
    {
        if (!Dead)
        {
            StateComplete = true;
        }
    }

    void StartDead()
    {
        spriteRenderer.sprite = CatSprites[4];
    }

    void CheckInputs()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (xInput != 0 || yInput != 0)
        {
            if (!Rolling)
            {
                InputPressed = true;
            }
        }
        else
        {
            InputPressed = false;
        }
    }

    private void ResetPerfectDodgeFlag()
    {
        perfectDodgeEffectSpawned = false;
    }
}