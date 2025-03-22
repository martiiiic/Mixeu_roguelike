using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public SpriteRenderer backgroundRenderer;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color disabledColor = Color.gray; // New color for disabled buttons

    public TextMeshPro GeneralVolumePercentage;
    public TextMeshPro MusicVolumePercentage;

    public TextMeshPro DifficultyText;

    public AudioMixer generalMixer;
    public AudioMixer musicMixer;

    public string[] Difficulties;
    public static string[] Dificulties;

    public static float GVolume = 100;
    public static float MVolume = 100;

    public GameObject Music;
    public AudioClip[] StartingMusic;

    private MainMenu mm;

    public int ButtonID;
    private int i;

    private bool isHovered = false;
    private bool isButtonEnabled = true; // New flag to track if button is enabled

    public Camera cam;

    public static int difficulty;

    public GameObject Particles;

    public AudioMixerGroup MusicMix;

    // Reference to all menu buttons for disabling
    private static MainMenuButtons[] allButtons;

    void Awake()
    {
        i = 1;
        if (DifficultyText != null) { DifficultyText.text = "Medium"; }
        difficulty = 1;
        Dificulties = new string[Difficulties.Length];
        for (int j = 0; j < Difficulties.Length; j++)
        {
            Dificulties[j] = Difficulties[j];
        }

        if (musicMixer != null) { musicMixer.SetFloat("Volume", Mathf.Log10(MVolume / 100) * 20); MusicVolumePercentage.text = MVolume + "%"; }
        if (generalMixer != null) { generalMixer.SetFloat("Volume", Mathf.Log10(GVolume / 100) * 20); GeneralVolumePercentage.text = GVolume + "%"; }

        if (backgroundRenderer != null)
            backgroundRenderer.material.color = normalColor; // Set default color
    }

    void Start()
    {
        // Find all menu buttons in the scene
        allButtons = FindObjectsOfType<MainMenuButtons>();
    }

    void Update()
    {
        if (!isButtonEnabled)
        {
            if (isHovered)
            {
                isHovered = false;
                ChangeColor(disabledColor);
            }
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isHovered)
                {
                    isHovered = true;
                    ChangeColor(hoverColor);
                }

                // Click detection
                if (Input.GetMouseButtonDown(0))
                {
                    OnClick();
                }
            }
            else
            {
                if (isHovered)
                {
                    isHovered = false;
                    ChangeColor(normalColor);
                }
            }
        }
        else
        {
            if (isHovered)
            {
                isHovered = false;
                ChangeColor(normalColor);
            }
        }
    }

    void ChangeColor(Color newColor)
    {
        if (backgroundRenderer != null)
            backgroundRenderer.material.color = newColor;
    }
    public void DisableButton()
    {
        isButtonEnabled = false;
        ChangeColor(disabledColor);
    }

    private void DisableAllOtherButtons()
    {
        foreach (MainMenuButtons button in allButtons)
        {
                button.DisableButton();
        }
    }

    void OnClick()
    {
        mm = FindObjectOfType<MainMenu>();
        switch (ButtonID)
        {
            case 0:
                Debug.Log("NewRun");
                mm.ToNewRunMenu();
                break;
            case 1:
                Debug.Log("Settings");
                mm.ToSettingsMenu();
                break;
            case 2:
                Debug.Log("LogBook");
                break;
            case 3:
                Debug.Log("Stats");
                break;
            case 4:
                Debug.Log("Quit");
                Application.Quit();
                break;
            case 5:
                Debug.Log("StartGame");

                var player = FindObjectOfType<PlayerState>();
                if (player != null)
                    Destroy(player.gameObject);

                var gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                    Destroy(gameManager.gameObject);

                DisableAllOtherButtons();
                StartCoroutine(IncrementFOV());
                AudioSource MusicComp = Music.GetComponent<AudioSource>();
                MusicComp.Stop();
                AudioSource StartMusic = Music.gameObject.AddComponent<AudioSource>();
                StartMusic.outputAudioMixerGroup = MusicMix;
                StartMusic.loop = false;
                StartMusic.clip = StartingMusic[difficulty];
                StartMusic.Play();
                break;
            case 6:
                Debug.Log("Difficulty");
                i += 1;
                if (i >= Difficulties.Length) { i = 0; }
                DifficultyText.text = Difficulties[i];
                difficulty = i;

                if (DifficultyText.text == "Mixeu's torment")
                {
                    DifficultyText.fontSize = 16;
                }
                else
                {
                    DifficultyText.fontSize = 22;
                }
                break;
            case 7:
                Debug.Log("QuitRunMenu");
                mm.RunMenuToMainMenu();
                break;
            case 8:
                Debug.Log("GeneralVolume");
                GVolume += 25;
                if (GVolume > 100) { GVolume = 0; }
                GeneralVolumePercentage.text = GVolume + "%";
                generalMixer.SetFloat("Volume", Mathf.Log10(GVolume / 100) * 20);
                if (GVolume == 0)
                {
                    generalMixer.SetFloat("Volume", -80);
                }
                break;
            case 9:
                Debug.Log("MusicVolume");
                MVolume += 25;
                if (MVolume > 100) { MVolume = 0; }
                MusicVolumePercentage.text = MVolume + "%";
                musicMixer.SetFloat("Volume", Mathf.Log10(MVolume / 100) * 30);
                if (MVolume == 0)
                {
                    musicMixer.SetFloat("Volume", -80);
                }
                break;
        }
    }

    private float startFOV = 50f;
    private float targetFOV = 179f;
    private float duration = 5.5f;
    IEnumerator IncrementFOV()
    {
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            if (timeElapsed >= 4.1f && Particles != null)
            {
                Particles.SetActive(true);
            }
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, Mathf.Pow(timeElapsed / duration, 2));

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Particles.SetActive(false);
        StartCoroutine(WaitToEnterGame());

        cam.fieldOfView = targetFOV;
    }

    private IEnumerator WaitToEnterGame()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Gameplay");
        asyncLoad.allowSceneActivation = false;
        yield return new WaitForSeconds(1f);
        Debug.Log("Starting Game, Difficulty + " + difficulty);
        yield return new WaitForSeconds(1f);
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("MainMenu");
        asyncLoad.allowSceneActivation = true;
    }
}