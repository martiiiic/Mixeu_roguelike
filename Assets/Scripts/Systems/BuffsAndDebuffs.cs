using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffsAndDebuffs : MonoBehaviour
{
    public GameObject DamageIndicator;

    public enum EffectType { Buff, Debuff }

    private Dictionary<string, Coroutine> activeEffects = new Dictionary<string, Coroutine>();
    private Dictionary<GameObject, HashSet<string>> enemyEffects = new Dictionary<GameObject, HashSet<string>>();

    private VisualEffects Effects;

    private void Awake()
    {
        Effects = FindObjectOfType<VisualEffects>();
    }

    public void ApplyEffect(EffectData effectData)
    {
        if (activeEffects.ContainsKey(effectData.effectName))
            StopCoroutine(activeEffects[effectData.effectName]);

        if (IsEffectAlreadyApplied(effectData.effectName))
            return;

        activeEffects[effectData.effectName] = StartCoroutine(HandleEffect(effectData));
    }

    private IEnumerator HandleEffect(EffectData effect)
    {
        if (TryGetComponent(out Enemy enemy))
        {
            if (!enemyEffects.ContainsKey(enemy.gameObject))
                enemyEffects[enemy.gameObject] = new HashSet<string>();
            enemyEffects[enemy.gameObject].Add(effect.effectName);
        }

        switch (effect.effectName)
        {
            case "Burning":
                yield return StartCoroutine(HandleBurningEffect(effect));
                break;
            case "Poison":
                yield return StartCoroutine(HandlePoisonEffect(effect));
                break;
            case "Healing":
                yield return StartCoroutine(HandleHealingEffect(effect));
                break;
            case "SpeedBurst":
                yield return StartCoroutine(HandleBurstSpeedEffect(effect));
                break;
        }

        activeEffects.Remove(effect.effectName);

        if (TryGetComponent(out Enemy expiredEnemy))
        {
            if (enemyEffects.ContainsKey(expiredEnemy.gameObject))
                enemyEffects[expiredEnemy.gameObject].Remove(effect.effectName);
        }
    }

    private bool IsEffectAlreadyApplied(string effectName)
    {
        foreach (var enemy in enemyEffects)
        {
            if (enemy.Value.Contains(effectName))
                return true;
        }
        return false;
    }

    private IEnumerator HandleBurningEffect(EffectData effect)
    {
        yield return HandleDamageOverTime(effect, Effects.BurningEffectPrefab, Color.yellow);
    }

    private IEnumerator HandlePoisonEffect(EffectData effect)
    {
        yield return HandleDamageOverTime(effect, Effects.PoisonEffectPrefab, Color.green, true);
    }

    private IEnumerator HandleHealingEffect(EffectData effect)
    {
        float elapsedTime = 0f;
        while (elapsedTime < effect.duration)
        {
            ApplyHealingOverTime(Mathf.RoundToInt(effect.tickDamage));
            elapsedTime += effect.tickInterval;
            yield return new WaitForSeconds(effect.tickInterval);
        }
    }

    private IEnumerator HandleDamageOverTime(EffectData effect, GameObject effectPrefab, Color color, bool isPoison = false)
    {
        float elapsedTime = 0f;
        bool firstTick = true;
        GameObject effectInstance = null;

        if (TryGetComponent(out Enemy enemy))
            effectInstance = Instantiate(effectPrefab, enemy.transform);

        while (elapsedTime < effect.duration)
        {
            if (!firstTick)
                ApplyDamageOverTime(Mathf.RoundToInt(effect.tickDamage), color, isPoison);
            firstTick = false;
            elapsedTime += effect.tickInterval;
            yield return new WaitForSeconds(effect.tickInterval);
        }

        if (effectInstance != null) Destroy(effectInstance);
    }

    private void ApplyDamageOverTime(int damage, Color effectColor, bool isPoison = false)
    {
        if (TryGetComponent(out Enemy enemy))
        {
            WeaponTriggerDamageHandler WTDH = FindObjectOfType<WeaponTriggerDamageHandler>();


            if (isPoison)
            {
                enemy.Health -= damage + Mathf.RoundToInt(enemy.Health * 0.025f);
                WTDH.SpawnDamageIndicator(enemy.transform.position, damage + Mathf.RoundToInt(enemy.Health * 0.025f), enemy.invulnerable, effectColor);
            }
            else
            {
                enemy.TakeDamage(damage, Vector2.zero);
                WTDH.SpawnDamageIndicator(enemy.transform.position, damage, enemy.invulnerable, effectColor);
            }
                
        }
    }

    public bool IsEffectActive(string effectName)
    {
        if (TryGetComponent(out Enemy enemy))
        {
            if (enemyEffects.ContainsKey(enemy.gameObject))
                return enemyEffects[enemy.gameObject].Contains(effectName);
        }
        return false;
    }

    private void ApplyHealingOverTime(int healing)
    {
        if (TryGetComponent(out Enemy enemy))
        {
            WeaponTriggerDamageHandler WTDH = FindObjectOfType<WeaponTriggerDamageHandler>();
            WTDH.SpawnDamageIndicator(enemy.transform.position, healing, enemy.invulnerable, Color.green);
            enemy.Health = Mathf.Min(enemy.MaxHealth, enemy.Health + healing);
        }
    }

    private IEnumerator HandleBurstSpeedEffect(EffectData effect)
    {
        if (TryGetComponent(out Enemy enemy))
        {
            enemy.invulnerable = true;
            float originalSpeed = enemy.Speed;
            enemy.Speed *= Mathf.Log(1.72f + effect.speedModifier);
            yield return new WaitForSeconds(effect.duration);
            enemy.invulnerable = false;
            enemy.Speed = originalSpeed;
        }
    }
}