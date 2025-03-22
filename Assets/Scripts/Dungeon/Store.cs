using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : MonoBehaviour
{
    public bool NormalStore = true;
    public bool VoidBoundStore = false;

    // Optional: manual assignment of power-ups through inspector
    [Tooltip("Optional: Manually assign normal power-ups here")]
    public List<PowerUpDefinition> normalPowerUps = new List<PowerUpDefinition>();

    [Tooltip("Optional: Manually assign void bound power-ups here")]
    public List<PowerUpDefinition> voidBoundPowerUps = new List<PowerUpDefinition>();

    // Whether to use auto-loaded power-ups from PowerUpManager
    [Tooltip("If true, will use power-ups from PowerUpManager instead of the lists above")]
    public bool useAutoLoadedPowerUps = true;

    // Whether to avoid duplicates when assigning power-ups
    [Tooltip("If true, will avoid assigning the same power-up to multiple slots")]
    public bool avoidDuplicates = true;

    private void Awake()
    {
        StartCoroutine(WaitForSetUp());
    }

    private IEnumerator WaitForSetUp()
    {
        // Wait a short time to ensure PowerUpManager is initialized
        yield return new WaitForSeconds(0.1f);

        // Get all PowerUpStore components in children
        PowerUpStore[] powerUpStores = GetComponentsInChildren<PowerUpStore>();

        if (powerUpStores.Length == 0)
        {
            Debug.LogWarning("No PowerUpStore components found in children of Store.");
            yield break;
        }

        // If auto-loading, get power-ups from PowerUpManager
        if (useAutoLoadedPowerUps)
        {
            LoadPowerUpsFromManager();
        }

        // Display debug information
        Debug.Log($"Store setup: Normal={NormalStore}, VoidBound={VoidBoundStore}");
        Debug.Log($"Normal PowerUps: {normalPowerUps.Count}, Void PowerUps: {voidBoundPowerUps.Count}");

        // Track which power-ups have been assigned to avoid duplicates
        List<PowerUpDefinition> assignedPowerUps = new List<PowerUpDefinition>();

        // Assign power-ups based on store type
        if (NormalStore)
        {
            AssignPowerUpsToStores(powerUpStores, normalPowerUps, assignedPowerUps);
        }
        else if (VoidBoundStore)
        {
            AssignPowerUpsToStores(powerUpStores, voidBoundPowerUps, assignedPowerUps);
        }
    }

    private void LoadPowerUpsFromManager()
    {
        PowerUpManager powerUpManager = PowerUpManager.Instance;

        if (powerUpManager == null)
        {
            Debug.LogError("PowerUpManager instance not found! Make sure it exists in the scene.");
            return;
        }

        // Get power-ups from manager
        if (NormalStore)
        {
            normalPowerUps = new List<PowerUpDefinition>(powerUpManager.GetNormalPowerUps());
        }

        if (VoidBoundStore)
        {
            voidBoundPowerUps = new List<PowerUpDefinition>(powerUpManager.GetVoidBoundPowerUps());
        }
    }

    private void AssignPowerUpsToStores(PowerUpStore[] stores, List<PowerUpDefinition> availablePowerUps, List<PowerUpDefinition> assignedPowerUps)
    {
        if (availablePowerUps == null || availablePowerUps.Count == 0)
        {
            Debug.LogWarning($"No {(NormalStore ? "normal" : "void bound")} power-ups available to assign!");
            return;
        }

        foreach (PowerUpStore store in stores)
        {
            // Skip stores that already have power-ups assigned
            if (store.powerUpDefinition != null)
            {
                Debug.Log($"Store already has {store.powerUpDefinition.displayName} assigned, skipping.");
                continue;
            }

            // Create a temporary list of available power-ups
            List<PowerUpDefinition> remainingPowerUps = new List<PowerUpDefinition>(availablePowerUps);

            // If avoiding duplicates, remove already assigned power-ups
            if (avoidDuplicates && assignedPowerUps.Count > 0)
            {
                foreach (PowerUpDefinition assigned in assignedPowerUps)
                {
                    remainingPowerUps.Remove(assigned);
                }

                // If all power-ups have been assigned and we need more, reset the assigned list
                if (remainingPowerUps.Count == 0)
                {
                    Debug.Log("All unique power-ups assigned. Starting to repeat power-ups.");
                    remainingPowerUps = new List<PowerUpDefinition>(availablePowerUps);
                }
            }

            // Select and assign a random power-up
            if (remainingPowerUps.Count > 0)
            {
                int randomIndex = Random.Range(0, remainingPowerUps.Count);
                PowerUpDefinition selectedPowerUp = remainingPowerUps[randomIndex];

                AssignPowerUpDefinition(store, selectedPowerUp);

                // Track assigned power-up
                if (avoidDuplicates)
                {
                    assignedPowerUps.Add(selectedPowerUp);
                }

                Debug.Log($"Assigned {selectedPowerUp.displayName} to store.");
            }
            else
            {
                Debug.LogWarning("No power-ups available to assign!");
            }
        }
    }

    private void AssignPowerUpDefinition(PowerUpStore powerUpStore, PowerUpDefinition definition)
    {
        if (powerUpStore != null && definition != null)
        {
            // Set the power-up definition
            powerUpStore.powerUpDefinition = definition;

            // Update the store's isVoidBoundStore flag to match the power-up type
            powerUpStore.isVoidBoundStore = definition.isVoidBound;

            // Initialize the store with the new definition
            powerUpStore.InitializeFromDefinition();

            Debug.Log($"PowerUp {definition.displayName} assigned to store. Icon null? {definition.icon == null}");
        }
        else
        {
            Debug.LogError("Failed to assign power-up: " +
                          (powerUpStore == null ? "PowerUpStore is null. " : "") +
                          (definition == null ? "PowerUpDefinition is null." : ""));
        }
    }
}