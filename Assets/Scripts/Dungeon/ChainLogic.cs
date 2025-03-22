using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLogic : MonoBehaviour
{
    private BoxCollider2D Trigger;
    private Vector3 chainPos;
    private void Awake()
    {
        Trigger = GetComponent<BoxCollider2D>();
        Trigger.enabled = true;
        this.chainPos = this.transform.position;
        this.transform.position = new Vector2 (1000, 1000);
        StartCoroutine(DestroyTrigger(6));
    }

    private void Start()
    {
        StartCoroutine(CoolDown());
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(0.01f);
        this.transform.position = this.chainPos;
    }

    private IEnumerator DestroyTrigger(float timer)
    {
        yield return new WaitForSeconds(timer);
        Trigger.enabled = false;
        this.enabled = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }

    }
}
