using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;

public class WeaponTriggerDamageHandler : MonoBehaviour, IDisposable
{
    public BoxCollider2D DamageBox;
    private SpriteRenderer sprite;
    private PlayerState Player;
    private PlayerStats Stats;
    private AudioSource Hit;
    private WeaponTypes WT;
    private SpecialPlayerStats SPS;
    private VisualEffects Effects;
    public GameObject DamageIndicator;
    public bool Dead;
    private Dictionary<string, GameObject> activeEffectInstances = new Dictionary<string, GameObject>();

    private void Start()
    {
        SPS = FindObjectOfType<SpecialPlayerStats>();
        WT = FindObjectOfType<WeaponTypes>();
        Hit = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        Player = FindObjectOfType<PlayerState>();
        Stats = FindObjectOfType<PlayerStats>();
        DamageBox = GetComponent<BoxCollider2D>();
        Effects = FindObjectOfType<VisualEffects>();
    }

    private void Update()
    {
        sprite.enabled = !Player.Dead;
        Dead = Player.Dead;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Dead || other == null) return;

        if (other.CompareTag("Enemy")) HandleEnemyCollision(other);
        else if (other.CompareTag("Boss")) HandleBossCollision(other);
    }

    private void HandleEnemyCollision(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>() ?? other.GetComponentInParent<Enemy>() ?? other.GetComponentInChildren<Enemy>();
        if (enemy == null || enemy.invulnerable) return;

        Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
        Vector2 knockbackForce = knockbackDirection * 5f;
        int damage = Mathf.RoundToInt(Stats.AttackDamage * WT.DamageMultiplier);
        enemy.TakeDamage(damage, knockbackForce);

        if (SPS.HasAnyEnabledEffects()) ApplyEffects(enemy);

        Hit.Play();
        DamageBox.enabled = false;
        SpawnDamageIndicator(enemy.transform.position, damage, enemy.invulnerable, Color.white);
    }

    private void HandleBossCollision(Collider2D other)
    {
        BossScript boss = other.GetComponentInParent<BossScript>() ??
                          other.GetComponent<BossScript>();

        if (boss == null || boss.Invulnerable) return;

        int damage = Stats.AttackDamage;
        boss.TakeDamage(damage);

        Hit.Play();
        DamageBox.enabled = false;
        SpawnDamageIndicator(boss.transform.position, damage, boss.Invulnerable, Color.white);

    }

    public void SpawnDamageIndicator(Vector3 position, int damage, bool isInvulnerable, Color textColor)
    {
        GameObject DNumber = Instantiate(DamageIndicator, position + new Vector3(0, 2, 0), Quaternion.identity);
        TextMeshPro text = DNumber.GetComponent<TextMeshPro>();

        if (text != null)
        {
            text.text = isInvulnerable ? "0" : damage.ToString();
            text.color = textColor;
        }
    }

    public void ApplyEffects(Enemy enemy)
    {
        BuffsAndDebuffs buffsAndDebuffs = enemy.gameObject.GetComponent<BuffsAndDebuffs>() ?? enemy.gameObject.AddComponent<BuffsAndDebuffs>();


        if (!SPS.GetStat("CanPoisonAndBurn"))
        {
            if (SPS.GetStat("CanBurn") && UnityEngine.Random.Range(0, 101) <= Mathf.Min(100, SPS.BurnPercentage)) ApplyBurnEffect(buffsAndDebuffs, enemy);
            else if (SPS.GetStat("CanPoison") && UnityEngine.Random.Range(0, 101) <= Mathf.Min(60, SPS.PoisonPercentage)) ApplyPoisonEffect(buffsAndDebuffs, enemy);
        }
        else
        {
            if (SPS.GetStat("CanBurn") && UnityEngine.Random.Range(0, 101) <= Mathf.Min(100, SPS.BurnPercentage)) ApplyBurnEffect(buffsAndDebuffs, enemy);
            if (SPS.GetStat("CanPoison") && UnityEngine.Random.Range(0, 101) <= Mathf.Min(60, SPS.PoisonPercentage)) ApplyPoisonEffect(buffsAndDebuffs, enemy);
        }

        if (SPS.GetStat("SpeedBurst") && enemy.Dead) ApplyBurstSpeedEffect(buffsAndDebuffs);
    }

    private Dictionary<string, Coroutine> effectDestructionCoroutines = new Dictionary<string, Coroutine>();

    private void ApplyBurnEffect(BuffsAndDebuffs buffsAndDebuffs, Enemy enemy)
    {
        Debug.Log("Applying Burn Effect");

        EffectData burnEffect = new EffectData
        {
            effectName = "Burning",
            type = BuffsAndDebuffs.EffectType.Debuff,
            duration = 2 * Mathf.Min(SPS.BurnLongevityFactor, 10),
            tickDamage = 2 * Mathf.Max(0.1f, Mathf.Log(1.71f + SPS.BurnDamageFactor)),
            tickInterval = Mathf.Max(0.01f, 1f / Mathf.Log(1.71f + (SPS.BurnSpeedFactor * 0.75f)))
        };

        if (buffsAndDebuffs.IsEffectActive("Burning"))
        {
            Debug.Log("Burning effect already exists! Extending duration.");
            ExtendEffectDuration("Burning", burnEffect.duration);
        }
        else
        {
            Debug.Log("Creating new Burning effect.");
            buffsAndDebuffs.ApplyEffect(burnEffect);
            HandleEffectInstance("Burning", Effects.BurningEffectPrefab, enemy.transform, burnEffect.duration);
        }
    }

    private void ApplyPoisonEffect(BuffsAndDebuffs buffsAndDebuffs, Enemy enemy)
    {
        EffectData poisonEffect = new EffectData
        {
            effectName = "Poison",
            type = BuffsAndDebuffs.EffectType.Debuff,
            duration = 2 * Mathf.Min(SPS.PoisonLongevityFactor, 5),
            tickDamage = 1 * Mathf.Max(0.1f, Mathf.Log(1.71f + SPS.PoisonDamageFactor * 0.5f)),
            tickInterval = Mathf.Max(0.01f, 0.5f / Mathf.Log(1.71f + (SPS.PoisonSpeedFactor * 0.75f)))
        };

        if (buffsAndDebuffs.IsEffectActive("Poison"))
        {
            Debug.Log("Poison effect already exists! Extending duration.");
            ExtendEffectDuration("Poison", poisonEffect.duration);
        }
        else
        {
            Debug.Log("Creating new Poison effect.");
            buffsAndDebuffs.ApplyEffect(poisonEffect);
            HandleEffectInstance("Poison", Effects.PoisonEffectPrefab, enemy.transform, poisonEffect.duration);
        }
    }

    private void HandleEffectInstance(string effectName, GameObject effectPrefab, Transform parent, float duration)
    {
        if (activeEffectInstances.TryGetValue(effectName, out GameObject existingEffect))
        {
            Destroy(existingEffect); // Remove old visual effect
        }

        GameObject effectInstance = Instantiate(effectPrefab, parent);
        activeEffectInstances[effectName] = effectInstance;
        StartEffectDestructionTimer(effectName, duration);
    }

    private void ExtendEffectDuration(string effectName, float additionalDuration)
    {
        if (effectDestructionCoroutines.TryGetValue(effectName, out Coroutine existingCoroutine))
        {
            StopCoroutine(existingCoroutine);
        }

        float newTotalDuration = additionalDuration;
        effectDestructionCoroutines[effectName] = StartCoroutine(DestroyEffectAfterDuration(effectName, newTotalDuration));
    }

    private IEnumerator DestroyEffectAfterDuration(string effectName, float duration)
    {
        Debug.Log($"Waiting {duration} seconds before destroying {effectName}");
        yield return new WaitForSeconds(duration);

        if (activeEffectInstances.TryGetValue(effectName, out GameObject effectInstance))
        {
            Debug.Log($"Destroying effect {effectName}");
            Destroy(effectInstance);
            activeEffectInstances.Remove(effectName);
        }
        effectDestructionCoroutines.Remove(effectName);
    }

    private void StartEffectDestructionTimer(string effectName, float duration)
    {
        if (effectDestructionCoroutines.ContainsKey(effectName))
        {
            StopCoroutine(effectDestructionCoroutines[effectName]);
        }

        effectDestructionCoroutines[effectName] = StartCoroutine(DestroyEffectAfterDuration(effectName, duration));
    }



    private void ApplyBurstSpeedEffect(BuffsAndDebuffs buffsAndDebuffs)
    {
        EffectData speedBurst = new EffectData
        {
            effectName = "SpeedBurst",
            type = BuffsAndDebuffs.EffectType.Buff,
            duration = Mathf.Min(SPS.BurstSpeedLongevityFactor, 10),
            speedModifier = 2 * Mathf.Max(0, Mathf.Log(1.71f + SPS.BurstSpeedFactor))
        };

        buffsAndDebuffs.ApplyEffect(speedBurst);
    }

    private bool _disposed = false;

    public void Dispose()
    {
        if (!_disposed)
        {
            // Stop all coroutines
            StopAllCoroutines();

            // Clean up dictionaries
            foreach (var effectInstance in activeEffectInstances.Values)
            {
                if (effectInstance != null)
                {
                    Destroy(effectInstance);
                }
            }
            activeEffectInstances.Clear();

            effectDestructionCoroutines.Clear();

            // Clear references
            sprite = null;
            Player = null;
            Stats = null;
            Hit = null;
            WT = null;
            SPS = null;
            Effects = null;
            DamageBox = null;
            DamageIndicator = null;

            _disposed = true;
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }


}