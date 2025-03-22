using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnDungeonEnterText : MonoBehaviour
{
    private TextMeshProUGUI DungeonText;
    public Image Skull;
    public TextMeshProUGUI DifficultyText;
    public TextMeshProUGUI WelcomeText;
    public string[] WelcomeMessages;
    public AudioClip[] OnDungeonEntranceAudio;
    private GameManager Manager;
    private AudioSource Source;

    void Start()
    {
        Source = GetComponent<AudioSource>();
        DungeonText = GetComponent<TextMeshProUGUI>();
        Manager = FindFirstObjectByType<GameManager>();
        DungeonText.enabled = false;
        ShowDungeonText();
    }

    public void ShowDungeonText(bool NormalDungeon = true, string Text = "")
    {
        if (NormalDungeon) 
        {

            Manager = FindFirstObjectByType<GameManager>();


            Source = GetComponent<AudioSource>();
            Source.clip = OnDungeonEntranceAudio[0];
            Source.Play();

            int rand = Random.Range(0, WelcomeMessages.Length);

            WelcomeText.enabled = true;
            WelcomeText.text = WelcomeMessages[rand];

            if (DungeonText != null)
            {
                DungeonText.enabled = true; DungeonText.text = "Dungeon " + Manager.DungeonNumber; DifficultyText.text = "x" + (Mathf.RoundToInt((Manager.enemyDamageMultiplier + Manager.enemyHealthMultiplier) / 2));
                StartCoroutine(FadeOutDungeonText());
            }

        }

        else
        {

            Source = GetComponent<AudioSource>();
            Source.clip = OnDungeonEntranceAudio[0];
            Source.Play();

            WelcomeText.enabled = false;
            if (DungeonText != null)
            {
                DungeonText.enabled = true; DungeonText.text = Text; DifficultyText.text = "x" + (Mathf.RoundToInt((Manager.enemyDamageMultiplier + Manager.enemyHealthMultiplier) / 2));
                StartCoroutine(FadeOutDungeonText());
            }

        }
        
        
    }

    private IEnumerator FadeOutDungeonText()
    {
        Color originalColor = DungeonText.color;
        Color originalColor2 = WelcomeText.color;

        float fadeDuration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            DungeonText.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            WelcomeText.color = new Color(originalColor2.r, originalColor2.g, originalColor2.b, newAlpha);

            yield return null;
        }

        DungeonText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        DungeonText.enabled = false;
        WelcomeText.color = new Color(originalColor2.r, originalColor2.g, originalColor2.b, 0f);
        WelcomeText.enabled = false;
    }
}
