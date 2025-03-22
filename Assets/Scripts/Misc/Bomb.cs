using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject Explosion;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Activate", true);
    }
    public void Explode()
    {
        Instantiate(Explosion, gameObject.transform.position, Quaternion.identity);
        Destroy(gameObject);
        return;
    }
}
