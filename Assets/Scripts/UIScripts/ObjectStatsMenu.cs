using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ObjectStatsMenu : MonoBehaviour
{
    public GameObject ObjectPrefab;
    public GameObject ObjectsBox;
    private PowerUpStore PowerUp;

    public Vector3 VectorOffset;

    private int i = 0;
    private int j = 0;

    private List<int> usedIDs = new List<int>();
    private Dictionary<int, GameObject> idToObjectMap = new Dictionary<int, GameObject>();
    private Dictionary<int, int> idToCountMap = new Dictionary<int, int>();

    public void AddPowerUp(int id, PowerUpStore ps = null)
    {
        if (usedIDs.Contains(id))
        {
            if (idToObjectMap.TryGetValue(id, out GameObject existingObject))
            {
                TextMeshProUGUI textComponent = existingObject.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    idToCountMap[id]++;
                    textComponent.text = $"x{idToCountMap[id]}";
                }
            }
            return;
        }

        usedIDs.Add(id);
        idToCountMap[id] = 1;


        if (i >= 11) { i = 0; j++; }
        PowerUpStoreUI PSUI = FindObjectOfType<PowerUpStoreUI>();
        PowerUp = PSUI.Store;

        GameObject newObject = Instantiate(ObjectPrefab, ObjectPrefab.GetComponent<RectTransform>().localPosition + (i * new Vector3(65f, 0, 0)) + (j * new Vector3(0, -30, 0)), Quaternion.identity, ObjectsBox.transform); i++;
        newObject.GetComponent<RectTransform>().pivot = new Vector2(1f, 2.05f);
        newObject.transform.localPosition += VectorOffset;

        // Use the PowerUpDefinition to get the sprite
        if (PowerUp.powerUpDefinition != null)
        {
            newObject.GetComponent<Image>().sprite = PowerUp.powerUpDefinition.icon;
        }

        idToObjectMap[id] = newObject;

        TextMeshProUGUI textComponentNew = newObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponentNew != null)
        {
            textComponentNew.text = $"x{idToCountMap[id]}";
        }
    }

    public void SubtractPowerUp(int id)
    {
        if (!usedIDs.Contains(id) || !idToCountMap.ContainsKey(id) || idToCountMap[id] <= 0)
        {
            Debug.LogWarning($"Item with ID {id} not found or count is already 0.");
            return;
        }

        if (idToCountMap[id] > 1)
        {
            idToCountMap[id]--;
            if (idToObjectMap.TryGetValue(id, out GameObject existingObject))
            {
                TextMeshProUGUI textComponent = existingObject.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = $"x{idToCountMap[id]}";
                }
            }
        }
        else
        {
            if (idToObjectMap.TryGetValue(id, out GameObject objectToDelete))
            {
                Destroy(objectToDelete);
                idToObjectMap.Remove(id);
                usedIDs.Remove(id);
                idToCountMap.Remove(id);
                UpdatePositions();
            }
        }
    }

    private void UpdatePositions()
    {
        int tempI = 0;
        int tempJ = 0;

        foreach (var kvp in idToObjectMap)
        {
            GameObject obj = kvp.Value;
            obj.transform.position = ObjectPrefab.transform.position + (tempI * new Vector3(65f, 0, 0)) + (tempJ * new Vector3(0, -30, 0));

            tempI++;
            if (tempI >= 8)
            {
                tempI = 0;
                tempJ++;
            }
        }

        i = tempI;
        j = tempJ;
    }

    public void ResetAllPowerUps()
    {
        foreach (var obj in idToObjectMap.Values)
        {
            Destroy(obj);
        }

        usedIDs.Clear();
        idToObjectMap.Clear();
        idToCountMap.Clear();

        i = 0;
        j = 0;
    }
}