using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandScript : MonoBehaviour
{
    public GameObject Player;
    public GameObject PlayerPoint;
    private GameObject PlayerPoint_;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void ActivateAnimation()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        if (cam != null)
        {
            cam.transform.position = new Vector2(0, 0);
        }
        Invoke("placePlayerPoint", 0.015f);
    }

    public void LetPlayer()
    {
        Time.timeScale = 1;
        if (Player == null)
        {
            Debug.LogWarning("Player reference is null in HandScript.LetPlayer()");
            return;
        }

        Vector3 PlayerPos = Player.transform.position;
        PlayerPos.x = 0;
        PlayerPos.z = -3;
        Player.transform.position = PlayerPos;

        if (PlayerPoint_ != null)
        {
            Collider2D collider = PlayerPoint_.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            Destroy(PlayerPoint_);
        }

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.name == "PlayerPoint(Clone)")
            {
                // Disable collider first to prevent trigger callbacks
                Collider2D collider = obj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
                Destroy(obj);
            }
        }

        Player.SetActive(true);

        // Find weapon script safely
        WeaponScript WS = FindAnyObjectByType<WeaponScript>();
        if (WS != null)
        {
            WS.StartCoroutine(WS.ResetWeaponPosition(WS.stabOffset));
        }
    }

    private void placePlayerPoint()
    {
        Time.timeScale = 1;
        if (PlayerPoint_ != null)
        {
            Destroy(PlayerPoint_);
        }
        if (Player == null)
        {
            Debug.LogWarning("Player reference is null in HandScript.placePlayerPoint()");
            return;
        }
        Vector2 PlayerPos = Player.transform.position;
        PlayerPos.x = 10000;
        Player.transform.position = PlayerPos;

        // Add a delay before deactivating the player
        StartCoroutine(DelayedPlayerDeactivation());
    }

    private IEnumerator DelayedPlayerDeactivation()
    {
        // Wait for physics update to complete
        yield return new WaitForFixedUpdate();

        if (PlayerPoint != null)
        {
            PlayerPoint_ = Instantiate(PlayerPoint, Vector3.zero, Quaternion.identity);
            Player.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PlayerPoint prefab reference is null in HandScript.placePlayerPoint()");
        }
    }

    public void AnimationFinish()
    {
        gameObject.SetActive(false);
    }
}