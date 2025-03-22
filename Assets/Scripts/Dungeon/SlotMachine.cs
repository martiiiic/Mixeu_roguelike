using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    public Sprite UISprite;
    public int Price;
    private PowerUpStoreUI powerUpStoreUI;
    private GameManager manager;
    public BoxCollider2D boxCollider;
    public BoxCollider2D trigger;
    private CollectibleCoin coins;
    private CoinSystem coinSystem;
    private AudioSource SlotSound;
    public AudioClip[] Sounds;

    private Animator anim;

    public void Awake()
    {
        SlotSound = gameObject.AddComponent<AudioSource>();
        transform.position = new Vector3(transform.position.x, transform.position.y + 1f, -4f);
        coinSystem = FindFirstObjectByType<CoinSystem>();
        coins = FindObjectOfType<CollectibleCoin>();
        anim = GetComponent<Animator>();
        powerUpStoreUI = FindObjectOfType<PowerUpStoreUI>();
        manager = FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            powerUpStoreUI = FindFirstObjectByType<PowerUpStoreUI>();
            Price = Mathf.RoundToInt(coinSystem.Coins * 0.5f);
            powerUpStoreUI.ShowMenuSlotMachine(Price);
        }
        
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            powerUpStoreUI.CloseMenu();
        }
    }

    public void PlaySound()
    {
        SlotSound.clip = Sounds[0];
        SlotSound.Play();
    }

    public void ExplodeSound()
    {
        SlotSound.clip = Sounds[1];
        SlotSound.Play();
    }

    public void StartGambling()
    {
        trigger.enabled = false;
        anim.SetBool("Activate", true);
    }
    public void StartSpinning()
    {
        anim.SetBool("Roll", true);
        StartCoroutine(Explode(5));
    }
    private IEnumerator Explode(float Time)
    {
        yield return new WaitForSeconds(Time);
        anim.SetBool("Explode", true);
    }
    public void Exploded()
    {
        int rand = Random.Range(0, 101);

        if (rand == 0)
        {
            coins.SpawnCoin(100 * Price, this.transform);
        }
        else if (rand > 0 && rand <= 10)
        {
            coins.SpawnCoin(Mathf.RoundToInt(0.05f * Price), this.transform);
        }
        else if (rand > 10 && rand <= 25)
        {
            coins.SpawnCoin(Mathf.RoundToInt(0.25f * Price), this.transform);
        }
        else if (rand > 25 && rand <= 50)
        {
            coins.SpawnCoin(Mathf.RoundToInt(0.5f * Price), this.transform);
        }
        else if (rand > 50 && rand <= 72)
        {
            coins.SpawnCoin(Mathf.RoundToInt(0.75f * Price), this.transform);
        }
        else if (rand > 72 && rand <= 77)
        {
            coins.SpawnCoin(1 * Price, this.transform);
        }
        else if (rand > 77 && rand <= 93)
        {
            coins.SpawnCoin(2 * Price, this.transform);
        }
        else if (rand > 93 && rand <= 97)
        {
            coins.SpawnCoin(4 * Price, this.transform);
        }
        else if (rand > 97)
        {
            coins.SpawnCoin(10 * Price, this.transform);
        }
        boxCollider.enabled = false;
        Destroy(gameObject);
    }
}
