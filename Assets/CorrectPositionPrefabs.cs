using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectPositionPrefabs : MonoBehaviour
{
    public GameObject ObjectToCorrect;

    private void Awake()
    {
        ObjectToCorrect.transform.localPosition = Vector3.zero;
    }
}
