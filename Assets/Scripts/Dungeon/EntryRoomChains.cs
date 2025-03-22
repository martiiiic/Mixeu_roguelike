using UnityEngine;

public class EntryRoomChains : MonoBehaviour
{
    private RoomTemplates rooms;
    public GameObject Chains;
    private AudioSource Audio;
    private bool initialized;

    private void Awake()
    {
        Audio = GetComponent<AudioSource>();
        rooms = FindObjectOfType<RoomTemplates>();
        if (Chains != null) Chains.SetActive(true);
        initialized = true;
    }

    private void OnEnable()
    {
        if (initialized && Chains != null) Chains.SetActive(true);
    }

    private void Update()
    {
        if (rooms.spawnedBoss && Chains != null)
        {
            Audio.Play();
            Destroy(Chains);
            this.enabled = false;
        }
    }
}