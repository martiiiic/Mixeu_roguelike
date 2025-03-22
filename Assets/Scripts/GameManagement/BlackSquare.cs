using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackSquare : MonoBehaviour
{
    public GameObject Square;

    private void Awake()
    {
        Square.SetActive(true);
    }
}
