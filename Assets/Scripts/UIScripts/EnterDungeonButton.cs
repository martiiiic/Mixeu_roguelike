using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnterDungeonButton : MonoBehaviour
{
    private EnterBossRoom BR;
    public GameObject Player;
    public BossBar BossBar;
    private Vector3 Position;

    public void EnterBossRoom()
    {
        BossBar = FindObjectOfType<BossBar>();
        BR = FindObjectOfType<EnterBossRoom>();
        if (BR == null) {Debug.Log("Failed"); return; }
        if (BR.CurrentPlayerChain == 1)
        {
            Position = new Vector3(0, -3.7f);
            Player.transform.position += Position; 
        }
        if (BR.CurrentPlayerChain == 2)
        {
            Position = new Vector3(0, 3.7f);
            Player.transform.position += Position;
        }
        if (BR.CurrentPlayerChain == 3)
        {
            Position = new Vector3(-3.7f, 0);
            Player.transform.position += Position;
        }
        if (BR.CurrentPlayerChain == 4)
        {
            Position = new Vector3(3.7f, 0);
            Player.transform.position += Position;
        }

        BossBar.ShowBossBar();
        BR.DisableAllTriggers();

    }
}
