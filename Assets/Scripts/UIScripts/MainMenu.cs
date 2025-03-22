using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Animator anim;

    public GameObject Canvas;

    public GameObject RunMenu_;
    public GameObject Settings_;

    //0 = mainmenu

    public void ToNewRunMenu()
    {

        anim = Canvas.GetComponent<Animator>();
        anim.SetBool("RunMenu", true);
        RunMenu_.SetActive(true);
        Settings_.SetActive(false);
    }

    public void RunMenuToFalse()
    {
        anim = Canvas.GetComponent<Animator>();
        anim.SetBool("RunMenu", false);
    }

    public void RunMenuToMainMenu()
    {
        anim = Canvas.GetComponent<Animator>();
        anim.SetBool("ExitRunMenu", true);
    }

    public void ExitRunMenuToFalse()
    {
        anim = Canvas.GetComponent<Animator>();
        anim.SetBool("ExitRunMenu", false);
    }

    public void ToSettingsMenu()
    {

        anim = Canvas.GetComponent<Animator>();
        anim.SetBool("RunMenu", true);
        RunMenu_.SetActive(false);
        Settings_.SetActive(true);
    }

}
