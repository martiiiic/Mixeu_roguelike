using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class DungeonInfo : MonoBehaviour
{
    public TextMeshProUGUI DungeonNumber;
    public TextMeshProUGUI PlayerDamage;
    public TextMeshProUGUI PlayerSpeed;
    public TextMeshProUGUI PlayerDefense;
    public TextMeshProUGUI PlayerBowSkill;
    public TextMeshProUGUI PlayerHealth;

    public Image WeaponImage;
    public TextMeshProUGUI WeaponAttackDamage;
    public TextMeshProUGUI WeaponAttackSpeed;
    public TextMeshProUGUI WeaponRarity;

    public TextMeshProUGUI DungeonHealthMultiplier;
    public TextMeshProUGUI DungeonDamageMultiplier;
    public TextMeshProUGUI TotalDungeonDifficulty;
    public TextMeshProUGUI Difficulty;

    private GameManager manager;
    private PlayerStats stats;
    private WeaponTypes WT;

    private void Awake()
    {
        WT = FindObjectOfType<WeaponTypes>();
        stats = FindObjectOfType<PlayerStats>();
        manager = FindObjectOfType<GameManager>();
        UpdateRunInfo();
    }

    public void UpdateRunInfo()
    {
        stats = FindObjectOfType<PlayerStats>();
        manager = FindObjectOfType<GameManager>();
        WT = FindObjectOfType<WeaponTypes>();

        if(stats != null)
        {
            DungeonNumber.text = "Dungeon:" + manager.DungeonNumber;
            PlayerDamage.text = "" + (stats.AttackDamage - 4);
            PlayerBowSkill.text = "" + Mathf.RoundToInt(stats.RangedSpeed + 0.75f);
            PlayerDefense.text = "" + (stats.Defense - 1);
            PlayerSpeed.text = "" + (stats.Speed - 4);
            if (stats.Defense > 1) { PlayerHealth.text = "" + stats.MaxHealth + " + " + Mathf.RoundToInt(stats.Defense - 1); }
            else if (stats.Defense <= 1) { PlayerHealth.text = "" + stats.MaxHealth; }
        }

        if(manager != null) 
        {
            DungeonHealthMultiplier.text = "x" + manager.enemyHealthMultiplier;
            DungeonDamageMultiplier.text = "x" + (Mathf.RoundToInt(manager.enemyDamageMultiplier + manager.rangedEnemyDamageMultiplier / 2));
            TotalDungeonDifficulty.text = Difficulty.text;
        }

        if (WT == null) { return; }
        WeaponImage.sprite = WT.WeaponSprites[WT.CurrentWeaponId];
        WeaponAttackDamage.text = "x" + (Mathf.Round((WT.DamageMultiplier) * 100f) / 100f).ToString("F2");
        WeaponAttackSpeed.text = "x" + (Mathf.Round((1 / WT.AttackSpeedMultiplier) * 100f) / 100f).ToString("F2");
        WeaponRarity.text = WT.Wrarity;
        switch (WT.Wrarity)
        {
            case "Uncommon": WeaponRarity.color = new Color(0f, 1f, 0f, 1f); break;
            case "Rare": WeaponRarity.color = new Color(0f, 0.5f, 1f, 1f); break;
            case "Epic": WeaponRarity.color = new Color(1f, 0f, 1f, 1f); break;
            case "Legendary": WeaponRarity.color = new Color(1f, 1f, 0f, 1f); break;
            default: WeaponRarity.color = new Color(1f, 1f, 1f, 1f); break;
        }
    }
}
