using System;
using System.Collections;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public Transform pivot;
    public float radius = 1f;
    public GameObject weapon;
    private Animator weaponAnimator;
    public float swingCooldown = 2f;
    private BoxCollider2D DamageBox;
    bool Animation = false;
    private PlayerStats Stats;
    public AudioSource Swing;
    public AudioClip[] SwingSFX;
    public AudioClip[] BowSFX;
    public GameObject[] arrowPrefabs;
    private PlayerState Player;

    private WeaponTypes WT;

    private bool isSwinging = false;
    private float lastSwingTime = 0;
    private float lastShootTime = 0;

    public float maxSpeed = 40f;
    public float accelerationTime = 1f;

    private float currentSpeed = 0f;
    private float timeSinceShot = 0f;

    private GameObject arrow;

    // Wall detection parameters
    public LayerMask wallLayer;
    public float minWallDistance = 0.5f;

    private void Start()
    {
        Player = FindFirstObjectByType<PlayerState>();
        Swing = gameObject.AddComponent<AudioSource>();
        Stats = FindFirstObjectByType<PlayerStats>();
        DamageBox = GetComponentInChildren<BoxCollider2D>();
        weaponAnimator = GetComponent<Animator>();

        Swing.volume = 0.3f;

        WT = weapon.GetComponent<WeaponTypes>();
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (Player.Dead)
        {
            return;
        }

        if (!Animation)
        {
            RotateWeaponAroundPivot();
        }

        if (WT.MeleeWeapon)
        {
            if (Input.GetMouseButtonDown(0) && Time.time > lastSwingTime + (swingCooldown / Stats.AttackSpeed * WT.AttackSpeedMultiplier))
            {
                if (!IsWallBetweenWeaponAndPlayer())
                {
                    Attack();
                }
            }
        }
        else if (WT.RangedWeapon)
        {
            if (Input.GetMouseButtonDown(0) && Time.time > lastShootTime + (swingCooldown / Stats.RangedSpeed * WT.AttackSpeedMultiplier))
            {
                if (!IsWallBetweenWeaponAndPlayer())
                {
                    Shoot();
                }
            }
        }

        if (currentSpeed < maxSpeed)
        {
            if (arrow != null)
            {
                timeSinceShot += Time.deltaTime;
                currentSpeed = Mathf.Lerp(Stats.RangedSpeed + 8.5f, maxSpeed, timeSinceShot / accelerationTime);
                Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
                rb.velocity = rb.velocity.normalized * currentSpeed;
            }
        }
    }

    bool IsWallBetweenWeaponAndPlayer()
    {
        Vector3 direction = weapon.transform.position - pivot.position;
        float distance = Vector3.Distance(weapon.transform.position, pivot.position);

        RaycastHit2D hit = Physics2D.Raycast(pivot.position, direction.normalized, distance, wallLayer);

        return hit.collider != null;
    }

    void RotateWeaponAroundPivot()
    {
        radius = WT.WeaponRange;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePosition - pivot.position;
        direction.z = 0;
        Vector3 offset = direction.normalized * radius;
        weapon.transform.position = pivot.position + offset;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weapon.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    public float minDistanceToShoot = 1.5f;

    void Shoot()
    {
        if (WT.MeleeWeapon) { return; }

        Vector3 arrowSpawnPoint = Stats.transform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        float distanceToMouse = Vector3.Distance(arrowSpawnPoint, mousePosition);

        if (distanceToMouse < minDistanceToShoot)
        {
            return;
        }

        timeSinceShot = 0f;
        currentSpeed = Stats.RangedSpeed + 8.5f;

        Swing.clip = BowSFX[UnityEngine.Random.Range(0, BowSFX.Length)];
        Swing.Play();

        lastShootTime = Time.time;

        StartCoroutine(ShootArrowsAsync(arrowSpawnPoint, mousePosition));
    }

    IEnumerator ShootArrowsAsync(Vector3 arrowSpawnPoint, Vector3 mousePosition)
    {
        for (int i = 0; i < WT.ArrowsToShoot; i++)
        {
            float spreadAngle = 0f;

            if (WT.ArrowsToShoot == 3)
            {
                if (i == 0)
                {
                    spreadAngle = -20f;
                }
                else if (i == 1)
                {
                    spreadAngle = 0f;
                }
                else if (i == 2)
                {
                    spreadAngle = 20f;
                }
            }
            ShootArrow(arrowSpawnPoint, mousePosition, spreadAngle);
            yield return new WaitForSeconds(0.005f);
        }
    }

    void ShootArrow(Vector3 arrowSpawnPoint, Vector3 mousePosition, float angleOffset)
    {
        GameObject arrow = Instantiate(arrowPrefabs[0], arrowSpawnPoint, Quaternion.identity);
        Vector3 shootDirection = (mousePosition - arrowSpawnPoint).normalized;

        if (angleOffset != 0f)
        {
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg + angleOffset;
            shootDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        }

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.velocity = shootDirection * (Stats.RangedSpeed + 8.5f);

        arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg);
    }
    public Vector3 stabOffset;

    void Attack()
    {
        if (WT.RangedWeapon) { return; }

        Swing.clip = SwingSFX[UnityEngine.Random.Range(0, SwingSFX.Length)];
        Swing.Play();
        isSwinging = true;
        Animation = true;
        lastSwingTime = Time.time;
        DamageBox.enabled = true;

        stabOffset = weapon.transform.up * radius;
        weapon.transform.position += stabOffset;

        StartCoroutine(ResetWeaponPosition(stabOffset));
        StartCoroutine(EndSwing());
    }

    public System.Collections.IEnumerator ResetWeaponPosition(Vector3 offset)
    {
        yield return new WaitForSeconds(swingCooldown * WT.AttackSpeedMultiplier / (Stats.AttackSpeed));
        DamageBox.enabled = false;
        weapon.transform.position -= offset;
        Animation = false;
    }

    System.Collections.IEnumerator EndSwing()
    {
        yield return new WaitForSeconds(swingCooldown / (WT.RangedWeapon ? Stats.RangedSpeed : Stats.AttackSpeed));
        isSwinging = false;
    }
}