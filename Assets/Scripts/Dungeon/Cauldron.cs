using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Cauldron : MonoBehaviour
{
    public SpriteRenderer Sprite;
    public GameObject EmptyGameObject;
    public GameObject PowerUp;
    public GameObject Particles;
    private bool PlayerIsClose;

    private PowerUpStoreUI powerUpStoreUI;

    void Awake()
    {
        Particles.SetActive(false);
        PlayerIsClose = false;
        Sprite = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        powerUpStoreUI = FindObjectOfType<PowerUpStoreUI>();
        if (other.CompareTag("Player") && powerUpStoreUI != null)
        {
            powerUpStoreUI.ShowCauldronMenu();
            PlayerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        powerUpStoreUI = FindObjectOfType<PowerUpStoreUI>();
        if (other.CompareTag("Player") && powerUpStoreUI != null)
        {
            powerUpStoreUI.CloseMenu();
            PlayerIsClose = false;
        }
    }

    public void SpawnPowerUp()
    {
        StopAllCoroutines();
        Particles.SetActive(true);
        StartCoroutine(DeleteParticles());
        AudioSource A = GetComponent<AudioSource>();
        if (A != null)
        {
            A.Play();
        }
        GameObject P;
        GameObject E;
        GameObject cauldron = GameObject.FindWithTag("Cauldron");
        Transform S = cauldron.transform;
        E = Instantiate(EmptyGameObject, S.position, Quaternion.identity, gameObject.transform);
        E.transform.position += new Vector3(Random.Range(-2, 2), -2, 0);
        P = Instantiate(PowerUp, E.transform.position, Quaternion.identity, E.transform);
        PowerUpStore PS = P.GetComponent<PowerUpStore>();

        List<PowerUpDefinition> availablePowerUps = GetAvailablePowerUps();

        if (availablePowerUps != null && availablePowerUps.Count > 0)
        {
            PowerUpDefinition randomDefinition = availablePowerUps[Random.Range(0, availablePowerUps.Count)];

            if (randomDefinition.id == 0)
            {
                Destroy(P); Destroy(E);
                SpawnPowerUp();
                return;
            }

            // Set the power-up definition
            PS.SetPowerUpDefinition(randomDefinition);

            PS.calculatedPrice = 0;
        }
        else
        {
            Debug.LogWarning("No available power-up definitions found!");
            Destroy(P); Destroy(E);
            return;
        }

        powerUpStoreUI = FindObjectOfType<PowerUpStoreUI>();
        if (powerUpStoreUI != null && PlayerIsClose)
        {
            powerUpStoreUI.ShowCauldronMenu();
        }
    }

    private List<PowerUpDefinition> GetAvailablePowerUps()
    {
        PowerUpManager powerUpManager = FindObjectOfType<PowerUpManager>();
        if (powerUpManager != null)
        {
            return powerUpManager.GetNormalPowerUps();
        }

        PowerUpDefinition[] powerUps = Resources.LoadAll<PowerUpDefinition>("PowerUps");
        return new List<PowerUpDefinition>(powerUps);
    }

    private IEnumerator DeleteParticles()
    {
        yield return new WaitForSeconds(0.5f);
        Particles.SetActive(false);
    }

    void Update()
    {
        if(PlayerIsClose)
        {
            powerUpStoreUI.ShowCauldronMenu();
        }
    }


}
