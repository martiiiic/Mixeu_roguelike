using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoidBoundEvents : MonoBehaviour
{
    private PlayerStats stats;
    private HealthSystem healthSystem;
    private PowerUpManager powerUpManager;

    private Dictionary<int, bool> activeEffects = new Dictionary<int, bool>();
    private Dictionary<string, int> effectNameToId = new Dictionary<string, int>();

    private void Start()
    {
        stats = FindObjectOfType<PlayerStats>();
        healthSystem = FindObjectOfType<HealthSystem>();
        powerUpManager = PowerUpManager.Instance;

        activeEffects = new Dictionary<int, bool>();
        effectNameToId = new Dictionary<string, int>();

        InitializeEffectMapping();
    }

    private void InitializeEffectMapping()
    {
        List<PowerUpDefinition> voidBoundPowerUps = powerUpManager.GetVoidBoundPowerUps();
        foreach (PowerUpDefinition powerUp in voidBoundPowerUps)
        {

            if (powerUp.displayName == "Cesk's Kidney")
            {
                effectNameToId["Cesk's Kidney"] = powerUp.id;
            }
            activeEffects[powerUp.id] = false;
        }
    }

    public void ActivateHealthVoidBindingEffect()
    {
        if (!IsEffectActive("Cesk's Kidney") && effectNameToId.ContainsKey("Cesk's Kidney"))
        {
            int powerUpId = effectNameToId["Cesk's Kidney"];
            activeEffects[powerUpId] = true;

            PowerUpDefinition kidneyPowerUp = powerUpManager.GetPowerUpById(powerUpId, true);

            if (kidneyPowerUp != null)
            {
                ApplyPowerUpStats(kidneyPowerUp);
            }

            healthSystem.ActivateVoidBound();
        }
    }

    public void DeactivateHealthVoidBindingEffect()
    {
        if (IsEffectActive("Kidney") && effectNameToId.ContainsKey("Kidney"))
        {
            int powerUpId = effectNameToId["Kidney"];
            activeEffects[powerUpId] = false;

            PowerUpDefinition kidneyPowerUp = powerUpManager.GetPowerUpById(powerUpId, true);

            if (kidneyPowerUp != null)
            {
                RemovePowerUpStats(kidneyPowerUp);
            }

            healthSystem.DeactivateVoidBound();
        }
    }

    public void ResetEvents()
    {
        List<string> activeEffectNames = new List<string>();

        foreach (var pair in effectNameToId)
        {
            if (activeEffects.ContainsKey(pair.Value) && activeEffects[pair.Value])
            {
                activeEffectNames.Add(pair.Key);
            }
        }

        foreach (string effectName in activeEffectNames)
        {
            if (effectName == "Cesk's Kidney")
            {
                DeactivateHealthVoidBindingEffect();
            }
        }

        activeEffects.Clear();
        effectNameToId.Clear();

        InitializeEffectMapping();

        healthSystem.ResetHealthIcons();
    }

    public void AddEvent(string eventName)
    {
        switch (eventName)
        {
            case "Cesk's Kidney":
                ActivateHealthVoidBindingEffect();
                break;
        }

        if (effectNameToId.ContainsKey(eventName))
        {
            int id = effectNameToId[eventName];
            Debug.Log($"Effect '{eventName}' (ID: {id}) active: {activeEffects[id]}");
        }
    }

    public bool IsEffectActive(string effectName)
    {
        if (activeEffects == null)
        {
            Debug.LogError("activeEffects dictionary is null!");
            return false;
        }

        if (!effectNameToId.ContainsKey(effectName))
        {
            Debug.Log($"Effect '{effectName}' not found in effectNameToId mapping.");
            return false;
        }

        int id = effectNameToId[effectName];

        if (!activeEffects.ContainsKey(id))
        {
            Debug.Log($"Effect ID '{id}' not found in activeEffects.");
            return false;
        }

        return activeEffects[id];
    }

    public bool HasActiveEffects()
    {
        return activeEffects.Values.Any(v => v); // Checks if any effect is active (true)
    }

    private void ApplyPowerUpStats(PowerUpDefinition powerUp)
    {
        foreach (var statMod in powerUp.statModifiers)
        {
            string statName = statMod.statToModify.ToString();
            int value = Mathf.RoundToInt(statMod.value);

            if (statMod.useRawValue)
            {
                stats.AddStats(statName, null, value, 0);
            }
            else
            {
                stats.AddStats(statName, null, 0, value);
            }

            Debug.Log($"Applied {statName} modification: {(statMod.useRawValue ? "+" + value : "×" + value)}");
        }
    }

    private void RemovePowerUpStats(PowerUpDefinition powerUp)
    {
        foreach (var statMod in powerUp.statModifiers)
        {
            string statName = statMod.statToModify.ToString();
            float value = statMod.value;

            if (statMod.useRawValue)
            {
                stats.AddStats(statName, null, Mathf.RoundToInt(- value), 0);
            }
            else
            {
                float inverseMultiplier = 1f / (1f + value) - 1f;
                stats.AddStats(statName, null, 0, Mathf.RoundToInt(inverseMultiplier));
            }

            Debug.Log($"Removed {statName} modification");
        }
    }

    public void OnDungeonEnterEffects()
    {
        if (IsEffectActive("Cesk's Kidney"))
        {
            if (effectNameToId.ContainsKey("Cesk's Kidney"))
            {
                int powerUpId = effectNameToId["Cesk's Kidney"];
                PowerUpDefinition kidneyPowerUp = powerUpManager.GetPowerUpById(powerUpId, true);

                stats.AddStats("MaxHealth", null, 1, 0);
                healthSystem.SetToMaxHealth();
            }
        }
    }
}