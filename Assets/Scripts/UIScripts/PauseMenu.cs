using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PauseMenu : MonoBehaviour
{
    public GameObject Menu;
    public GameObject Settings;
    public GameObject Stats;

    public static float _GVolume = 100;
    public static float _MVolume = 100;

    private bool canToggle = true;
    private float toggleCooldown = 0.5f;
    public AudioMixer mixer;
    public string cutOffParameter = "CutOff";
    public string resonanceParameter = "Resonance";
    public float fadeDuration = 1f; // Time for the fade effect
    private bool isPaused = false;
    private Animator anim;
    public GameObject DeathScreen;

    public AudioMixer GeneralMix;
    public AudioMixer MusicMix;

    public TextMeshProUGUI GMixText;
    public TextMeshProUGUI MMixText;


    private void Awake()
    {
        _GVolume = MainMenuButtons.GVolume;
        _MVolume = MainMenuButtons.MVolume;
        MMixText.text = _MVolume + "%";
        GMixText.text = _GVolume + "%";
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        _GVolume = MainMenuButtons.GVolume;
        _MVolume = MainMenuButtons.MVolume;
        MMixText.text = _MVolume + "%";
        GMixText.text = _GVolume + "%";
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && canToggle)
        {
            if (DeathScreen.activeSelf) { Menu.SetActive(false); return; }

            if (Settings.activeSelf) { ToPauseMenu(); return; }
            if (Stats.activeSelf) { ToPauseMenu(); return; }
            Menu.SetActive(!Menu.activeSelf);
            StartCoroutine(ToggleCooldown());
            if (Menu.activeSelf)
            {
                anim.SetBool("Start", false);
                anim.SetBool("Stop", true);
                isPaused = true;
                Time.timeScale = 0;
                StartCoroutine(FadeAudioParameters(3.5f, 2000f, fadeDuration)); // Fade to paused values
            }
            else
            {
                anim.SetBool("Start", false);
                anim.SetBool("Stop", true);
                isPaused = false;
                Time.timeScale = 1;
                StartCoroutine(FadeAudioParameters(1f, 22000f, fadeDuration)); // Fade to unpaused values
            }
        }
    }

    public void a()
    {
        anim.SetBool("Start", true);
        anim.SetBool("Stop", false);
    }

    private IEnumerator ToggleCooldown()
    {
        canToggle = false;
        yield return new WaitForSecondsRealtime(toggleCooldown);
        canToggle = true;
    }

    private IEnumerator FadeAudioParameters(float targetCutOff, float targetResonance, float duration)
    {
        float elapsedTime = 0f;

        float currentCutOff, currentResonance;
        mixer.GetFloat(cutOffParameter, out currentCutOff);
        mixer.GetFloat(resonanceParameter, out currentResonance);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            // Linearly interpolate the values for CutOff and Resonance
            float newCutOff = Mathf.Lerp(currentCutOff, targetCutOff, t);
            float newResonance = Mathf.Lerp(currentResonance, targetResonance, t);

            mixer.SetFloat(cutOffParameter, newCutOff);
            mixer.SetFloat(resonanceParameter, newResonance);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure the final values are set
        mixer.SetFloat(cutOffParameter, targetCutOff);
        mixer.SetFloat(resonanceParameter, targetResonance);
    }

    public void Resume()
    {
        if (!canToggle) { return; }
        Menu.SetActive(!Menu.activeSelf);
        StartCoroutine(ToggleCooldown());
        if (Menu.activeSelf)
        {
            Time.timeScale = 0;
            StartCoroutine(FadeAudioParameters(4f, 1000f, fadeDuration)); // Fade to paused values
        }
        else
        {
            anim.SetBool("Stop", true);
            anim.SetBool("Start", false);
            Time.timeScale = 1;
            StartCoroutine(FadeAudioParameters(1f, 22000f, fadeDuration)); // Fade to unpaused values
        }
    }

    public void ToSettings()
    {
        Settings.SetActive(true);
        Stats.SetActive(false);
        Menu.SetActive(false);
    }

    public void ToPauseMenu()
    {
        Settings.SetActive(false);
        Stats.SetActive(false);
        Menu.SetActive(true);
    }

    public void ToStatsMenu()
    {
        Settings.SetActive(false);
        Stats.SetActive(true);
        Menu.SetActive(false);
    }

    public void GeneralMixButton()
    {
        _GVolume += 25;
        if (_GVolume > 100) {  _GVolume = 0; }
        GMixText.text = _GVolume + "%";
        GeneralMix.SetFloat("Volume", Mathf.Log10(_GVolume / 100) * 20);
        if (_GVolume == 0)
        {
            GeneralMix.SetFloat("Volume", -80);
        }
    }

    public void MusicMixButton()
    {
        _MVolume += 25;
        if (_MVolume > 100) { _MVolume = 0; }
        MMixText.text = _MVolume + "%";
        MusicMix.SetFloat("Volume", Mathf.Log10(_MVolume / 100) * 20);
        if (_MVolume == 0)
        {
            MusicMix.SetFloat("Volume", -80);
        }
    }

    public void ReturnToMainMenu()
    {
        StopAllCoroutines();
        mixer.SetFloat(cutOffParameter, 1f);
        mixer.SetFloat(resonanceParameter, 22000f);
        Camera cam = FindObjectOfType<Camera>();
        Time.timeScale = 1;
        MainMenuButtons.GVolume = _GVolume;
        MainMenuButtons.MVolume = _MVolume;
        SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        Debug.Log("LoadingMenu");
    }
}
