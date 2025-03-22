using System.Collections.Generic;
using UnityEngine;

public class StoreAudioManager : MonoBehaviour
{
    [Header("Store Configuration")]
    [Tooltip("The audio source that plays in this store")]
    [SerializeField] private AudioSource storeAudioSource;

    [Tooltip("Distance from the store entrance where the effect begins")]
    [SerializeField] private float detectionRadius = 5f;

    [Tooltip("How quickly the store audio effect increases (higher = sharper transition)")]
    [SerializeField] private float exponentialFactor = 2f;

    [Tooltip("How smoothly to transition between audio states")]
    [SerializeField] private float transitionSpeed = 3f;

    [Header("External Audio")]
    [Tooltip("Maximum volume reduction for external audio sources")]
    [Range(0f, 1f)]
    [SerializeField] private float maxExternalMute = 0.9f;

    [Tooltip("Tags of audio sources that should be ignored by this manager")]
    [SerializeField] private string[] ignoredTags = { "UI", "Ambient" };

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    // Reference to the player
    private Transform playerTransform;

    // List of external audio sources to manage
    private List<AudioSource> externalAudioSources = new List<AudioSource>();

    // Original volumes of external sources
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();

    // Is the player currently in the store's influence area?
    private bool playerInRange = false;

    private void Start()
    {
        playerTransform = FindObjectOfType<PlayerState>()?.transform;

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (playerTransform == null)
            {
                Debug.LogError("StoreAudioManager: Player not found!");
                enabled = false;
                return;
            }
        }

        // Make sure we have a store audio source
        if (storeAudioSource == null)
        {
            storeAudioSource = GetComponent<AudioSource>();

            if (storeAudioSource == null)
            {
                Debug.LogError("StoreAudioManager: No audio source assigned or found!");
                enabled = false;
                return;
            }
        }

        // Find all external audio sources
        RefreshAudioSources();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Periodically refresh audio sources
        if (Time.frameCount % 90 == 0)
        {
            RefreshAudioSources();
        }

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Determine if player is in range
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer < detectionRadius;

        // If player just entered range
        if (playerInRange && !wasInRange)
        {
            CurrentPlayerLocation playerLocation = CurrentPlayerLocation.Instance;
            if (playerLocation != null)
            {
                if (playerLocation.IsInShop())
                {
                    AdjustStoreAudioVolume(1.0f);
                }
            }
        }

        // Handle volume transitions
        if (playerInRange)
        {
            float distanceFactor = 1.0f - Mathf.Clamp01(distanceToPlayer / detectionRadius);
            float influenceFactor = Mathf.Pow(distanceFactor, exponentialFactor);

            AdjustExternalAudioVolumes(influenceFactor);

            AdjustStoreAudioVolume(influenceFactor);
        }
        else
        {
            RestoreExternalAudioVolumes();

            AdjustStoreAudioVolume(0.0f);
        }
    }

    private void RefreshAudioSources()
    {
        externalAudioSources.Clear();

        AudioSource[] allSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource source in allSources)
        {
            if (source == storeAudioSource) continue;

            bool shouldIgnore = false;
            foreach (string tag in ignoredTags)
            {
                if (source.CompareTag(tag))
                {
                    shouldIgnore = true;
                    break;
                }
            }

            if (!shouldIgnore)
            {
                externalAudioSources.Add(source);

                if (!originalVolumes.ContainsKey(source))
                {
                    originalVolumes[source] = source.volume;
                }
            }
        }
    }

    private void AdjustExternalAudioVolumes(float influenceFactor)
    {
        foreach (AudioSource source in externalAudioSources)
        {
            if (source == null || !source.isActiveAndEnabled) continue;

            float originalVolume = GetOriginalVolume(source);
            float targetVolume = originalVolume * (1.0f - (maxExternalMute * influenceFactor));

            source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * transitionSpeed);
        }
    }

    private void RestoreExternalAudioVolumes()
    {
        foreach (AudioSource source in externalAudioSources)
        {
            if (source == null || !source.isActiveAndEnabled) continue;

            float originalVolume = GetOriginalVolume(source);
            source.volume = Mathf.Lerp(source.volume, originalVolume, Time.deltaTime * transitionSpeed);
        }
    }

    private void AdjustStoreAudioVolume(float influenceFactor)
    {
        if (storeAudioSource == null) return;

        float originalVolume = GetOriginalVolume(storeAudioSource);
        float targetVolume = originalVolume * influenceFactor;

        storeAudioSource.volume = Mathf.Lerp(storeAudioSource.volume, targetVolume, Time.deltaTime * transitionSpeed);
    }

    private float GetOriginalVolume(AudioSource source)
    {
        if (originalVolumes.TryGetValue(source, out float volume))
        {
            return volume;
        }

        originalVolumes[source] = source.volume;
        return source.volume;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

    #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, "Store Audio Zone");
    #endif
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}