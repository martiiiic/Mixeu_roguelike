using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteEnemyManager : MonoBehaviour
{
    [Header("Elite Properties")]
    public Material eliteMaterial;
    public float healthMultiplier = 2.5f;
    public float damageMultiplier = 1.5f;
    public float speedMultiplier = 1.2f;

    private List<Enemy> activeEnemies = new List<Enemy>();
    private HashSet<Enemy> markedAsElite = new HashSet<Enemy>();

    private RoomTemplates roomTemplates;
    private bool hasSetupElitesForCurrentDungeon = false;

    private void Awake()
    {
        RefreshEnemyList();
    }

    private void Start()
    {
        roomTemplates = FindObjectOfType<RoomTemplates>();
    }

    private void Update()
    {
        if (roomTemplates != null && roomTemplates.enemyRoomsComplete && !hasSetupElitesForCurrentDungeon)
        {
            StartCoroutine(SetupElitesAfterDelay());
            hasSetupElitesForCurrentDungeon = true;
        }
    }

    private IEnumerator SetupElitesAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        RefreshEnemyList();

        FindObjectOfType<Difficulty>()?.SelectEliteEnemies(GetBaseEliteCount());
    }

    private int GetBaseEliteCount()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        int dungeonNumber = gameManager != null ? gameManager.DungeonNumber : 1;

        return Mathf.FloorToInt(dungeonNumber * 0.5f);
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void RefreshEnemyList()
    {
        activeEnemies.Clear();
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        Debug.Log($"RefreshEnemyList found {allEnemies.Length} total enemies");

        activeEnemies.AddRange(allEnemies);
    }

    public void TransformToElite(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("Attempted to transform null enemy to elite");
            return;
        }

        if (enemy.Dead)
        {
            Debug.LogWarning("Attempted to transform dead enemy to elite");
            return;
        }

        if (markedAsElite.Contains(enemy))
        {
            Debug.LogWarning($"Enemy {enemy.name} already marked as elite");
            return;
        }

        try
        {
            markedAsElite.Add(enemy);
            Debug.Log($"Transforming {enemy.name} to elite. Total elites: {markedAsElite.Count}");

            SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"No sprite renderers found on enemy {enemy.name}");
            }

            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer != null && eliteMaterial != null)
                {
                    renderer.material = eliteMaterial;
                }
                else
                {
                    Debug.LogWarning($"Null renderer or elite material when transforming {enemy.name}");
                }
            }

            if (enemy.TryGetComponent(out BuffsAndDebuffs buffs))
            {
                EffectData eliteEffect = ScriptableObject.CreateInstance<EffectData>();
                eliteEffect.effectName = "Elite";
                eliteEffect.type = BuffsAndDebuffs.EffectType.Buff;
                eliteEffect.duration = 9999999f;
                eliteEffect.healthModifier = healthMultiplier;
                eliteEffect.attackModifier = damageMultiplier;
                eliteEffect.speedModifier = speedMultiplier;
                buffs.ApplyEffect(eliteEffect);

                Debug.Log($"Applied elite buff to {enemy.name} via BuffsAndDebuffs");
            }
            else
            {
                int originalHealth = enemy.MaxHealth;
                enemy.MaxHealth = Mathf.RoundToInt(enemy.MaxHealth * healthMultiplier);
                enemy.Health = enemy.MaxHealth;
                enemy.Speed *= speedMultiplier;

                Debug.Log($"Applied direct elite stats to {enemy.name}. Health: {originalHealth} → {enemy.MaxHealth}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error transforming enemy {enemy.name} to elite: {ex.Message}");
        }
    }

    public List<Enemy> GetEligibleEnemies()
    {
        List<Enemy> eligible = new List<Enemy>();

        RefreshEnemyList();

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null) continue;

            if (markedAsElite.Contains(enemy))
            {
                continue;
            }

            if (enemy.Health <= 0 || enemy.Dead)
            {
                continue;
            }

            eligible.Add(enemy);
        }

        Debug.Log($"GetEligibleEnemies found {eligible.Count} eligible out of {activeEnemies.Count} total");
        return eligible;
    }

    public bool IsElite(Enemy enemy)
    {
        return markedAsElite.Contains(enemy);
    }

    public void ResetEliteStatus()
    {
        int eliteCount = markedAsElite.Count;
        markedAsElite.Clear();
        hasSetupElitesForCurrentDungeon = false;
        Debug.Log($"Reset elite status. Cleared {eliteCount} elite markers");
    }
}