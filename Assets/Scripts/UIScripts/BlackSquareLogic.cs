using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackSquareLogic : MonoBehaviour
{
    private Image Image;

    private void Start()
    {
        Image = GetComponent<Image>();
        Image.enabled = true;
    }
}
