using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerUpStore : MonoBehaviour
{
    [Header("Settings")]
    public PowerUpDefinition powerUpDefinition;
    public bool shouldLoadRandomPowerUp = true;  // Set to true to load a random power-up at start
    public bool isVoidBoundStore = false;        // Set to true if this store sells void bound power-ups

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public TextMeshPro nameText;
    public TextMeshPro priceText;                // Add this if you have a price display text

    [Header("Runtime Data - Don't modify in Inspector")]
    public int calculatedPrice;
    private Animator animator;
    private PowerUpStoreUI powerUpUI;
    private GameManager gameManager;
    private RoomTemplates rooms;
    private bool playerIsClose = false;
    private PowerUpManager powerUpManager;

    private void Awake()
    {
        playerIsClose = false;
        gameManager = FindFirstObjectByType<GameManager>();
        powerUpUI = FindAnyObjectByType<PowerUpStoreUI>();

        if (powerUpUI == null)
        {
            Debug.LogError("Failed to find PowerUpStoreUI in the scene!");
        }
        else
        {
            Debug.Log("Successfully found PowerUpStoreUI reference");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on this GameObject!");
        }

        // Rest of your Awake code...
    }

    private void Start()
    {
        // If no powerup is set and we should load randomly, get a random one
        if (powerUpDefinition == null && shouldLoadRandomPowerUp)
        {
            LoadRandomPowerUp();
        }
        else if (powerUpDefinition != null)
        {
            InitializeFromDefinition();
        }
        else
        {
            Debug.LogWarning("No PowerUpDefinition assigned to PowerUpStore and random loading is disabled.");
        }
    }

    public void LoadRandomPowerUp()
    {
        if (powerUpManager == null)
        {
            powerUpManager = PowerUpManager.Instance;
            if (powerUpManager == null)
            {
                Debug.LogError("PowerUpManager instance not found!");
                return;
            }
        }

        List<PowerUpDefinition> availablePowerUps;

        if (isVoidBoundStore)
        {
            availablePowerUps = powerUpManager.GetVoidBoundPowerUps();
        }
        else
        {
            availablePowerUps = powerUpManager.GetNormalPowerUps();
        }

        if (availablePowerUps.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePowerUps.Count);
            powerUpDefinition = availablePowerUps[randomIndex];

            Debug.Log($"Selected random power-up: {powerUpDefinition.displayName} with ID: {powerUpDefinition.id}");
            InitializeFromDefinition();
        }
        else
        {
            Debug.LogError($"No {(isVoidBoundStore ? "void bound" : "normal")} power-ups found to select from!");

            // Set fallback appearance
            if (spriteRenderer != null)
                spriteRenderer.sprite = null;

            if (nameText != null)
                nameText.text = "No PowerUps";
        }
    }

    public void InitializeFromDefinition()
    {
        if (powerUpDefinition != null)
        {
            if (spriteRenderer != null)
            {
                if (powerUpDefinition.icon != null)
                {
                    spriteRenderer.sprite = powerUpDefinition.icon;
                }
                else
                {
                    Debug.LogError($"PowerUp {powerUpDefinition.displayName} has no icon!");
                    spriteRenderer.sprite = null;
                }
            }

            if (nameText != null)
                nameText.text = powerUpDefinition.displayName;

            if (gameManager != null)
            {
                calculatedPrice = powerUpDefinition.basePrice * gameManager.PriceMultiplier;
                ControlPricesPerRoomCount();

                if (priceText != null)
                    priceText.text = calculatedPrice.ToString();
            }

            Debug.Log($"Initialized store with power-up: {powerUpDefinition.displayName}, Icon null? {powerUpDefinition.icon == null}");
        }
        else
        {
            Debug.LogError("Tried to initialize PowerUpStore with null PowerUpDefinition!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            powerUpUI.Store = this;
            powerUpUI.ShowMenu(0);
            animator.SetBool("Hover", true);
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            powerUpUI.CloseMenu();
            animator.SetBool("Hover", false);
            playerIsClose = false;
        }
    }

    private void Update()
    {
        if (playerIsClose)
        {
            powerUpUI.Store = this;
            powerUpUI.ShowMenu(0);
            PlayerStats stats = FindObjectOfType<PlayerStats>();
            if (stats != null && stats.CurrentHealth <= 0)
            {
                playerIsClose = false;
                powerUpUI.CloseMenu();
            }
        }
    }

    public void ControlPricesPerRoomCount()
    {
        rooms = FindObjectOfType<RoomTemplates>();
        if (rooms != null && rooms.rooms.Count > 10)
        {
            float priceF = calculatedPrice * Mathf.Log10(10 + (rooms.rooms.Count * 0.15f));
            calculatedPrice = Mathf.RoundToInt(priceF);
        }
    }

    public void ApplyPowerUp(PlayerStats playerStats, SpecialPlayerStats specialStats)
    {
        if (powerUpDefinition == null) return;

        foreach (var modifier in powerUpDefinition.statModifiers)
        {
            ApplyStatModifier(playerStats, specialStats, modifier);
        }

        // Handle special powerup effects
        if (powerUpDefinition.isSpecialPowerUp && !string.IsNullOrEmpty(powerUpDefinition.specialStatName))
        {
            specialStats.SetStat(powerUpDefinition.specialStatName, true);
        }

        // Track applied power-ups if it's a void bound power-up
        if (powerUpDefinition.isVoidBound)
        {
            VoidBoundEvents voidEvents = FindObjectOfType<VoidBoundEvents>();
            if (voidEvents != null)
            {
                voidEvents.AddEvent(powerUpDefinition.displayName);
            }
        }

        // Update UI info if available
        DungeonInfo dungeonInfo = FindObjectOfType<DungeonInfo>();
        if (dungeonInfo != null)
        {
            dungeonInfo.UpdateRunInfo();
        }
    }

    private void ApplyStatModifier(PlayerStats playerStats, SpecialPlayerStats specialStats, PowerUpDefinition.StatModifier modifier)
    {
        float value = modifier.value;

        // Apply the value to the appropriate stat
        switch (modifier.statToModify)
        {
            // Player Stats
            case PowerUpDefinition.StatModifier.StatType.MaxHealth:
                playerStats.MaxHealth += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.CurrentHealth:
                playerStats.CurrentHealth += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.Speed:
                if (!modifier.useRawValue)
                    value = value * 0.5f;
                playerStats.Speed += value;
                break;
            case PowerUpDefinition.StatModifier.StatType.AttackSpeed:
                if (!modifier.useRawValue)
                    value = value * 0.8f;
                playerStats.AttackSpeed += value;
                break;
            case PowerUpDefinition.StatModifier.StatType.RangedSpeed:
                if (!modifier.useRawValue)
                    value = value * 0.75f;
                playerStats.RangedSpeed += value;
                break;
            case PowerUpDefinition.StatModifier.StatType.AttackDamage:
                if (!modifier.useRawValue)
                {
                    GameManager manager = FindObjectOfType<GameManager>();
                    value += Mathf.RoundToInt(0.25f * manager.enemyDamageMultiplier);
                }
                playerStats.AttackDamage += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.Defense:
                playerStats.Defense += value;
                break;
            case PowerUpDefinition.StatModifier.StatType.DashSpeed:
                if (!modifier.useRawValue)
                    value = value * 0.75f;
                playerStats.DashSpeed += value;
                break;

            // Special Stats
            case PowerUpDefinition.StatModifier.StatType.BurnPercentage:
                specialStats.BurnPercentage += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.BurnDamageFactor:
                specialStats.BurnDamageFactor += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.BurnLongevityFactor:
                specialStats.BurnLongevityFactor += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.BurnSpeedFactor:
                specialStats.BurnSpeedFactor += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.PoisonPercentage:
                specialStats.PoisonPercentage += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.PoisonDamageFactor:
                specialStats.PoisonDamageFactor += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.PoisonLongevityFactor:
                specialStats.PoisonLongevityFactor += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.PoisonSpeedFactor:
                specialStats.PoisonSpeedFactor += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.BurstSpeedFactor:
                specialStats.BurstSpeedFactor += Mathf.RoundToInt(value);
                break;
            case PowerUpDefinition.StatModifier.StatType.BurstSpeedLongevityFactor:
                specialStats.BurstSpeedLongevityFactor += Mathf.RoundToInt(value);
                break;
        }
    }

    public void SetPowerUpDefinition(PowerUpDefinition definition)
    {
        powerUpDefinition = definition;
        InitializeFromDefinition();
    }
}