using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthSystem : MonoBehaviour
{
    public GameObject FishPointPrefab;
    public Sprite[] FishSprites;
    public Sprite[] VoidBoundSprites;
    public Transform FishContainer;
    public float fishSpacing = 50f;

    private List<Image> fishPoints = new List<Image>();
    private PlayerStats Stats;
    private Vector2 basePosition;
    private bool isVoidBound = false;

    private void Start()
    {
        Stats = FindAnyObjectByType<PlayerStats>();
        ClearExistingFishPoints();
        CreateFishPoints();
    }

    private void ClearExistingFishPoints()
    {
        // Destroy all existing fish points
        foreach (Image fishPoint in fishPoints)
        {
            if (fishPoint != null)
            {
                Destroy(fishPoint.gameObject);
            }
        }
        fishPoints.Clear();
    }

    private void CreateFishPoints()
    {
        basePosition = FishContainer.GetComponent<RectTransform>().anchoredPosition;
        int maxHealth = Stats.MaxHealth;

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newFishPoint = Instantiate(FishPointPrefab, FishContainer);
            Image fishImage = newFishPoint.GetComponent<Image>();
            fishPoints.Add(fishImage);
        }

        UpdateFishPositions();
        UpdateFishSprites();
    }

    private void Update()
    {
        if (Stats.MaxHealth != fishPoints.Count)
        {
            ClearExistingFishPoints();
            CreateFishPoints();
        }
        else
        {
            UpdateFishSprites();
        }
    }

    private void UpdateFishPositions()
    {
        for (int i = 0; i < fishPoints.Count; i++)
        {
            RectTransform fishTransform = fishPoints[i].GetComponent<RectTransform>();
            fishTransform.anchoredPosition = new Vector2(
                basePosition.x + (i * fishSpacing),
                basePosition.y
            );
        }
    }

    private void UpdateFishSprites()
    {
        for (int i = 0; i < fishPoints.Count; i++)
        {
            if (isVoidBound)
            {
                fishPoints[i].sprite = VoidBoundSprites[i < Stats.CurrentHealth ? 0 : 1];
            }
            else
            {
                fishPoints[i].sprite = FishSprites[i < Stats.CurrentHealth ? 0 : 1];
            }
        }
    }

    public void SetToMaxHealth()
    {
        Stats.CurrentHealth = Stats.MaxHealth;
    }

    public void ActivateVoidBound()
    {
        isVoidBound = true;
        UpdateFishSprites();
    }

    public void DeactivateVoidBound()
    {
        isVoidBound = false;
        UpdateFishSprites();
    }

    public void ResetHealthIcons()
    {
        isVoidBound = false;
        UpdateFishSprites();
    }
}