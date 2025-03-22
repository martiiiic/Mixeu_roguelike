using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFurnitureSpawner : MonoBehaviour
{
    public GameObject[] furniturePrefabs;
    private RoomTemplates roomTemplates;

    private void Start()
    {
        roomTemplates = FindObjectOfType<RoomTemplates>();
    }

    public void AddFurnitureToRooms()
    {
        foreach (GameObject room in roomTemplates.rooms)
        {
            if (room != null && IsEligibleForFurniture(room))
            {
                GameObject furniture = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
                Vector3 randomPosition;
                float xOffset;
                float yOffset;
                if (Random.value < 0.5f)
                    xOffset = Random.Range(-3f, -2f);
                else
                    xOffset = Random.Range(2f, 3f);
                if (Random.value < 0.5f)
                    yOffset = Random.Range(-2f, -2f);
                    yOffset = Random.Range(2f, 2f);

                // Apply offsets to room position
                randomPosition = room.transform.position + new Vector3(xOffset, yOffset, 0f);
                Instantiate(furniture, randomPosition, Quaternion.identity);
            }
        }
    }
    private bool IsEligibleForFurniture(GameObject room)
    {
        if (room.CompareTag("Store") || room.CompareTag("Boss") || room.CompareTag("Casino"))
        {
            return false;
        }

        if (RoomContainsTaggedObject(room.transform.position, "Enemy"))
        {
            return true;
        }

        return false;
    }

    private bool RoomContainsTaggedObject(Vector3 roomPosition, string tag)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(roomPosition, 1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
}
