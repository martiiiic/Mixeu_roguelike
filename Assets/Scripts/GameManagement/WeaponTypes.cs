using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTypes : MonoBehaviour
{
    public Sprite[] WeaponSprites;
    public bool RangedWeapon = false;
    public bool TogglableWeapon;
    public bool MeleeWeapon;
    public string Wrarity;
    public float DamageMultiplier = 1f;
    public float AttackSpeedMultiplier = 1f;
    public float PlayerSpeedMultiplier = 1f;
    public int ArrowsToShoot = 1;
    public float WeaponRange = 1f;

    public int CurrentWeaponId = 0;
    public int SpecialArrows = 0;

    private SpriteRenderer Image;


    private void Start()
    {
        Wrarity = "Common";
        CurrentWeaponId = 0;
        SwitchWeapon(0);
    }

    private void Awake()
    {
        Image = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (TogglableWeapon && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            ToggleWeaponType();
        }
    }

    public void SwitchWeapon(int WeaponID)
    {
        DamageMultiplier = 1f;
        AttackSpeedMultiplier = 1f;
        PlayerSpeedMultiplier = 1f; 
        WeaponRange = 1f;
        ArrowsToShoot = 1;
        SpecialArrows = 0;
        TogglableWeapon = false;


        switch (WeaponID)
        {
            case 0: //Knife
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 1: //Bow
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 2: //BlueDagger
                DamageMultiplier = 0.75f;
                AttackSpeedMultiplier = 0.5f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 3: //Stick
                DamageMultiplier = 0.05f;
                AttackSpeedMultiplier = 0.75f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 4: //Scythe
                DamageMultiplier = 1.15f;
                PlayerSpeedMultiplier = 1.25f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 5: //BlueBow
                DamageMultiplier = 1.25f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 6: //GreenBow
                DamageMultiplier = 1.75f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 7: //CrossBow
                DamageMultiplier = 1.85f;
                AttackSpeedMultiplier = 2.35f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 8: //A.M. Bow
                SpecialArrows = 1;
                DamageMultiplier = 2f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 9: //Dagger
                DamageMultiplier = 0.59f;
                AttackSpeedMultiplier = 0.53f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 10: //Gold Dagger
                DamageMultiplier = 0.87f;
                AttackSpeedMultiplier = 0.49f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 11: //Anti Matter Crossbow
                SpecialArrows = 1;
                DamageMultiplier = 3.85f;
                AttackSpeedMultiplier = 2.15f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 12: //Dildo
                DamageMultiplier = 3f;
                AttackSpeedMultiplier = 1f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 13: //BlackDildo
                DamageMultiplier = 5f;
                AttackSpeedMultiplier = 1.5f;
                PlayerSpeedMultiplier = 0.8f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 14: //AM Sword
                DamageMultiplier = 2f;
                AttackSpeedMultiplier = 1f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 15: //Blackbow
                DamageMultiplier = 1.5f;
                AttackSpeedMultiplier = 0.9f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 16: //BlueCrossbow
                DamageMultiplier = 2.75f;
                AttackSpeedMultiplier = 2.25f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 17: //OldKnife
                DamageMultiplier = 1.15f;
                AttackSpeedMultiplier = 1.1f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 18: //Oldbow
                DamageMultiplier = 0.8f;
                AttackSpeedMultiplier = 0.8f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 19: //Shuriken
                SpecialArrows = 2;
                DamageMultiplier = 0.8f;
                AttackSpeedMultiplier = 0.8f;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 20: //WoodenFork
                DamageMultiplier = 0.9f;
                AttackSpeedMultiplier = 1.2f;
                WeaponRange = 1.25f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 21: //BlueSword
                DamageMultiplier = 1.25f;
                AttackSpeedMultiplier = 1f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 22: //TripleCrossbow
                DamageMultiplier = 1.75f;
                AttackSpeedMultiplier = 2.85f;
                ArrowsToShoot = 3;
                MeleeWeapon = false;
                RangedWeapon = true;
                break;
            case 23: //Greatsword
                DamageMultiplier = 2f;
                AttackSpeedMultiplier = 3.5f;
                PlayerSpeedMultiplier = 0.7f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 24: //Steelfork
                DamageMultiplier = 1.25f;
                AttackSpeedMultiplier = 1.2f;
                WeaponRange = 1.25f;
                MeleeWeapon = true;
                RangedWeapon = false;
                break;
            case 25: //Trident
                SpecialArrows = 3;
                DamageMultiplier = 1.25f;
                AttackSpeedMultiplier = 1.2f;
                WeaponRange = 1.05f;
                MeleeWeapon = true;
                RangedWeapon = false;
                TogglableWeapon = true;
                break;
        }

        UpdateBools();
        Image.sprite = WeaponSprites[WeaponID];
        DungeonInfo DI = FindObjectOfType<DungeonInfo>();
        if (DI != null)
        {
            DI.UpdateRunInfo();
        }
    }

    private void ToggleWeaponType()
    {
        if (TogglableWeapon)
        {
            if (MeleeWeapon)
            {
                MeleeWeapon = false;
                RangedWeapon = true;
            }
            else if (RangedWeapon)
            {
                RangedWeapon = false;
                MeleeWeapon = true;
            }

            UpdateBools();
        }
    }

    public void UpdateRarityBoost()
    {
        if (Wrarity == "Uncommon")
        {
            DamageMultiplier += 0.1f;
            AttackSpeedMultiplier -= 0.1f;
        }
        else if (Wrarity == "Rare")
        {
            DamageMultiplier += 0.125f;
            AttackSpeedMultiplier -= 0.125f;
        }
        else if (Wrarity == "Epic")
        {
            DamageMultiplier += 0.175f;
            AttackSpeedMultiplier -= 0.175f;
        }
        else if (Wrarity == "Legendary")
        {
            DamageMultiplier += 0.25f;
            AttackSpeedMultiplier -= 0.25f;
        }
    }

    private void UpdateBools()
    {
        if (MeleeWeapon)
        {
            RangedWeapon = false;
        }
        else if (RangedWeapon)
        {
            MeleeWeapon = false;
        }
    }
}
