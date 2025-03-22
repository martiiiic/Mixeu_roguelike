using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDamageAbsorb : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Animator anim;
    AudioSource source;
    public AudioClip[] clips;
    public AudioClip shieldBreak;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        spriteRenderer.enabled = false;
        anim.enabled = false;
    }

    public void ShowShield()
    {
        anim.enabled = true;
        spriteRenderer.enabled = true;
        source.PlayOneShot(clips[Random.Range(0,clips.Length)]);
    }

    public void ShieldBreak()
    {
        anim.enabled = true;
        spriteRenderer.enabled = true;
        anim.SetBool("Break", true);
        source.PlayOneShot(shieldBreak);
    }

    public void HideShield()
    {
        anim.SetBool("Break", false);
        anim.enabled = false;
        spriteRenderer.enabled = false;
    }

}
