using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngelDeath : MonoBehaviour
{
    private PlayerState player;
    private Rigidbody2D rb;
    private SpriteRenderer Sprite;

    private void Start()
    {
        Sprite = GetComponent<SpriteRenderer>();
        player = GetComponentInParent<PlayerState>();
        rb = GetComponent<Rigidbody2D>();
        Sprite.enabled = false;
    }

    public void AngelBehaviour()
    {
        Sprite.enabled = true;
        rb.velocity = new Vector2(0, 0.5f);   
    }

    private void Update()
    {
        if(!player.Dead)
        {
            Sprite.enabled = false;
            this.transform.position = player.transform.position;
            rb.velocity = Vector2.zero;
        }
    }
}
