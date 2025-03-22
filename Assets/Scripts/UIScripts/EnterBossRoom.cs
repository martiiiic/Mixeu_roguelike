using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterBossRoom : MonoBehaviour
{
    public int ChainID;
    public int CurrentPlayerChain;
    private PlayerState Player;
    private BossRoomUI BossUI;

    void OnTriggerEnter2D(Collider2D other)
    {
        BossUI = FindObjectOfType<BossRoomUI>();
        if(other.CompareTag("Player"))
        {
            EnterBossRoom[] X = FindObjectsOfType<EnterBossRoom>();
            foreach(EnterBossRoom X2 in X)
            {
                CurrentPlayerChain = this.ChainID;
            }
            BossUI.OpenUI();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        BossUI = FindObjectOfType<BossRoomUI>();
        if (other.CompareTag("Player"))
        {
            BossUI.CloseUI();
        }
    }

    public void DisableAllTriggers()
    {
        EnterBossRoom[] enterBossRoomObjects = FindObjectsOfType<EnterBossRoom>();
        foreach (EnterBossRoom enterBossRoom in enterBossRoomObjects)
        {
            BoxCollider2D boxCollider = enterBossRoom.GetComponent<BoxCollider2D>();

            if (boxCollider != null && boxCollider.isTrigger)
            {
                boxCollider.enabled = false;
            }
        }
    }
}
