using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MusicTrack
{
    public string trackName;
    public AudioClip clip;
    public bool canPlayRandomly = true;
    public int[] dungeonNumbers; // Which dungeon numbers this track can play in
    public MusicContext[] allowedContexts;
    [Tooltip("If this is boss music, specify the boss ID here")]
    public int bossID = -1; // -1 means not boss music, otherwise it's the specific boss ID
}

[Serializable]
public enum MusicContext
{
    Ambient,
    Exploration,
    Combat,
    Boss,
    Puzzle,
    Shop,
    Victory,
    GameOver,
    MainMenu,
    Credits
}

public class Music : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource ambientSource;
    public AudioSource musicSource;

    [Header("Legacy Clips - For Backwards Compatibility")]
    public AudioClip[] ambientClips;
    public AudioClip[] musicClips;
    public AudioClip[] zoneClips;
    public AudioClip[] combatClips;

    [Header("Enhanced Music System")]
    [Tooltip("All music tracks in the game, configurable by context and dungeon")]
    public List<MusicTrack> musicTracks = new List<MusicTrack>();

    [Header("Music Settings")]
    public bool CanStartPlaying = true;
    [Tooltip("Default fade time when transitioning between tracks")]
    public float defaultFadeTime = 1.5f;
    [Tooltip("Volume for music playback")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Header("Current State")]
    [SerializeField] private int currentDungeonNumber = 1;
    [SerializeField] private MusicContext currentContext = MusicContext.Exploration;
    private bool isMusicPlaying = false;
    private AudioClip currentClip;
    private MusicTrack currentTrack;
    public Dictionary<MusicContext, MusicTrack> lastPlayedByContext = new Dictionary<MusicContext, MusicTrack>();


    private GameManager gameManager;

    private void Awake()
    {
        // Find the GameManager
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found!");
        }
    }

    private void Start()
    {
        if (gameManager != null)
        {
            currentDungeonNumber = gameManager.DungeonNumber;
        }

        if (CanStartPlaying && musicTracks.Count > 0)
        {
            PlayRandomMusic(MusicContext.Ambient);
        }
    }

    public void PlayMusic(AudioClip clip, AudioSource source, float fadeTime = 1.5f)
    {
        if (!CanStartPlaying || (source.isPlaying && source.clip == clip)) return;
        StartCoroutine(TransitionMusic(clip, source, fadeTime));
    }

    public void PlayContextMusic(MusicContext context, int dungeonNumber = -1, int bossID = -1)
    {
        currentContext = context;

        // If dungeonNumber is not specified, use the GameManager's value
        if (dungeonNumber <= 0 && gameManager != null)
        {
            dungeonNumber = gameManager.DungeonNumber;
        }

        if (dungeonNumber > 0)
        {
            currentDungeonNumber = dungeonNumber;
        }

        // Find appropriate tracks
        List<MusicTrack> eligibleTracks = new List<MusicTrack>();

        foreach (MusicTrack track in musicTracks)
        {
            bool contextMatch = Array.Exists(track.allowedContexts, c => c == context);
            bool dungeonMatch = true;

            if (track.dungeonNumbers != null && track.dungeonNumbers.Length > 0)
            {
                dungeonMatch = Array.Exists(track.dungeonNumbers, d => d == currentDungeonNumber);
            }

            bool bossMatch = true;
            if (context == MusicContext.Boss && bossID >= 0)
            {
                bossMatch = (track.bossID == bossID);
            }

            if (contextMatch && dungeonMatch && bossMatch)
            {
                eligibleTracks.Add(track);
            }
        }

        if (eligibleTracks.Count > 0)
        {
            List<MusicTrack> randomTracks = eligibleTracks.FindAll(t => t.canPlayRandomly);

            List<MusicTrack> tracksToChooseFrom = (randomTracks.Count > 0) ? randomTracks : eligibleTracks;

            MusicTrack selectedTrack;

            if (lastPlayedByContext.ContainsKey(context) && tracksToChooseFrom.Count > 1)
            {
                MusicTrack lastTrack = lastPlayedByContext[context];
                List<MusicTrack> filteredTracks = tracksToChooseFrom.FindAll(t => t != lastTrack);
                selectedTrack = filteredTracks[UnityEngine.Random.Range(0, filteredTracks.Count)];
            }
            else
            {
                selectedTrack = tracksToChooseFrom[UnityEngine.Random.Range(0, tracksToChooseFrom.Count)];
            }

            AudioSource sourceToUse = (context == MusicContext.Ambient) ? ambientSource : musicSource;
            PlayMusic(selectedTrack.clip, sourceToUse, defaultFadeTime);

            currentTrack = selectedTrack;
            lastPlayedByContext[context] = selectedTrack;

            Debug.Log($"Playing '{selectedTrack.trackName}' for context: {context} in dungeon: {currentDungeonNumber}");
        }
        else
        {
            Debug.LogWarning($"No eligible tracks found for context: {context} in dungeon: {currentDungeonNumber}");

            if (context == MusicContext.Ambient && ambientClips.Length > 0)
            {
                PlayMusic(ambientClips[UnityEngine.Random.Range(0, ambientClips.Length)], ambientSource);
            }
            else if (context == MusicContext.Combat && combatClips.Length > 0)
            {
                PlayMusic(combatClips[UnityEngine.Random.Range(0, combatClips.Length)], musicSource);
            }
            else if (musicClips.Length > 0)
            {
                PlayMusic(musicClips[UnityEngine.Random.Range(0, musicClips.Length)], musicSource);
            }
        }
    }

    public void PlayRandomMusic(MusicContext context)
    {
        PlayContextMusic(context);
    }

    public void PlayBossMusic(int bossID)
    {
        PlayContextMusic(MusicContext.Boss, -1, bossID);
    }

    public void SetDungeonNumber(int dungeonNumber)
    {
        this.currentDungeonNumber = dungeonNumber;
    }

    // New method to update music when entering a new dungeon
    public void OnNewDungeon()
    {
        if (gameManager != null)
        {
            currentDungeonNumber = gameManager.DungeonNumber;
            Debug.Log($"Music system updated to dungeon number: {currentDungeonNumber}");

            // Optionally play new ambient music based on the new dungeon
            PlayContextMusic(MusicContext.Ambient);
        }
    }

    private IEnumerator TransitionMusic(AudioClip newClip, AudioSource source, float fadeTime)
    {
        if (source.isPlaying && source.clip != newClip)
        {
            float elapsedTime = 0f;
            float startVolume = source.volume;
            while (elapsedTime < fadeTime)
            {
                source.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            source.Stop();
            source.volume = 0f;
        }
        if (source.clip != newClip)
        {
            currentClip = newClip;
            isMusicPlaying = true;
            source.clip = newClip;
            source.Play();
            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                source.volume = Mathf.Lerp(0f, musicVolume, elapsedTime / fadeTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            source.volume = musicVolume;
        }
    }

    public void StopAllMusic(float fadeTime = 1.5f)
    {
        Debug.Log("StopMusic called from: " + System.Environment.StackTrace);
        if (ambientSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(ambientSource, fadeTime));
        }
        if (musicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(musicSource, fadeTime));
        }
        isMusicPlaying = false;
    }

    private IEnumerator FadeOutMusic(AudioSource source, float fadeTime)
    {
        float elapsedTime = 0f;
        float startVolume = source.volume;
        while (elapsedTime < fadeTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        source.Stop();
        source.volume = 0f;
    }

    private void Update()
    {
        if (gameManager != null && currentDungeonNumber != gameManager.DungeonNumber)
        {
            currentDungeonNumber = gameManager.DungeonNumber;
        }

        if (CanStartPlaying && !isMusicPlaying && !ambientSource.isPlaying)
        {
            if (musicTracks.Count > 0)
            {
                PlayRandomMusic(MusicContext.Ambient);
            }
            else if (ambientClips.Length > 0)
            {
                ambientSource.clip = ambientClips[UnityEngine.Random.Range(0, ambientClips.Length)];
                ambientSource.loop = true;
                ambientSource.Play();
            }
        }
    }
}