using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreNPCAnimations : MonoBehaviour
{
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    
    public void Hurray()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Purchase", true);
    }

    public void BackToNormal()
    {
        anim.SetBool("Purchase", false);
    }

}
