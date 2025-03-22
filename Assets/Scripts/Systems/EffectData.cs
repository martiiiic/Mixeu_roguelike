using UnityEngine;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Effects/Effect")]
public class EffectData : ScriptableObject
{
    public string effectName;
    public BuffsAndDebuffs.EffectType type;
    public float duration;
    public float attackModifier;
    public float healthModifier;
    public float tickDamage;
    public float tickInterval;
    public float speedModifier;  
    public float blockModifier;    
   
    public float criticalHitModifier; 
    public float defenseModifier;  
    public float cooldownReduction; 
    public float regenerationRate; 
    public float dodgeModifier;         
    public float attackSpeedModifier;

    public bool oneUseOnly;
    public bool rechargeUsesOverTime;
    public float rechargeTimeInterval;
    public float rechargeAmount;          
    public bool preventUseExhaustion;
    public int maxUses;
    public int usesRemaining;

    public float damageReductionPercentage;

    public void RechargeUses(float deltaTime)
    {
        if (rechargeUsesOverTime && usesRemaining < maxUses)
        {
            if (deltaTime >= rechargeTimeInterval)
            {
                usesRemaining = Mathf.Min(usesRemaining + (int)rechargeAmount, maxUses);
            }
        }

        if (preventUseExhaustion && usesRemaining <= 0)
        {
            usesRemaining = 1;
        }
    }
}

