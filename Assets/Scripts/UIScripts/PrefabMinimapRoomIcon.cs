using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabMinimapRoomIcon : MonoBehaviour
{
    private MinimapController minimapController;

    private void Awake()
    {
        GameObject roomInstance = this.gameObject;
        minimapController = FindObjectOfType<MinimapController>();
        minimapController.CreateMinimapIcon(roomInstance.transform.position, "Normal");

    }

}
