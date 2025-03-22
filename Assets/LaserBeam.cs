using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class LaserBeam : MonoBehaviour
{
    private PlayerStats Stats;
    public int damage = 1;
    public BoxCollider2D Trigger;

    public void DestroyCollider()
    {
        Destroy(Trigger);
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerState Player = collision.GetComponent<PlayerState>();
            Player.TakeDamage(damage);
        }
    }
}
