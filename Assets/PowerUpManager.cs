using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    // Lists to store different types of power-ups
    [SerializeField] private List<PowerUpDefinition> normalPowerUps = new List<PowerUpDefinition>();
    [SerializeField] private List<PowerUpDefinition> voidBoundPowerUps = new List<PowerUpDefinition>();

    // Singleton instance
    private static PowerUpManager _instance;
    public static PowerUpManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PowerUpManager>();

                if (_instance == null)
                {
                    GameObject managerObject = new GameObject("PowerUpManager");
                    _instance = managerObject.AddComponent<PowerUpManager>();
                    DontDestroyOnLoad(managerObject);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Singleton pattern implementation
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Load power-ups if lists are empty
        if (normalPowerUps.Count == 0 || voidBoundPowerUps.Count == 0)
        {
            LoadPowerUps();
        }
    }
    private void LoadPowerUps()
    {
        PowerUpDefinition[] mainFolderPowerUps = Resources.LoadAll<PowerUpDefinition>("PowerUps");
        PowerUpDefinition[] normalFolderPowerUps = Resources.LoadAll<PowerUpDefinition>("PowerUps/NormalPowerUps");
        PowerUpDefinition[] voidFolderPowerUps = Resources.LoadAll<PowerUpDefinition>("PowerUps/VoidPowerUps");
        List<PowerUpDefinition> allPowerUps = new List<PowerUpDefinition>();
        allPowerUps.AddRange(mainFolderPowerUps);
        allPowerUps.AddRange(normalFolderPowerUps);
        allPowerUps.AddRange(voidFolderPowerUps);

        foreach (PowerUpDefinition powerUp in allPowerUps)
        {
            if (powerUp.isVoidBound)
            {
                voidBoundPowerUps.Add(powerUp);
            }
            else
            {
                normalPowerUps.Add(powerUp);
            }
        }

        if (normalPowerUps.Count == 0 && voidBoundPowerUps.Count == 0)
        {
            Debug.LogWarning("No PowerUpDefinitions found in Resources/PowerUps folders!");
        }
        else
        {
            Debug.Log($"Loaded {normalPowerUps.Count} normal power-ups and {voidBoundPowerUps.Count} void bound power-ups");
        }
    }

    // Get all normal power-ups
    public List<PowerUpDefinition> GetNormalPowerUps()
    {
        if (normalPowerUps.Count == 0)
        {
            LoadPowerUps();
        }
        return normalPowerUps;
    }

    // Get all void bound power-ups
    public List<PowerUpDefinition> GetVoidBoundPowerUps()
    {
        if (voidBoundPowerUps.Count == 0)
        {
            LoadPowerUps();
        }
        return voidBoundPowerUps;
    }

    // Get a specific power-up by ID
    public PowerUpDefinition GetPowerUpById(int id, bool isVoidBound = false)
    {
        List<PowerUpDefinition> list = isVoidBound ? voidBoundPowerUps : normalPowerUps;

        foreach (PowerUpDefinition powerUp in list)
        {
            if (powerUp.id == id)
            {
                return powerUp;
            }
        }

        return null;
    }

    // Add a power-up definition at runtime
    public void AddPowerUpDefinition(PowerUpDefinition powerUp)
    {
        if (powerUp == null) return;

        if (powerUp.isVoidBound)
        {
            if (!voidBoundPowerUps.Contains(powerUp))
            {
                voidBoundPowerUps.Add(powerUp);
            }
        }
        else
        {
            if (!normalPowerUps.Contains(powerUp))
            {
                normalPowerUps.Add(powerUp);
            }
        }
    }
}