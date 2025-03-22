using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossRoomUI : MonoBehaviour
{
    public GameObject Container;
    public TextMeshProUGUI BossDifficulty;
    public TextMeshProUGUI BossText;
    public Image BossImage;
    private Animator anim;
    private BossScript Boss;


    private void Awake()
    {
        Container.SetActive(false);
        anim = GetComponentInChildren<Animator>();
    }

    public void OpenUI()
    {
        Boss = FindObjectOfType<BossScript>();
        BossDifficulty.text = "x" + Boss.BossDifficulty;
        BossText.text = "" + Boss.BossName;
        BossImage.sprite = Boss.BossSprites[Boss.BossId];
        Container.SetActive(true);
    }
    public void CloseUI()
    {
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("Loop", false);
        Container.SetActive(false);
    }

    public void ActivateLoop()
    {
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("Loop", true);
    }
}
