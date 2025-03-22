using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Linq;

public class DeathScreenScript : MonoBehaviour
{
    public GameObject DeathScreen;
    public GameObject ObjectsBox;

    public GameObject[] AppearanceObjectsInOrder;

    public GameObject FlyingCat;
    public TextMeshProUGUI CatText;
    public string[] CatPhrases;

    public AudioMixer mixer;
    public AudioMixer MusicMixer;

    public AudioClip[] Sounds;

    private AudioSource DeathMenuMusic;
    private AudioSource SFX;

    private string Pitch = "Pitch";
    private string resonanceParameter = "CutOff";
    private string cutOff = "Resonance";

    private GameManager Manager;

    public float Amplitude;
    public float Frequency;
    private float Value;

    private Music music;

    

    private void Start()
    {
        SFX = gameObject.AddComponent<AudioSource>();
        SFX.loop = false; SFX.playOnAwake = false;
        SFX.volume = 0.5f;
        Manager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {

        if (DeathScreen.activeSelf) { Value = Amplitude * Mathf.Abs(Mathf.Sin(Frequency * Time.realtimeSinceStartup)); 
            FlyingCat.transform.localScale = new Vector3(1,Value + 0.6f,1); FlyingCat.transform.rotation = Quaternion.Euler(0, 0, Value * -30);
            CatText.gameObject.transform.localScale = new Vector3(-Value + 1.4f, 1, 1); CatText.gameObject.transform.rotation = Quaternion.Euler(-19.714f, -22.905f, Value + 20f);
        }

    }

    public IEnumerator ShowDeathScreen()
    {

        music = FindObjectOfType<Music>();

        DeathMenuMusic = GetComponent<AudioSource>();
        DeathMenuMusic.volume = 0.4f;

        CatText.text = string.Empty;

        GameObject[] objects = ObjectsBox.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray();

        foreach (GameObject obj in objects)
        {
            obj.transform.localPosition += new Vector3(-7.25f, 14.5f, 0);
        }

        StartCoroutine(FadeTimeScaleIn(4f));
        StartCoroutine(FadeInAudio(3f));
        yield return new WaitForSecondsRealtime(2f);
        DeathScreen.SetActive(true);
        SFX.PlayOneShot(Sounds[0]);
        AppearanceObjectsInOrder[0].SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);
        SFX.PlayOneShot(Sounds[0]);
        AppearanceObjectsInOrder[1].SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);

        SFX.PlayOneShot(Sounds[0]);

        MusicMixer.SetFloat("Volume", Mathf.Log10(PauseMenu._MVolume / 100) * 20);
        
        AppearanceObjectsInOrder[2].SetActive(true);
        AppearanceObjectsInOrder[3].SetActive(true);

        yield return new WaitForSecondsRealtime(0.25f);

        SFX.PlayOneShot(Sounds[0]);

        AppearanceObjectsInOrder[4].SetActive(true);
        AppearanceObjectsInOrder[5].SetActive(true);

        DeathMenuMusic.Play();
        music.CanStartPlaying = false;
        music.StopAllMusic(0f);
        mixer.SetFloat(Pitch, 1f);
        mixer.SetFloat(cutOff, 15000f);
        mixer.SetFloat(resonanceParameter, 4f);

        yield return new WaitForSecondsRealtime(0.25f);

        SFX.PlayOneShot(Sounds[0]);

        AppearanceObjectsInOrder[6].SetActive(true);
        AppearanceObjectsInOrder[7].SetActive(true);
        yield return new WaitForSecondsRealtime(0.15f);

        SFX.PlayOneShot(Sounds[0]);

        AppearanceObjectsInOrder[8].SetActive(true);
        AppearanceObjectsInOrder[9].SetActive(true);

