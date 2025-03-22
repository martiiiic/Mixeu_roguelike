using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int MaxHealth;
    public int CurrentHealth;
    public float Speed;
    public int AttackDamage;
    public float AttackSpeed;
    public float RangedSpeed;
    public float Defense;
    public float DashSpeed;

    private void Start()
    {
        ResetStats();
        CurrentHealth = MaxHealth;
    }

    private void Update()
    {
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        else if (AttackSpeed > 4.3f )
        {
            AttackSpeed = 4.3f;
            int rand = Random.Range(0, 2);
            if (rand == 1)
            {
                CurrentHealth += 1;
            }
        }
        else if(Defense < 0)
        {
            Defense = 0;    
        }

        else if(AttackDamage < 1)
        {
            AttackDamage = 1;
        }

        else if(AttackSpeed < 0.25f)
        {
            AttackSpeed = 0.25f;
        }

        else if (Speed > 13.5f)
        {
            Speed = 13.5f;
            int rand = Random.Range(0, 2);
            if (rand == 1)
            {
                CurrentHealth += 1;
            }
        }
        else if (Speed < 3f)
        {
            Speed = 3f;
        }
    }

    public void ResetStats()
    {
        MaxHealth = 3;
        CurrentHealth = 3;
        Speed = 5f;
        AttackDamage = 5;
        AttackSpeed = 1.5f;
        RangedSpeed = 0.75f;
        Defense = 0;
        DashSpeed = 10f;
    }

    public void AddStats(string Stat,string Stat2, int Increment, int Increment2)
    {
        switch (Stat)
        {
            case "MaxHealth":
                MaxHealth += Increment;
                break;
            case "CurrentHealth":
                CurrentHealth += Increment;
                break;
            case "Speed":
                Speed += (Increment - 0.5f);
                break;
            case "AttackSpeed":
                AttackSpeed += (Increment - 0.8f);
                break;
            case "RangedSpeed":
                RangedSpeed += (Increment - 0.75f);
                break;
            case "AttackDamage":
                GameManager Manager = FindObjectOfType<GameManager>();
                AttackDamage += (Increment + Mathf.RoundToInt(0.25f * Manager.enemyDamageMultiplier));
                break;
            case "Defense":
                Defense += Increment;
                break;
            case "DashSpeed":
                DashSpeed += (Increment - 0.75f);
                break;
        }

        if(Stat2 != null)
        {
            switch (Stat2)
            {
                case "MaxHealth":
                    MaxHealth += Increment2;
                    break;
                case "CurrentHealth":
                    CurrentHealth += Increment2;
                    break;
                case "Speed":
                    Speed += (Increment2 * 0.5f);
                    break;
                case "AttackSpeed":
                    AttackSpeed += (Increment2 * 0.8f);
                    break;
                case "RangedSpeed":
                    RangedSpeed += (Increment2 * 0.75f);
                    break;
                case "AttackDamage":
                    AttackDamage += Increment2;
                    break;
                case "Defense":
                    Defense += Increment2;
                    break;
                case "DashSpeed":
                    DashSpeed += (Increment2 * 0.75f);
                    break;
            }
            Stat2 = null;
        }

        DungeonInfo DI = FindObjectOfType<DungeonInfo>();
        if (DI != null) { DI.UpdateRunInfo(); }
        
    }      
}
