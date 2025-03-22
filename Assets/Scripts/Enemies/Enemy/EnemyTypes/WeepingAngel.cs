using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeepingAngelEnemy : Enemy
{
    protected override void HandleEnemyBehavior()
    {
        PlayerState playerState = FindObjectOfType<PlayerState>();
        if (playerState == null || target == null)
        {
            return;
        }

        Vector2 playerPosition = playerState.transform.position;
        Vector2 weepingAngelPosition = transform.position;

        Sprite playerSprite = playerState.GetComponent<SpriteRenderer>().sprite;
        bool isFlipped = playerState.GetComponent<SpriteRenderer>().flipX;

        bool isPlayerLooking = IsPlayerLookingAtAngel(playerSprite, isFlipped, playerPosition, weepingAngelPosition);

        if (isPlayerLooking)
        {
            StopMoving();
        }
        else
        {
            // Smooth approach when moving towards the player
            Vector2 direction = (playerPosition - weepingAngelPosition).normalized;
            rb.velocity = Vector2.Lerp(rb.velocity, direction * Speed, 0.15f);
        }
    }

    private bool IsPlayerLookingAtAngel(Sprite playerSprite, bool isFlipped, Vector2 playerPosition, Vector2 angelPosition)
    {
        PlayerState playerState = FindObjectOfType<PlayerState>();
        if (Mathf.Abs(playerPosition.x - angelPosition.x) > Mathf.Abs(playerPosition.y - angelPosition.y))
        {
            if (isFlipped)
            {
                return angelPosition.x > playerPosition.x;
            }
            else
            {
                return angelPosition.x < playerPosition.x;
            }
        }
        else
        {
            if (playerSprite == playerState.CatSprites[1]) // Player is looking up
            {
                return angelPosition.y > playerPosition.y;
            }

            if (playerSprite == playerState.CatSprites[0]) // Player is looking down
            {
                return angelPosition.y < playerPosition.y;
            }
        }

        return false;
    }

    protected override void HandleCollisions(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && rb.velocity.magnitude > 0.2f)
        {
            PlayerState PS = other.gameObject.GetComponent<PlayerState>();
            PlayerStats ps = other.gameObject.GetComponent<PlayerStats>();
            PS.TakeDamage(Mathf.RoundToInt(ps.Defense));
            Destroy(gameObject);
        }
    }
}
