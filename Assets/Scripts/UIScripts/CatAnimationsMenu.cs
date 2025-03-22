using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CatAnimationsMenu : MonoBehaviour
{
    private float timer;

    public event Action<float> OnTimeUpdated;

    private int rand;

    private Animator anim;

    private bool Performing;

    public bool[] Actions;


    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        OnTimeUpdated?.Invoke(timer);

        if (timer >= 10)
        {
            timer = 0;
            rand = UnityEngine.Random.Range(1, 10);
            Debug.Log("TimerReset");
        }
        if (!Performing)
        {
            //Sleep
            if (rand == 7)
            {
                anim = GetComponent<Animator>();
                anim.SetBool("Sleep", true);
                Actions[0] = true;
                Performing = true;
            }
        }
        else if (Performing)
        {
            if (rand >= 1 && rand <= 4)
            {
                rand = 1;
                anim = GetComponent<Animator>();
                Performing = false;
            }
        }

        if (Performing == false)
        {
            if (Actions[0])
            {
                Actions[0] = false;
                SStep2();
            }
        }
    }

    public void StopAllAnimations()
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(param.name, false);
            }
        }
    }

    public void SStep1()
    {
        anim.SetBool("SleepLoop", true);
        anim.SetBool("Sleep", false);

    }
    public void SStep2()
    { 
        anim.SetBool("WakeUp", true);
        anim.SetBool("SleepLoop", false);
        
    }
}
