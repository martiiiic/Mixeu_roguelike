using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int DungeonNumber;
    private float PriceMultiplierf;
    public int PriceMultiplier;
    public int enemyHealthMultiplier;
    public int enemyDamageMultiplier;
    public int rangedEnemyDamageMultiplier;
    private OnDungeonEnterText DungeonText;
    private RoomTemplates roomTemplates;
    private int healthIncrementCounter = 0;
    private int damageIncrementCounter = 0;

    private void Start()
    {
        roomTemplates = FindObjectOfType<RoomTemplates>();
        DungeonNumber = 0;
        PriceMultiplierf = PriceMultiplier = 1;
        enemyHealthMultiplier = enemyDamageMultiplier = rangedEnemyDamageMultiplier = 1;
        DungeonText = FindObjectOfType<OnDungeonEnterText>();
    }

    public void NewDungeon()
    {
        Time.timeScale = 1;
        DungeonNumber++;
        DungeonText.ShowDungeonText();

        Music music = FindObjectOfType<Music>();
        music.PlayRandomMusic(MusicContext.Exploration);

        float difficultyModifier = Difficulty.Diff;

        float healthScalingChance = 0.15f + (0.05f * difficultyModifier);

        if (DungeonNumber >= 2)
        {
            if (Random.Range(0f, 1f) < healthScalingChance)
            {
                enemyHealthMultiplier++;
                healthIncrementCounter = 0;
            }
            else
            {
                healthIncrementCounter++;
                if (healthIncrementCounter >= 4) // Increased threshold to slow scaling
                {
                    enemyHealthMultiplier++;
                    healthIncrementCounter = 0;
                }
            }

            float damageScalingChance = 0.5f + (0.02f * difficultyModifier);
            bool damageIncreased = false;

            if (DungeonNumber >= 16)
            {
                if (damageIncrementCounter >= 2 || Random.Range(0f, 1f) < damageScalingChance)
                {
                    enemyDamageMultiplier++;
                    damageIncreased = true;
                    damageIncrementCounter = 0;
                }
                else
                {
                    damageIncrementCounter++;
                }
            }
        }

        if (DungeonNumber > 5 && DungeonNumber % 3 == 0)
        {
            enemyHealthMultiplier++;
        }

        rangedEnemyDamageMultiplier = Mathf.RoundToInt(enemyDamageMultiplier * 0.75f);

        float priceScalingFactor = 1f + (0.03f * difficultyModifier);
        float basePriceGrowth = DungeonNumber * 0.75f * priceScalingFactor;
        float multiplierBonus = Mathf.Abs(Mathf.Pow(Mathf.Max(enemyHealthMultiplier, enemyDamageMultiplier),
                               Mathf.Log10(10 + DungeonNumber) + (0.2f * Mathf.Sqrt(DungeonNumber))));

        PriceMultiplierf = Mathf.Abs(Mathf.Log10(10 + (PriceMultiplierf * basePriceGrowth + multiplierBonus)) +
                           enemyHealthMultiplier * 1.5f + (DungeonNumber * 0.8f) -
                           (Mathf.Log(DungeonNumber + 1) / (DungeonNumber + 1)));

        if (PriceMultiplierf <= 1) { PriceMultiplierf = 1f; }
        PriceMultiplier = Mathf.RoundToInt(PriceMultiplierf);
        if (PriceMultiplier < 1) { PriceMultiplier = 1; }
    }
}