        StartCoroutine(TypeSentenceWordByWord(CatText, CatPhrases[(Random.Range(0, CatPhrases.Length))], 0.1f));

    }

    public void QuitToMenu()
    {
        DeathScreen.SetActive(false);
        int i = AppearanceObjectsInOrder.Length - 1;
        while (i >= 0) 
        {
            AppearanceObjectsInOrder[i].SetActive(false);
            i--;
        }

        music.CanStartPlaying = true;
        MusicMixer.SetFloat("Volume", Mathf.Log10(PauseMenu._MVolume / 100) * 20);
        mixer.SetFloat(Pitch, 1);
        mixer.SetFloat(cutOff, 22000f);
        mixer.SetFloat(resonanceParameter, 1f);
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void NewRun()
    {
        DeathScreen.SetActive(false);
        int i = AppearanceObjectsInOrder.Length - 1;
        while (i >= 0)
        {
            AppearanceObjectsInOrder[i].SetActive(false);
            i--;
        }
        ObjectsBox.GetComponent<ObjectStatsMenu>().ResetAllPowerUps();
        StartCoroutine(FadeOut(DeathMenuMusic, 1.25f));
        music.CanStartPlaying = true;
        music.PlayMusic(music.musicClips[0], music.musicSource);
        StartCoroutine(FadeOutAudio(3f));
        StartCoroutine(FadeTimeScaleOut(1f));
    }

    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
    }

    private IEnumerator FadeInAudio(float duration)
    {
        float startTime = Time.unscaledTime;
        float endTime = startTime + duration;

        float startPitch = 1f;
        float startCutOff = 22000f;
        float startResonance = 1f;
        float startVolume = Mathf.Log10(PauseMenu._MVolume / 100) * 20;

        float targetPitch = 0.5f;
        float targetCutOff = 2000f;
        float targetResonance = 5f;

        while (Time.unscaledTime < endTime)
        {
            float t = (Time.unscaledTime - startTime) / duration;

            // Interpolamos los valores
            float currentPitch = Mathf.Lerp(startPitch, targetPitch, t);
            float currentCutOff = Mathf.Lerp(startCutOff, targetCutOff, t);
            float currentResonance = Mathf.Lerp(startResonance, targetResonance, t);
            float currentVolume = Mathf.Lerp(startVolume, 0f, t);

            mixer.SetFloat(Pitch, currentPitch);
            mixer.SetFloat(cutOff, currentCutOff);
            mixer.SetFloat(resonanceParameter, currentResonance);
            MusicMixer.SetFloat("Volume", currentVolume);

            yield return null;
        }

        mixer.SetFloat(Pitch, targetPitch);
        mixer.SetFloat(cutOff, targetCutOff);
        mixer.SetFloat(resonanceParameter, targetResonance);
        MusicMixer.SetFloat("Volume", 0f);
    }

    private IEnumerator FadeOutAudio(float duration)
    {
        float startTime = Time.unscaledTime;
        float endTime = startTime + duration;

        float startPitch = 0.5f;
        float startCutOff = 2000f;
        float startResonance = 5f;

        float targetPitch = 1f;
        float targetCutOff = 22000f;
        float targetResonance = 1f;

        while (Time.unscaledTime < endTime)
        {
            float t = (Time.unscaledTime - startTime) / duration;

            float currentPitch = Mathf.Lerp(startPitch, targetPitch, t);
            float currentCutOff = Mathf.Lerp(startCutOff, targetCutOff, t);
            float currentResonance = Mathf.Lerp(startResonance, targetResonance, t);

            mixer.SetFloat(Pitch, currentPitch);
            mixer.SetFloat(cutOff, currentCutOff);
            mixer.SetFloat(resonanceParameter, currentResonance);

            yield return null;
        }

        mixer.SetFloat(Pitch, targetPitch);
        mixer.SetFloat(cutOff, targetCutOff);
        mixer.SetFloat(resonanceParameter, targetResonance);

        StopAllCoroutines();
    }

    public IEnumerator FadeTimeScaleIn(float duration)
    {
        float startTime = Time.unscaledTime;
        float endTime = startTime + duration;

        while (Time.unscaledTime < endTime)
        {
            float t = (Time.unscaledTime - startTime) / duration;
            Time.timeScale = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        Time.timeScale = 0;
    }

    public IEnumerator FadeTimeScaleOut(float duration)
    {
        float startTime = Time.unscaledTime;
        float endTime = startTime + duration;

        while (Time.unscaledTime < endTime)
        {
            float t = (Time.unscaledTime - startTime) / duration;
            Time.timeScale = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        Time.timeScale = 1;
    }

    private IEnumerator TypeSentenceWordByWord(TextMeshProUGUI textComponent, string sentence, float delayBetweenWords)
    {
        yield return new WaitForSecondsRealtime(1f);
        textComponent.text = "";

        foreach (char letter in sentence)
        {
            SFX.PlayOneShot(Sounds[1]);
            textComponent.text += letter;
            yield return new WaitForSecondsRealtime(delayBetweenWords);
        }
    }

}

