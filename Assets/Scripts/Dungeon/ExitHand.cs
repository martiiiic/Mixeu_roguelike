using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHand : MonoBehaviour
{
    private GameObject P;
    public GameObject PlayerPoint;
    private Animator anim;
    private BoxCollider2D Collider;
    private RoomTemplates roomTemplates;
    private PlayerState playerState;
    public GameObject E_Key;
    private PowerUpStoreUI PUI;
    private SpriteRenderer spriteRenderer;
    public bool PlayerIsInArea;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        PUI = FindObjectOfType<PowerUpStoreUI>();
        playerState = FindObjectOfType<PlayerState>();
        anim = GetComponent<Animator>();
        roomTemplates = FindObjectOfType<RoomTemplates>();
        Collider = GetComponent<BoxCollider2D>();

        anim.SetBool("Exit", false);
    }


    public void ExitDungeon()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null && boxCollider.isTrigger)
        {
            boxCollider.enabled = false;
        }
        anim.SetBool("Exit", true);
        P = Instantiate(PlayerPoint, roomTemplates.rooms[^1].gameObject.transform.position, Quaternion.identity, roomTemplates.rooms[^1].gameObject.transform);

        SpriteRenderer playerSprite = playerState.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = false;

        Time.timeScale = 1;
    }

    public void DestroyThis()
    {
        Time.timeScale = 1;

        SpriteRenderer playerSprite = playerState.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = true;

        if (playerState != null && !playerState.gameObject.activeSelf)
        {
            playerState.gameObject.SetActive(true);
        }

        Destroy(P);
        roomTemplates.DeleteAllRooms(false);
        Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            PUI = FindObjectOfType<PowerUpStoreUI>();
            PUI.ShowPortalMenu(null,"Exit Hand", "Continue.", 0, spriteRenderer.sprite);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PUI.CloseMenu();
            Time.timeScale = 1;
        }
    }
}
