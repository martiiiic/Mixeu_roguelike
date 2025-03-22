using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamWarn : MonoBehaviour
{
    public GameObject LaserBeam;
    
    public void Playsound()
    {
        AudioSource s = GetComponent<AudioSource>();
        s.Play();
    }

    public void DestroyAndInstantiateBeam()
    {
        Instantiate(LaserBeam,gameObject.transform.position + new Vector3(0,1.3f,0),Quaternion.identity);
        Destroy(gameObject);
    }
}
