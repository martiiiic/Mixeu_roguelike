using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpDefinition", menuName = "PowerUps/PowerUpDefinition")]
public class PowerUpDefinition : ScriptableObject
{
    public int id;                   // Unique identifier
    public string displayName;       // Name displayed in the UI
    public string description;       // Description of the power-up
    public Sprite icon;              // Icon for the power-up
    public int basePrice;            // Base price before multipliers
    public bool isVoidBound;         // Whether this is a void bound power-up
    public bool isSpecialPowerUp;    // Whether this has special effects
    public string specialStatName;   // Name of special stat if applicable

    [System.Serializable]
    public class StatModifier
    {
        public enum StatType
        {
            MaxHealth, CurrentHealth, Speed, AttackSpeed, RangedSpeed,
            AttackDamage, Defense, DashSpeed, BurnPercentage, BurnDamageFactor,
            BurnLongevityFactor, BurnSpeedFactor, PoisonPercentage, PoisonDamageFactor,
            PoisonLongevityFactor, PoisonSpeedFactor, BurstSpeedFactor, BurstSpeedLongevityFactor
        }

        public StatType statToModify;
        public float value;
        public bool useRawValue = false;
    }

    public List<StatModifier> statModifiers = new List<StatModifier>();
}