using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YSortingManager : MonoBehaviour
{
    private SpriteRenderer renderer;
    private PlayerState playerState;
    public int plusCapes;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        playerState = FindObjectOfType<PlayerState>();
        if (renderer != null && playerState != null)
        {
            float playerY = playerState.gameObject.transform.position.y;

            // Adjust the formula to allow lower sorting orders
            int sortingOrder = Mathf.RoundToInt((transform.position.y - playerY) * -10);
            sortingOrder = Mathf.Max(sortingOrder, -4) + plusCapes; // Allowing lower values

            renderer.sortingOrder = sortingOrder;
        }
    }
}
