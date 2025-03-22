using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCameraIntegration : MonoBehaviour
{
    private PlayerStats Stats;
    private PlayerState State;
    private DynamicCamera dynamicCamera;

    private bool wasRolling = false;
    private bool wasDead = false;
    private float lastHealth = 0;

    void Start()
    {
        Stats = GetComponent<PlayerStats>();
        State = GetComponent<PlayerState>();
        dynamicCamera = Camera.main.GetComponent<DynamicCamera>();

        if (State == null)
        {
            Debug.LogError("PlayerStateCameraIntegration: No PlayerState component found!");
            enabled = false;
            return;
        }

        if (dynamicCamera == null)
        {
            Debug.LogError("PlayerStateCameraIntegration: No DynamicCamera component found on main camera!");
            enabled = false;
            return;
        }

        lastHealth = Stats.CurrentHealth;
    }

    void Update()
    {
        if (Stats.CurrentHealth < lastHealth)
        {
            dynamicCamera.OnPlayerDamaged();
            lastHealth = Stats.CurrentHealth;
        }

        // Check for perfect dodge
        if (State.Rolling && !wasRolling)
        {
            wasRolling = true;
            bool isPerfectDodge = (Time.time - State.LastEnemyAttackTime) <= State.PerfectDodgeWindow;
            if (isPerfectDodge)
            {
                dynamicCamera.OnPerfectDodge();
            }
        }
        else if (!State.Rolling && wasRolling)
        {
            wasRolling = false;
        }

        if (State.Dead && !wasDead)
        {
            wasDead = true;
        }
        else if (!State.Dead && wasDead)
        {
            wasDead = false;
            lastHealth = Stats.CurrentHealth;
        }
    }
}