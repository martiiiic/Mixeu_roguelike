using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChestRoom : MonoBehaviour
{
    public GameObject[] EnemySpawnpoint;
    public GameObject[] Enemies;
    public Sprite[] Sprites;
    public GameObject PowerUp;
    public GameObject Chains;
    public BoxCollider2D trigger;
    private PowerUpStore powerUpStore;
    private SpriteRenderer spriteRender;
    private AudioSource Audio;
    private Music Music;
    public AudioClip[] SFX;
    private bool TrapActivated;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    public List<PowerUpDefinition> availablePowerUps;
    private void Awake()
    {
        Music = FindObjectOfType<Music>();
        Audio = GetComponent<AudioSource>();
        PowerUp.SetActive(false);
        spriteRender = GetComponent<SpriteRenderer>();
        spriteRender.sprite = Sprites[0];
        Chains.SetActive(false);
        powerUpStore = PowerUp.GetComponent<PowerUpStore>();

        if (availablePowerUps == null || availablePowerUps.Count == 0)
        {
            LoadPowerUpDefinitions();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Music.PlayMusic(Music.combatClips[0], Music.musicSource);
            Audio.clip = SFX[0];
            Audio.Play();
            trigger.enabled = false;
            Chains.SetActive(true);
            foreach (GameObject spawnPoint in EnemySpawnpoint)
            {
                Vector3 SpawnPosition = new Vector3(spawnPoint.transform.position.x, spawnPoint.transform.position.y, -2f);
                int rand = Random.Range(0, Enemies.Length);
                StartCoroutine(SpawnEnemies(SpawnPosition, rand));
            }
        }
    }
    private IEnumerator SpawnEnemies(Vector3 Spawn, int Enemy)
    {
        yield return new WaitForSeconds(3f);
        Audio.clip = SFX[1];
        Audio.Play();
        TrapActivated = true;
        GameObject spawnedEnemy = Instantiate(Enemies[Enemy], Spawn, Quaternion.identity);
        spawnedEnemies.Add(spawnedEnemy);
        foreach (GameObject enemy in spawnedEnemies)
        {
            Enemy E = enemy.GetComponent<Enemy>();
            if (E == null)
            {
                enemy.GetComponentInChildren<Enemy>();
            }
            if (E == null)
            {
                enemy.GetComponentInParent<Enemy>();
            }
            GameObject Player = FindObjectOfType<PlayerState>().gameObject;
            E.target = Player.transform;
        }
    }
    private void Update()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        if (spawnedEnemies.Count == 0 && Chains.activeSelf && TrapActivated)
        {
            powerUpStore = PowerUp.GetComponent<PowerUpStore>();
            Music.StopAllMusic();
            Audio.clip = SFX[2];
            Audio.Play();
            spriteRender.sprite = Sprites[1];
            PowerUp.SetActive(true);

            if (availablePowerUps != null && availablePowerUps.Count > 0)
            {
                PowerUpDefinition randomDefinition = availablePowerUps[Random.Range(0, availablePowerUps.Count)];

                powerUpStore.SetPowerUpDefinition(randomDefinition);

                powerUpStore.calculatedPrice = 0;
            }
            else
            {
                Debug.LogWarning("No available power-up definitions found!");
            }

            Chains.SetActive(false);
            this.enabled = false;
        }
    }

    private void LoadPowerUpDefinitions()
    {

        PowerUpDefinition[] powerUps = Resources.LoadAll<PowerUpDefinition>("PowerUps");
        availablePowerUps = new List<PowerUpDefinition>(powerUps);

        if (availablePowerUps == null || availablePowerUps.Count == 0)
        {
            Debug.LogError("Failed to load PowerUpDefinitions. Please ensure they are properly created and accessible.");
        }
    }
}