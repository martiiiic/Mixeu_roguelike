using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUIanimatiob : MonoBehaviour
{
    public void ActivateLoop()
    {
        BossRoomUI UI = GetComponentInParent<BossRoomUI>();
        UI.ActivateLoop();
    }
}
