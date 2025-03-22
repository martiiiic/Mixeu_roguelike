using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Difficulty : MonoBehaviour
{
    public static int Diff = 0;
    public int Dificulty;
    private string[] DifficultyName = new string[] { "Easy", "Normal", "Hard", "Insane", "Mixeu's Torment" };
    public string DiffName;

    [Header("Elite Enemy Settings")]
    [Tooltip("Base chance multiplier for elite enemies to spawn")]
    public float eliteSpawnMultiplier = 0.75f;
    public Material eliteMaterial;

    private GameManager Manager;
    private EliteEnemyManager eliteManager;
    private RoomTemplates roomTemplates;

    private void Awake()
    {
        eliteManager = GetComponent<EliteEnemyManager>();
        if (eliteManager == null)
        {
            eliteManager = gameObject.AddComponent<EliteEnemyManager>();
            eliteManager.eliteMaterial = eliteMaterial;
        }
    }

    public float GetCurrentDifficulty()
    {
        return 1.0f + (0.5f * Dificulty);
    }

    private void Start()
    {
        Manager = FindObjectOfType<GameManager>();
        roomTemplates = FindObjectOfType<RoomTemplates>();

        try
        {
            Diff = MainMenuButtons.difficulty;
            if (MainMenuButtons.Dificulties != null && Diff >= 0 && Diff < MainMenuButtons.Dificulties.Length)
            {
                DiffName = DifficultyName[Diff];
            }
            Dificulty = Diff;
        }
        catch (System.Exception)
        {
            Diff = 1;
            DiffName = DifficultyName[Diff];
            Dificulty = 1;
            Debug.LogWarning("Failed to get difficulty settings. Using defaults.");
        }
    }

    private void Update()
    {
        Dificulty = Diff;
        DiffName = DifficultyName[Diff];
    }

    public void SelectEliteEnemies(int baseNumberOfElites)
    {
        if (Manager == null) Manager = FindObjectOfType<GameManager>();
        if (roomTemplates == null) roomTemplates = FindObjectOfType<RoomTemplates>();
        if (eliteManager == null)
        {
            eliteManager = GetComponent<EliteEnemyManager>();
            if (eliteManager == null)
            {
                eliteManager = gameObject.AddComponent<EliteEnemyManager>();
                eliteManager.eliteMaterial = eliteMaterial;
            }
        }
        int dungeonNumber = Manager.DungeonNumber;
        int eliteRoomCount = 0;
        if (roomTemplates != null && roomTemplates.rooms != null)
        {
            eliteRoomCount = roomTemplates.rooms.Count;
        }

        if (dungeonNumber <= 6)
        {
            baseNumberOfElites = 0;
            return;
        }

        if (dungeonNumber >= 13)
        {
            int minElitesPerRoom = 1 + Dificulty;
            baseNumberOfElites = Mathf.Max(baseNumberOfElites, minElitesPerRoom * eliteRoomCount);
        }

        if (dungeonNumber >= 7 && dungeonNumber < 10)
        {
            if (Dificulty >= 3)
            {
                baseNumberOfElites = baseNumberOfElites + Mathf.RoundToInt(dungeonNumber * (0.1f * Dificulty));
            }
            else
            {
                baseNumberOfElites = baseNumberOfElites + Mathf.RoundToInt(dungeonNumber * 0.1f);
            }
        }

        if (dungeonNumber >= 10)
        {
            if (Dificulty >= 3)
            {
                baseNumberOfElites = baseNumberOfElites + Mathf.RoundToInt(dungeonNumber * (0.2f * Dificulty));
            }
            else
            {
                baseNumberOfElites = baseNumberOfElites + Mathf.RoundToInt(dungeonNumber * 0.2f);
            }
        }

        if (dungeonNumber >= 15)
        {
            baseNumberOfElites = baseNumberOfElites + Mathf.RoundToInt(dungeonNumber * 0.4f);
        }

        eliteManager.RefreshEnemyList();

        float difficultyMultiplier = 0.15f + (Dificulty * 0.15f);
        int numberOfElites = Mathf.RoundToInt(baseNumberOfElites * difficultyMultiplier * eliteSpawnMultiplier);

        if (roomTemplates != null && roomTemplates.rooms != null && roomTemplates.rooms.Count > 10)
        {
            numberOfElites += Mathf.FloorToInt(roomTemplates.rooms.Count * 0.04f);
        }

        numberOfElites += Mathf.FloorToInt((dungeonNumber / 5) * 0.2f);

        if (dungeonNumber >= 11 && Dificulty >= 3)
        {
            numberOfElites = Mathf.Max(numberOfElites, 4);
        }

        if (dungeonNumber >= 13)
        {
            int minElitesPerRoom = 1 + Dificulty;
            numberOfElites = Mathf.Max(numberOfElites, minElitesPerRoom * eliteRoomCount);
            if (dungeonNumber >= 15)
            {
                numberOfElites += Mathf.FloorToInt((dungeonNumber - 13) * 0.3f * eliteRoomCount);
            }
        }

        int maxElites = Mathf.Min(12, 1 + Mathf.FloorToInt(dungeonNumber * 0.4f));
        if (dungeonNumber >= 13)
        {
            maxElites = Mathf.Max(maxElites, 3 * eliteRoomCount);
        }

        numberOfElites = Mathf.Min(numberOfElites, maxElites);
        List<Enemy> eligibleEnemies = eliteManager.GetEligibleEnemies();
        Debug.Log($"Eligible enemies for elites: {eligibleEnemies.Count}");
        if (eligibleEnemies.Count == 0)
        {
            Debug.LogWarning("No eligible enemies found for elite transformation!");
            return;
        }

        numberOfElites = Mathf.Min(numberOfElites, eligibleEnemies.Count);
        for (int i = 0; i < numberOfElites; i++)
        {
            if (eligibleEnemies.Count == 0)
                break;
            int randomIndex = Random.Range(0, eligibleEnemies.Count);
            Enemy selectedEnemy = eligibleEnemies[randomIndex];
            eliteManager.TransformToElite(selectedEnemy);
            eligibleEnemies.RemoveAt(randomIndex);
        }

        Debug.Log($"Dungeon {dungeonNumber}: Selected {numberOfElites} enemies to become elites at difficulty {DiffName} (approx. {numberOfElites / (float)eliteRoomCount:F1} per room)");
    }

    // Returns a multiplier for the number of enemies based on current difficulty
    public float GetEnemyCountMultiplier()
    {
        float baseMultiplier = 1.0f;

        baseMultiplier += 0.1f * Dificulty;

        if (Manager != null)
        {
            int dungeonNumber = Manager.DungeonNumber;

            if (dungeonNumber >= 8)
            {
                baseMultiplier += 0.05f * (dungeonNumber - 7);
            }

            // Extra enemies for very high dungeon levels
            if (dungeonNumber >= 15)
            {
                baseMultiplier += 0.1f * (dungeonNumber - 14);
            }
        }

        float maxMultiplier = 3.0f;
        if (Dificulty >= 4) 
        {
            maxMultiplier = 4.0f;
        }

        return Mathf.Min(baseMultiplier, maxMultiplier);
    }


    public void OnNewDungeon()
    {
        if (eliteManager != null)
        {
            eliteManager.ResetEliteStatus();
        }
    }
}