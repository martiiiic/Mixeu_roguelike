using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCollectible : MonoBehaviour
{
    private SpriteRenderer Image;
    private PowerUpStoreUI UI;
    private WeaponTypes Weapons;
    private GameManager Manager;
    public int Id;
    public string PreviousWeaponRarity;
    public int PreviousWeaponId;
    public int GameDifficulty;
    public int WeaponRarity;
    public string WeaponAdditionalRarity;
    private bool Special;
    public Color32 RarityColor;

    public GameObject Weapon;

    public string WeaponName;
    public string WeaponDescription;

    private AudioSource Sound;
    public AudioClip RankUpSound;
    public AudioClip LegendarySound;

    public void SetWeapon(int ID, bool Bool)
    {
        WeaponTypes WT = FindObjectOfType<WeaponTypes>();
        CalculateGameDifficulty();
        Special = false;
        Id = ID;

        WeaponAdditionalRarity = "Common";
        Image.sprite = Weapons.WeaponSprites[ID];
        WeaponRarity = 0;

        if (Bool)
        {
            StopAllCoroutines();
            WeaponAdditionalRarity = PreviousWeaponRarity;
            ApplyOutlineBasedOnRarity();
        }

        string prevWeaponName = WeaponName;

        switch (ID)
        {
            case 0: // Knife
                WeaponRarity = -1;
                WeaponName = "Knife";
                WeaponDescription = "Stabs.";
                break;
            case 1: // Bow
                WeaponRarity = -1;
                WeaponName = "Bow";
                WeaponDescription = "Shoots arrows, I think...";
                break;
            case 2: // BlueKnife
                WeaponRarity = 4;
                WeaponName = "Blue Dagger";
                WeaponDescription = "Stabs Faster. Deals less damage.";
                break;
            case 3: // Stick
                WeaponRarity = 0;
                WeaponName = "Stick";
                WeaponDescription = "Stick gaming.";
                break;
            case 4: // Scythe
                WeaponRarity = 6;
                WeaponName = "Scythe";
                WeaponDescription = "Makes you go faster for no reason.";
                break;
            case 5: // BlueBow
                WeaponRarity = 4;
                WeaponName = "Azure Bow";
                WeaponDescription = "Just a better bow in terms of damage.";
                break;
            case 6: // GreenBow
                WeaponRarity = 10;
                WeaponName = "Verdant Bow";
                WeaponDescription = "Even better at hurting things.";
                break;
            case 7: // CrossBow
                WeaponRarity = 1;
                WeaponName = "Crossbow";
                WeaponDescription = "Shoots slower but harder IKYK.";
                break;
            case 8: // A.M. Bow
                WeaponRarity = 17;
                WeaponName = "A.M. Bow";
                WeaponDescription = "I'm probably one of the best bows.";
                break;
            case 9: // Dagger
                WeaponRarity = 0;
                WeaponName = "Dagger";
                WeaponDescription = "Turbo stabbing, not efficient.";
                break;
            case 10: // Gold Dagger
                WeaponRarity = 6;
                WeaponName = "Gold Dagger";
                WeaponDescription = "Turbo stabbing, more or less efficient.";
                break;
            case 11: // Anti Matter CrossBow
                WeaponRarity = 17;
                WeaponName = "A.M. Crossbow";
                WeaponDescription = "It's made out of anti-matter!";
                break;
            case 12: // Dildo
                WeaponRarity = 40;
                WeaponName = "Strange artifact";
                WeaponDescription = "Is it a sword?";
                break;
            case 13: // Black Dildo
                WeaponRarity = 45;
                WeaponName = "Strange black artifact";
                WeaponDescription = "How big";
                break;
            case 14: // AM sword
                WeaponRarity = 17;
                WeaponName = "A.M. Sword";
                WeaponDescription = "Good.";
                break;
            case 15: // BlackBow
                WeaponRarity = 10;
                WeaponName = "Black bow";
                WeaponDescription = "Strange bow.. efficent!";
                break;
            case 16: // BlueCrossBow
                WeaponRarity = 4;
                WeaponName = "Azure crossbow";
                WeaponDescription = "Its blue! and slow.. but strong!";
                break;
            case 17: // OldKnife
                WeaponRarity = 0;
                WeaponName = "Old knife";
                WeaponDescription = "Believe it or not, its blade is sharper";
                break;
            case 18: // Oldbow
                WeaponRarity = 0;
                WeaponName = "Old bow";
                WeaponDescription = "its string is tangled.. faster to shoot though!";
                break;
            case 19: // Shuriken
                WeaponRarity = 4;
                WeaponName = "Shuriken";
                WeaponDescription = "Ching Chong.. i swear im not racist";
                break;
            case 20: // Wooden Fork
                WeaponRarity = 4;
                WeaponName = "Wooden Fork";
                WeaponDescription = "Start the revolution!";
                break;
            case 21: // Wooden Fork
                WeaponRarity = 4;
                WeaponName = "Azure sword";
                WeaponDescription = "This one hurts!";
                break;
            case 22: //TripleCrossbow
                WeaponRarity = 6;
                WeaponName = "Triple Crossbow";
                WeaponDescription = "Shoots three arrows you moron.";
                break;
            case 23: //GreatSword
                WeaponRarity = 6;
                WeaponName = "Greatsword";
                WeaponDescription = "Heavy isn't it?";
                break;
            case 24: //SteelFork
                WeaponRarity = 6;
                WeaponName = "Steel Fork";
                WeaponDescription = "Quite large range!";
                break;
            case 25:
                WeaponRarity = 6;
                WeaponName = "Trident";
                WeaponDescription = "Atlantis? Press <ALT> to change modes!";
                break;
        }

        if(Bool) { return; }

        int maxRetries = 5;
        int attempts = 0;

        while (WeaponRarity == -1 && attempts < maxRetries)
        {
            int newWeaponID = Random.Range(0, Weapons.WeaponSprites.Length);
            SetWeapon(newWeaponID, false);
            attempts++;
            return;
        }

        if (WeaponRarity == -1)
        {
            return;
        }

        else if (WeaponRarity > GameDifficulty)
        {
            SetWeapon(Random.Range(0, Weapons.WeaponSprites.Length), false);
            return;
        }
        else if (GameDifficulty - WeaponRarity >= 25 + (Manager.DungeonNumber * 0.35f))
        {
            SetWeapon(Random.Range(0, Weapons.WeaponSprites.Length), false);
            return;
        }

        if (!Bool)
        {
            int RarityChoose = Random.Range(-13, 25);
            if (RarityChoose >= 14 && RarityChoose < 19)
            {
                WeaponAdditionalRarity = "Uncommon";
            }
            else if (RarityChoose >= 19 && RarityChoose < 22)
            {
                WeaponAdditionalRarity = "Rare";
            }
            else if (RarityChoose >= 22 && RarityChoose != 24)
            {
                WeaponAdditionalRarity = "Epic";
            }
            else if (RarityChoose == 24)
            {
                WeaponAdditionalRarity = "Legendary";
            }
            else
            {
                WeaponAdditionalRarity = "Common";
            }
        }

        if (!Bool && WeaponAdditionalRarity != "Common")
        {
            StartCoroutine(PlayRarityRankUpAnimation(WeaponAdditionalRarity));
        }
    }

    public GameObject OutlineObject;
    public GameObject OutlineParent;
    public Material OutlineMaterial;

    public void ApplyOutlineBasedOnRarity()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (OutlineParent == null || OutlineMaterial == null) return;

        if (OutlineObject == null)
        {
            WeaponTypes WT = FindObjectOfType<WeaponTypes>();
            OutlineObject = new GameObject("OutlineObject");
            OutlineObject.transform.SetParent(OutlineParent.transform);
            OutlineObject.transform.localPosition = new Vector3(0.01f, 0.01f,0);
            OutlineObject.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            OutlineObject.transform.rotation = new Quaternion(0f,0f,0f,0f);

            SpriteRenderer outlineRenderer = OutlineObject.AddComponent<SpriteRenderer>();
            if (outlineRenderer != null && WT != null) 
            {
                outlineRenderer.sprite = WT.WeaponSprites[Id];
                outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
                outlineRenderer.material = OutlineMaterial;
            }
            
        }

        SpriteRenderer outlineRenderer_ = OutlineObject.GetComponent<SpriteRenderer>();
        outlineRenderer_.sortingOrder = spriteRenderer.sortingOrder - 1;
        outlineRenderer_.sprite = Weapons.WeaponSprites[Id];
        outlineRenderer_.material = OutlineMaterial;

        switch (WeaponAdditionalRarity)
        {
            case "Uncommon": outlineRenderer_.color = new Color(0f, 1f, 0f, .35f); break;
            case "Rare": outlineRenderer_.color = new Color(0f, 0.5f, 1f, .35f); break;
            case "Epic": outlineRenderer_.color = new Color(1f, 0f, 1f, .35f); break;
            case "Legendary": outlineRenderer_.color = new Color(1f, 1f, 0f, .35f); break;
            default: outlineRenderer_.color = new Color(0f, 0f, 0f, 0f); break;
        }
        RarityColor = outlineRenderer_.color;
    }

    public IEnumerator PlayRarityRankUpAnimation(string targetRarity)
    {
        string[] rarityOrder = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        int currentRarityIndex = System.Array.IndexOf(rarityOrder, "Common");
        int targetRarityIndex = System.Array.IndexOf(rarityOrder, targetRarity);

        if (currentRarityIndex == -1 || targetRarityIndex == -1)
        {
            yield break;
        }

        while (currentRarityIndex < targetRarityIndex)
        {
            yield return new WaitForSeconds(Random.Range(0.1f + (currentRarityIndex * 0.05f), 0.5f + (currentRarityIndex * 0.1f)));

            Sound.pitch = 1.0f + (currentRarityIndex * 0.3f);
            currentRarityIndex++;
            WeaponAdditionalRarity = rarityOrder[currentRarityIndex];
            ApplyOutlineBasedOnRarity();

            Sound.volume = 1f;
            Sound.PlayOneShot(RankUpSound);
        }

        if (targetRarity == "Legendary")
        {
            Sound.Stop();
            AudioSource Sound2 = gameObject.AddComponent<AudioSource>();
            Sound2.pitch = 2f;
            Sound2.PlayOneShot(RankUpSound);
            Sound.pitch = 1f;
            Sound.volume = 1f;
            Sound.PlayOneShot(LegendarySound);
        }

        yield return new WaitForSeconds(1.25f);
        Sound.pitch = 1.0f;
        Sound.volume = 0;
    }

    private void Awake()
    {
        StopAllCoroutines();
        Sound = gameObject.AddComponent<AudioSource>();
        Manager = FindObjectOfType<GameManager>();
        UI = FindAnyObjectByType<PowerUpStoreUI>();
        Image = GetComponentInChildren<SpriteRenderer>();
        Weapons = FindObjectOfType<WeaponTypes>();
        Sound.playOnAwake = false;
        if(Weapons != null) { SetWeapon(Random.Range(0, Weapons.WeaponSprites.Length), false); }
    }

    private void CalculateGameDifficulty()
    {
        GameDifficulty = Manager.enemyDamageMultiplier + Manager.enemyHealthMultiplier;
    }

    public void SpawnWeapon(Transform Position)
    {
        Vector3 Spawnpoint;
        Spawnpoint = Position.transform.position;

        GameObject Weapon_ = Instantiate(Weapon, Spawnpoint, Quaternion.identity);
    }

    public void LeavePreviousWeapon(Transform Position, int id)
    {
        RoomTemplates RT = FindObjectOfType<RoomTemplates>();
        Vector3 spawnPoint = Position.position;
        GameObject newWeapon = Instantiate(Weapon, spawnPoint, Quaternion.identity);
        WeaponCollectible previousWeapon = newWeapon.GetComponent<WeaponCollectible>();
        newWeapon.name = "WeaponCollectible_";
        RT.WC_ = newWeapon;

        if (previousWeapon != null)
        {
            previousWeapon.SetWeapon(id, true);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            UI = FindAnyObjectByType<PowerUpStoreUI>();
            UI.ShowWeaponMenu(Id);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UI = FindAnyObjectByType<PowerUpStoreUI>();
            UI.CloseMenu();
        }
    }
}

