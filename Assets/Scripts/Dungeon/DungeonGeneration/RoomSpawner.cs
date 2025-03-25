using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public int openingDirection;
    private RoomTemplates templates;
    private int rand;
    public bool spawned = false;
    private MinimapController minimapController;
    private BoxCollider2D boxCollider;

    private bool HasChecked;

    private void Awake()
    {
        StartCoroutine(RecheckTrigger());
        HasChecked = false;
        templates = FindAnyObjectByType<RoomTemplates>();
        minimapController = FindObjectOfType<MinimapController>();
        boxCollider = GetComponent<BoxCollider2D>();

        Invoke("Spawn", 0.1f);
    }

    private void Spawn()
    {
        if (spawned) return;

        GameObject roomInstance = null;

        int currentRoomCount = templates.rooms.Count;
        bool isFirstRoom = currentRoomCount <= 1;

        if (openingDirection == 1)
        {
            if (isFirstRoom)
            {
                List<GameObject> validRooms = templates.rightRooms
                    .Where(room => room.name.Length > 2)
                    .ToList();

                if (validRooms.Count > 0)
                {
                    rand = Random.Range(0, validRooms.Count);
                    roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                }
                else
                {
                    rand = Random.Range(0, templates.rightRooms.Length);
                    roomInstance = Instantiate(templates.rightRooms[rand], transform.position, Quaternion.identity);
                }
            }
            else
            {
                rand = Random.Range(0, templates.rightRooms.Length);
                roomInstance = Instantiate(templates.rightRooms[rand], transform.position, Quaternion.identity);
            }
        }
        else if (openingDirection == 2)
        {
            if (isFirstRoom)
            {
                List<GameObject> validRooms = templates.leftRooms
                    .Where(room => room.name.Length > 2)
                    .ToList();

                if (validRooms.Count > 0)
                {
                    rand = Random.Range(0, validRooms.Count);
                    roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                }
                else
                {
                    rand = Random.Range(0, templates.leftRooms.Length);
                    roomInstance = Instantiate(templates.leftRooms[rand], transform.position, Quaternion.identity);
                }
            }
            else
            {
                rand = Random.Range(0, templates.leftRooms.Length);
                roomInstance = Instantiate(templates.leftRooms[rand], transform.position, Quaternion.identity);
            }
        }
        else if (openingDirection == 3)
        {
            if (isFirstRoom)
            {
                List<GameObject> validRooms = templates.topRooms
                    .Where(room => room.name.Length > 2)
                    .ToList();

                if (validRooms.Count > 0)
                {
                    rand = Random.Range(0, validRooms.Count);
                    roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                }
                else
                {
                    rand = Random.Range(0, templates.topRooms.Length);
                    roomInstance = Instantiate(templates.topRooms[rand], transform.position, Quaternion.identity);
                }
            }
            else
            {
                rand = Random.Range(0, templates.topRooms.Length);
                roomInstance = Instantiate(templates.topRooms[rand], transform.position, Quaternion.identity);
            }
        }
        else if (openingDirection == 4)
        {
            if (isFirstRoom)
            {
                List<GameObject> validRooms = templates.bottomRooms
                    .Where(room => room.name.Length > 2)
                    .ToList();

                if (validRooms.Count > 0)
                {
                    rand = Random.Range(0, validRooms.Count);
                    roomInstance = Instantiate(validRooms[rand], transform.position, Quaternion.identity);
                }
                else
                {
                    rand = Random.Range(0, templates.bottomRooms.Length);
                    roomInstance = Instantiate(templates.bottomRooms[rand], transform.position, Quaternion.identity);
                }
            }
            else
            {
                rand = Random.Range(0, templates.bottomRooms.Length);
                roomInstance = Instantiate(templates.bottomRooms[rand], transform.position, Quaternion.identity);
            }
        }

        if (roomInstance != null && minimapController != null)
        {
            if (roomInstance.CompareTag("Store"))
                minimapController.CreateMinimapIcon(roomInstance.transform.position, "Store");
            else if (roomInstance.CompareTag("Boss"))
                minimapController.CreateMinimapIcon(roomInstance.transform.position, "Boss");
            else if (roomInstance.CompareTag("Cauldron"))
                minimapController.CreateMinimapIcon(roomInstance.transform.position, "Cauldron");
            else if (roomInstance.CompareTag("Enemy"))
                minimapController.CreateMinimapIcon(roomInstance.transform.position, "Enemy");
            else
                minimapController.CreateMinimapIcon(roomInstance.transform.position, "Normal");
        }

        spawned = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpawnPoint"))
        {
            RoomSpawner otherSpawner = other.gameObject.GetComponent<RoomSpawner>();
            if (otherSpawner != null)
            {
                if (!otherSpawner.spawned && !spawned && transform.position.x != 0 && transform.position.y != 0)
                {
                    if (templates != null && templates.closedRooms != null)
                    {
                        if (!templates.IsRoomOccupied(transform.position))
                        {
                            Instantiate(templates.closedRooms[Random.Range(0, templates.closedRooms.Length)], transform.position, Quaternion.identity);
                            HasChecked = true;
                            StopCoroutine(RecheckTrigger());
                            Destroy(gameObject);
                        }
                        else
                        {
                            Debug.LogWarning("Room position is already occupied, skipping spawn.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Templates or closedRoom is not assigned!");
                    }
                }
                spawned = true;
            }
            else
            {
                Debug.Log("RoomSpawner component not found on the 'other' object.");
            }
        }
    }

    private IEnumerator RecheckTrigger()
    {
        if (boxCollider != null && boxCollider.isTrigger)
        {
            yield return new WaitForSeconds(5f);
            boxCollider.enabled = false;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            boxCollider.enabled = true;
        }
    }
}