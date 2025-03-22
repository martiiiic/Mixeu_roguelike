using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpStoreUI : MonoBehaviour
{
    public TextMeshProUGUI powerUpName;
    public TextMeshProUGUI powerUpDescription;
    public TextMeshProUGUI powerUpPrice;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI buyText;

    public Image coinIcon;
    public Sprite coinSprite;
    public Image powerUpImage;

    public GameObject[] betButtons;
    public GameObject[] menuObjects;

    [HideInInspector] public PowerUpStore Store;

    private bool isPowerUpBuy;
    private bool isSlotMachineBuy;
    private bool isWeaponBuy;
    private bool isCauldron;
    private bool isPortal;

    private SpecialPlayerStats specialStats;
    private PlayerStats playerStats;
    private Animator animator;
    private CoinSystem coinSystem;

    private SlotMachine slotMachine;
    private WeaponCollectible weaponCollectible;
    private WeaponTypes weaponTypes;
    private Cauldron cauldron;
    private PortalTeleporter portal;

    private int portalId;
    private RoomTemplates rooms;

    public GameObject playerPoint;

    private VoidBoundEvents voidBinding;

    public AudioSource purchaseSound;

    private void Start()
    {
        voidBinding = FindObjectOfType<VoidBoundEvents>();
        weaponTypes = FindObjectOfType<WeaponTypes>();
        weaponCollectible = FindObjectOfType<WeaponCollectible>();
        specialStats = FindObjectOfType<SpecialPlayerStats>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        coinSystem = FindAnyObjectByType<CoinSystem>();
        animator = GetComponent<Animator>();
    }

    public void ShowMenuSlotMachine(int Price)
    {
        foreach (GameObject obj in betButtons)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        buyText.gameObject.SetActive(true);
        buyText.text = "Bet";
        slotMachine = FindFirstObjectByType<SlotMachine>();
        powerUpImage.sprite = slotMachine.UISprite;
        powerUpName.text = "Slot Machine";
        powerUpDescription.text = "You can ONLY lose 100% but win 2000%!";
        powerUpPrice.text = "x" + Price;
        animator.SetBool("Show", true);
        isPowerUpBuy = false;
        isSlotMachineBuy = true;
        isWeaponBuy = false;
        isCauldron = false;
        coinIcon.sprite = coinSprite;
        Color imageColor = coinIcon.color;
        imageColor.a = 1f;
        coinIcon.color = imageColor;
        rarityText.enabled = false;
    }

    public void ShowCauldronMenu()
    {
        HealthSystem healthSystem = FindFirstObjectByType<HealthSystem>();
        foreach (GameObject obj in betButtons)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        buyText.gameObject.SetActive(true);
        buyText.text = "Damage";
        cauldron = FindFirstObjectByType<Cauldron>();
        powerUpImage.sprite = cauldron.Sprite.sprite;
        powerUpName.text = "Blood Offering";
        powerUpDescription.text = "Give out some max health for a price!";

        if (playerStats.MaxHealth > 1)
        {
            powerUpPrice.text = " x1";
        }
        else powerUpPrice.text = " Unable";

        animator.SetBool("Show", true);
        isPowerUpBuy = false;
        isSlotMachineBuy = false;
        isWeaponBuy = false;
        isCauldron = true;
        isPortal = false;
        Color imageColor = coinIcon.color;
        imageColor.a = 1f;
        coinIcon.color = imageColor;
        coinIcon.sprite = healthSystem.FishSprites[0];
        rarityText.enabled = false;
    }

    public void ShowPortalMenu(PortalTeleporter portalTeleporter = null, string portalName = "", string portalDescription = "", int portalID = 0, Sprite portalSprite = null)
    {
        portal = portalTeleporter;
        HealthSystem healthSystem = FindFirstObjectByType<HealthSystem>();
        foreach (GameObject obj in betButtons)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        powerUpPrice.text = "";

        rooms = FindObjectOfType<RoomTemplates>();
        portalId = portalID;
        buyText.gameObject.SetActive(true);
        buyText.text = "Enter";
        powerUpImage.sprite = portalSprite;
        powerUpName.text = portalName;
        powerUpDescription.text = portalDescription;

        animator.SetBool("Show", true);
        isPowerUpBuy = false;
        isSlotMachineBuy = false;
        isWeaponBuy = false;
        isCauldron = false;
        isPortal = true;
        Color imageColor = coinIcon.color;
        imageColor.a = 0f;
        coinIcon.color = imageColor;
        rarityText.enabled = false;
    }

    public void ShowWeaponMenu(int id)
    {
        weaponTypes = FindObjectOfType<WeaponTypes>();
        weaponCollectible = FindObjectOfType<WeaponCollectible>();
        buyText.gameObject.SetActive(true);
        buyText.text = "Equip";
        rarityText.enabled = true;
        rarityText.text = weaponCollectible.WeaponAdditionalRarity;
        rarityText.color = weaponCollectible.RarityColor;
        powerUpImage.sprite = weaponTypes.WeaponSprites[id];
        powerUpName.text = weaponCollectible.WeaponName;
        powerUpDescription.text = weaponCollectible.WeaponDescription;
        powerUpPrice.text = "";
        animator.SetBool("Show", true);
        isPowerUpBuy = false;
        isSlotMachineBuy = false;
        isWeaponBuy = true;
        isCauldron = false;
        isPortal = false;
        Color imageColor = coinIcon.color;
        imageColor.a = 0f;
        coinIcon.color = imageColor;
    }

    public void ShowMenu(int id)
    {
        if (Store == null || Store.powerUpDefinition == null) return;

        rarityText.enabled = false;
        buyText.gameObject.SetActive(true);
        buyText.text = "Buy";

        powerUpImage.sprite = Store.spriteRenderer.sprite;
        powerUpName.text = Store.powerUpDefinition.displayName;
        powerUpDescription.text = Store.powerUpDefinition.description;
        powerUpPrice.text = "x" + Store.calculatedPrice.ToString();

        animator.SetBool("Show", true);
        isPowerUpBuy = true;
        isSlotMachineBuy = false;
        isWeaponBuy = false;
        isCauldron = false;
        isPortal = false;

        Color imageColor = coinIcon.color;
        imageColor.a = 1f;
        coinIcon.color = imageColor;
        coinIcon.sprite = coinSprite;
    }

    public void CloseMenu()
    {
        animator.SetBool("Show", false);

        foreach (GameObject obj in betButtons)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    public void IncrementBet()
    {
        slotMachine.Price += Mathf.RoundToInt(0.1f * coinSystem.Coins);
        powerUpPrice.text = "x" + slotMachine.Price.ToString();
        if (slotMachine.Price >= coinSystem.Coins)
        {
            slotMachine.Price = coinSystem.Coins;
            powerUpPrice.text = "x" + slotMachine.Price.ToString();
        }
    }

    public void SubtractBet()
    {
        slotMachine.Price -= Mathf.RoundToInt(0.1f * coinSystem.Coins);
        powerUpPrice.text = "x" + slotMachine.Price.ToString();
        if (slotMachine.Price <= 0)
        {
            slotMachine.Price = 0;
            powerUpPrice.text = "x0";
        }
    }

    public void BuyItem()
    {
        if (isPowerUpBuy)
        {
            if (coinSystem.Coins < Store.calculatedPrice)
            {
                return;
            }

            coinSystem.Coins -= Store.calculatedPrice;

            if (Store.powerUpDefinition.statModifiers.Count > 0 &&
                Store.powerUpDefinition.statModifiers[0].statToModify == PowerUpDefinition.StatModifier.StatType.MaxHealth &&
                voidBinding.IsEffectActive("Kidney"))
            {
                Destroy(Store.gameObject);
                return;
            }

            Store.ApplyPowerUp(playerStats, specialStats);

            foreach (GameObject objectDisplay in menuObjects)
            {
                objectDisplay.GetComponent<ObjectStatsMenu>().AddPowerUp(0, Store);
            }

            if (purchaseSound != null)
            {
                purchaseSound.Play();
            }

            Destroy(Store.gameObject);
        }

        if (isSlotMachineBuy && slotMachine.Price != 0)
        {
            if (coinSystem.Coins < slotMachine.Price)
            {
                return;
            }
            else
            {
                coinSystem.Coins -= slotMachine.Price;
                slotMachine.StartGambling();
            }
        }

        if (isWeaponBuy)
        {
            weaponTypes = FindObjectOfType<WeaponTypes>();
            weaponCollectible = FindObjectOfType<WeaponCollectible>();
            DungeonInfo dungeonInfo = FindAnyObjectByType<DungeonInfo>();

            string oldWeaponRarity = weaponTypes.Wrarity;
            int oldWeaponId = weaponTypes.CurrentWeaponId;

            weaponTypes.CurrentWeaponId = weaponCollectible.Id;
            weaponTypes.Wrarity = weaponCollectible.WeaponAdditionalRarity;

            weaponCollectible.PreviousWeaponId = oldWeaponId;
            weaponCollectible.PreviousWeaponRarity = oldWeaponRarity;

            weaponTypes.SwitchWeapon(weaponTypes.CurrentWeaponId);

            weaponCollectible.LeavePreviousWeapon(weaponCollectible.transform, oldWeaponId);

            weaponTypes.UpdateRarityBoost();

            if (dungeonInfo != null) { dungeonInfo.UpdateRunInfo(); }

            Destroy(weaponCollectible.gameObject);
        }

        if (isCauldron)
        {
            cauldron = FindAnyObjectByType<Cauldron>();
            DungeonInfo dungeonInfo = FindAnyObjectByType<DungeonInfo>();

            if (playerStats.MaxHealth <= 1) { return; }
            else
            {
                PlayerState playerState = FindFirstObjectByType<PlayerState>();
                playerState.TakeDamage(0);
                playerStats.MaxHealth -= 1;
                cauldron.SpawnPowerUp();

                foreach (GameObject objectDisplay in menuObjects)
                {
                    objectDisplay.GetComponent<ObjectStatsMenu>().SubtractPowerUp(0);
                }
            }
        }

        if (isPortal)
        {
            PortalTeleporter portalTeleporter = portal;
            PlayerState playerState = FindObjectOfType<PlayerState>();
            Music music = FindObjectOfType<Music>();

            switch (portalId)
            {
                case 0:
                    if (FindObjectOfType<ExitHand>() == null)
                    {
                        portalTeleporter.anim.SetBool("EnterPortal", true);
                        StartCoroutine(DeleteRoomsCoroutine(false));
                        Instantiate(playerPoint, new Vector2(1500, -1500), Quaternion.identity);
                    }
                    else
                    {
                        FindObjectOfType<ExitHand>().ExitDungeon();
                    }
                    break;

                case 1:
                    music.PlayMusic(portalTeleporter.PortalMusic[portalId - 1], music.musicSource, 3f);
                    StartCoroutine(DeleteRoomsCoroutine(false, true, 0));
                    portalTeleporter.anim.SetBool("EnterPortal", true);
                    Instantiate(playerPoint, new Vector2(1500, -1500), Quaternion.identity);
                    break;
            }
            CloseMenu();
            playerState.gameObject.SetActive(false);
            isPortal = false;
        }
    }

    IEnumerator DeleteRoomsCoroutine(bool param1, bool param2 = false, int param3 = 0)
    {
        yield return new WaitForSeconds(2.5f);
        rooms.DeleteAllRooms(param1, param2, param3);
    }

    private void Update()
    {
        if (animator.GetBool("Show") && Input.GetKeyDown(KeyCode.E))
        {
            BuyItem();
        }
    }

    private bool _disposed = false;

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAllCoroutines();
            specialStats = null;
            playerStats = null;
            animator = null;
            coinSystem = null;
            Store = null;
            slotMachine = null;
            weaponCollectible = null;
            weaponTypes = null;
            rooms = null;
            portal = null;
            voidBinding = null;
            betButtons = null;
            menuObjects = null;
            _disposed = true;
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }
}