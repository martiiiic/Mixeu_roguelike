using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPlayerHandler : MonoBehaviour
{
    public int Bombs = 0;
    public GameObject Bomb;
    private Rigidbody2D rb;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F) && Bombs > 0)
        {
            rb = GetComponent<Rigidbody2D>();
            Instantiate(Bomb, new Vector3(gameObject.transform.position.x + (rb.velocity.x * 0.1f), gameObject.transform.position.y + (rb.velocity.y * 0.1f),-8), Quaternion.identity);
            Bombs--;
        }
    }
}
