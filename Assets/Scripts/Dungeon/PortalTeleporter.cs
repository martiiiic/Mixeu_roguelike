using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public int PortalId;
    private PowerUpStoreUI PUI;
    public Animator anim;
    Sprite Sprite;

    public AudioClip[] PortalMusic;
    //0 = normal
    //1 = voidBounds
    //2 =
    //3 =
    //4 =

    private void Awake()
    {
        anim = GetComponent<Animator>();
        Sprite = GetComponent<SpriteRenderer>().sprite;
        PUI = FindObjectOfType<PowerUpStoreUI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switch (PortalId)
            {
                case 0:
                    PUI.ShowPortalMenu(this,"Return To The Dungeons", "Continue.", 0, Sprite);
                    break;
                case 1:
                    PUI.ShowPortalMenu(this,"Void Store Portal", "", 1, Sprite);
                    break;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PUI.CloseMenu();
        }
    }
}
