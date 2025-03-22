using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SpecialPlayerStats : MonoBehaviour
{
    private Dictionary<string, bool> specialStats = new Dictionary<string, bool>();

    //Burn
    public int BurnPercentage = 0;
    public int BurnDamageFactor = 1;
    public int BurnLongevityFactor = 1;
    public int BurnSpeedFactor = 1;

    //Poison
    public int PoisonPercentage = 0;
    public int PoisonDamageFactor = 1;
    public int PoisonLongevityFactor = 1;
    public int PoisonSpeedFactor = 1;

    //Burst Speed
    public int BurstSpeedFactor = 1;
    public int BurstSpeedLongevityFactor = 1;

    #region Logic
    void Start()
    {
        specialStats["CanBurn"] = false;
        specialStats["CanPoison"] = false;
        specialStats["CanPoisonAndBurn"] = false;
        specialStats["CanBurstOfSpeed"] = false;
    }

    public void SetStat(string statName, bool value)
    {
        if (specialStats.ContainsKey(statName))
        {
            specialStats[statName] = value;
        }
        else
        {
            specialStats.Add(statName, value);
        }
    }

    public bool GetStat(string statName)
    {
        if (specialStats.ContainsKey(statName))
        {
            return specialStats[statName];
        }
        else
        {
            Debug.LogWarning("Stat not found: " + statName);
            return false;
        }
    }

    public bool HasAnyEnabledEffects()
    {
        foreach (KeyValuePair<string, bool> stat in specialStats)
        {
            if (stat.Value)
            {
                return true;
            }
        }

        return false;
    }

    public void ResetStats()
    {
        List<string> keys = new List<string>(specialStats.Keys);

        foreach (string key in keys)
        {
            specialStats[key] = false;
        }

        BurnPercentage = 0;
        BurnDamageFactor = 1;
        BurnLongevityFactor = 1;
        BurnSpeedFactor = 1;

        PoisonPercentage = 0;
        PoisonDamageFactor = 1;
        PoisonLongevityFactor = 1;
        PoisonSpeedFactor = 1;

        BurstSpeedFactor = 1;
        BurstSpeedLongevityFactor = 1;


}
    #endregion

    #region AddStats

    public void AddStats(string statName, string statName2, string statName3, string statName4,int increment, int increment2, int increment3, int increment4, string SpecialStatName, bool setSpecialStat)
    {
        AddStat(statName, increment);
        AddStat(statName2, increment2);
        AddStat(statName3, increment3);
        AddStat(statName4, increment4);

        if (setSpecialStat && !GetStat(SpecialStatName))
        {
            SetStat(SpecialStatName, true);
        }
    }

    public void AddStat(string statName, int increment)
    {
        FieldInfo field = this.GetType().GetField(statName, BindingFlags.Public | BindingFlags.Instance);

        if (field != null && field.FieldType == typeof(int))
        {

            int currentValue = (int)field.GetValue(this);
            field.SetValue(this, currentValue + increment);
        }
        else
        {
            Console.WriteLine($"Field '{statName}' not found or is not of type int.");
        }
    }


    #endregion
}
