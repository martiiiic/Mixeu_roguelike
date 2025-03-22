using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveEnemiesFromStore : MonoBehaviour
{
    public Vector2 Dimensions;
    BoxCollider2D Trigger;
    private void Awake()
    {
        Trigger = gameObject.AddComponent<BoxCollider2D>();
        Trigger.isTrigger = true;
        Trigger.size = Dimensions;
        
        this.StartCoroutine(TurnOffAfterTime(.75f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameObject parentGameObject = other.transform.parent?.gameObject;

            if (parentGameObject != null)
            {
                Destroy(parentGameObject);
            }
            Trigger.enabled = false;
        }
    }

    private IEnumerator TurnOffAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Trigger.enabled = false;
    }
}
