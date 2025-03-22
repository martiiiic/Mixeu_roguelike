using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("References")]
    public PlayerState player;
    private Camera cam;

    [Header("Follow Settings")]
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float followMargin = 0.1f;

    [Header("Shake Settings")]
    public float dashShakeIntensity = 0.2f;
    public float damageShakeIntensity = 0.35f;
    public float perfectDodgeShakeIntensity = 0.25f;
    public float deadShakeIntensity = 0.5f;
    public float shakeDecay = 5f;
    private float currentShake = 0f;

    [Header("Ambient Shake")]
    public bool enableAmbientShake = true;
    public float ambientShakeIntensity = 0.03f;
    public float ambientShakeSpeed = 0.7f;

    [Header("Zoom Settings")]
    public float normalZoom = 5f;
    public float shopZoom = 4.2f;
    public float combatZoom = 4.5f;
    public float dashZoom = 4.8f;
    public float perfectDodgeZoom = 4.6f;
    public float deadZoom = 6f;
    public float largeRoomZoom = 6.5f;  // New zoom level for large rooms
    public float zoomSpeed = 3f;
    private float targetZoom;
    private bool isRoomControllingZoom = false;

    [Header("Dynamic Effects")]
    public float dashTiltAngle = 3f;
    public float tiltRecoverySpeed = 3f;
    private float currentTilt = 0f;
    private float targetTilt = 0f;

    [Header("Room Compatibility")]
    public bool respectRoomTransitions = true;
    private bool roomControllingCamera = false;

    private bool wasRolling = false;
    private bool wasDead = false;
    private Vector2 lastMoveDirection;
    private Vector2 currentMoveDirection;
    private Vector3 lastPlayerPos;
    private float lastPlayerSpeed = 0f;
    private Vector3 originalPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (player == null)
        {
            player = FindObjectOfType<PlayerState>();
        }

        targetZoom = normalZoom;
        cam.orthographicSize = normalZoom;
    }

    private void Start()
    {
        if (player != null)
        {
            lastPlayerPos = player.transform.position;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        originalPosition = transform.position;

        Vector3 currentCamPos = transform.position;
        roomControllingCamera = Vector3.Distance(currentCamPos, transform.position) < 0.01f && respectRoomTransitions;

        if (!roomControllingCamera)
        {
            ApplyMicroMovements();
        }

        if (!isRoomControllingZoom)
        {
            UpdateZoom();
        }
        else
        {
            // Room is controlling zoom, so just interpolate to target
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }

        ApplyAmbientShake();
        UpdateShake();
        UpdateTilt();
    }

    private void ApplyAmbientShake()
    {
        if (!enableAmbientShake) return;

        float time = Time.time * ambientShakeSpeed;
        Vector2 shakeOffset = new Vector2(
            (Mathf.PerlinNoise(time, 0) - 0.5f) * 2f,
            (Mathf.PerlinNoise(0, time) - 0.5f) * 2f
        ) * ambientShakeIntensity;

        transform.position += new Vector3(shakeOffset.x, shakeOffset.y, 0);
    }

    private void ApplyMicroMovements()
    {
        if (Time.timeScale < 0.5f) return;

        Vector3 playerPos = player.transform.position;
        Vector3 playerVelocity = (playerPos - lastPlayerPos) / Time.deltaTime;
        float playerSpeed = playerVelocity.magnitude;
        lastPlayerPos = playerPos;

        Vector3 targetPosition = transform.position;

        if (!player.Rolling)
        {
            currentMoveDirection = new Vector2(player.xInput, player.yInput).normalized;
            if (currentMoveDirection.magnitude > 0.1f)
            {
                lastMoveDirection = currentMoveDirection;
            }
        }

        float moveBias = Mathf.Lerp(0.1f, 0.3f, playerSpeed / 10f);
        Vector3 leadOffset = new Vector3(lastMoveDirection.x * moveBias, lastMoveDirection.y * moveBias, 0);

        if (playerSpeed > 0.1f && !player.Rolling)
        {
            targetPosition = Vector3.Lerp(targetPosition, playerPos + leadOffset + offset, Time.deltaTime * followSpeed * 0.5f);
            transform.position = targetPosition;
        }

        lastPlayerSpeed = playerSpeed;
    }

    private void UpdateZoom()
    {
        if (isRoomControllingZoom) return;

        if (player.Dead && !wasDead)
        {
            targetZoom = deadZoom;
            wasDead = true;
            ApplyShake(deadShakeIntensity);
        }
        else if (!player.Dead && wasDead)
        {
            targetZoom = normalZoom;
            wasDead = false;
        }

        if (player.Rolling && !wasRolling)
        {
            bool isPerfectDodge = (Time.time - player.LastEnemyAttackTime) <= player.PerfectDodgeWindow;

            targetZoom = isPerfectDodge ? perfectDodgeZoom : dashZoom;
            wasRolling = true;

            if (isPerfectDodge)
            {
                ApplyShake(perfectDodgeShakeIntensity);
            }
            else
            {
                ApplyShake(dashShakeIntensity);
                float xDir = player.xInput;
                if (Mathf.Abs(xDir) > 0.1f)
                {
                    ApplyTilt(xDir > 0 ? -dashTiltAngle : dashTiltAngle);
                }
            }
        }
        else if (!player.Rolling && wasRolling)
        {
            CurrentPlayerLocation playerLocation = CurrentPlayerLocation.Instance;
            if (playerLocation != null && playerLocation.IsInShop())
            {
                targetZoom = shopZoom;
            }
            else
            {
                targetZoom = normalZoom;
            }
            wasRolling = false;
        }

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
    }

    private void UpdateShake()
    {
        if (currentShake > 0)
        {
            float time = Time.time * 10f;
            Vector2 shakeOffset = new Vector2(
                Mathf.PerlinNoise(time, 0) - 0.5f,
                Mathf.PerlinNoise(0, time) - 0.5f
            ) * currentShake;

            transform.position += new Vector3(shakeOffset.x, shakeOffset.y, 0);

            currentShake = Mathf.Lerp(currentShake, 0, Time.deltaTime * shakeDecay);

            if (currentShake < 0.01f)
            {
                currentShake = 0;
            }
        }
    }

    private void UpdateTilt()
    {
        if (Mathf.Abs(currentTilt - targetTilt) > 0.01f)
        {
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltRecoverySpeed);
            transform.rotation = Quaternion.Euler(0, 0, currentTilt);
        }

        if (Mathf.Abs(targetTilt) > 0.01f)
        {
            targetTilt = Mathf.Lerp(targetTilt, 0, Time.deltaTime * tiltRecoverySpeed * 0.7f);

            if (Mathf.Abs(targetTilt) < 0.01f)
            {
                targetTilt = 0;
                currentTilt = 0;
                transform.rotation = Quaternion.identity;
            }
        }
    }

    public void ApplyShake(float intensity)
    {
        currentShake = Mathf.Max(currentShake, intensity);
    }

    public void ApplyTilt(float angle)
    {
        targetTilt = angle;
    }

    public void SetShopZoom()
    {
        if (!isRoomControllingZoom)
        {
            targetZoom = shopZoom;
        }
    }

    public void SetNormalZoom()
    {
        if (!isRoomControllingZoom)
        {
            targetZoom = normalZoom;
        }
    }

    // New method to set room-specific zoom levels
    public void SetRoomZoom(float zoomLevel)
    {
        targetZoom = zoomLevel;
        isRoomControllingZoom = true;
    }

    // New method to reset zoom when exiting all rooms
    public void ResetToNormalZoom()
    {
        isRoomControllingZoom = false;
        CurrentPlayerLocation playerLocation = CurrentPlayerLocation.Instance;
        if (playerLocation != null && playerLocation.IsInShop())
        {
            targetZoom = shopZoom;
        }
        else
        {
            targetZoom = normalZoom;
        }
    }

    public void OnPlayerDamaged()
    {
        ApplyShake(damageShakeIntensity);
        if (!isRoomControllingZoom)
        {
            StartCoroutine(BriefZoom(combatZoom, 0.3f));
        }
    }

    public void OnPerfectDodge()
    {
        ApplyShake(perfectDodgeShakeIntensity);
        if (!isRoomControllingZoom)
        {
            StartCoroutine(BriefZoom(perfectDodgeZoom, 0.5f));
        }
    }

    public void OnEnemyKilled()
    {
        if (!isRoomControllingZoom)
        {
            StartCoroutine(BriefZoom(combatZoom + 0.3f, 0.3f));
        }
    }

    private IEnumerator BriefZoom(float zoom, float duration)
    {
        if (isRoomControllingZoom) yield break;

        float originalZoom = targetZoom;
        targetZoom = zoom;

        yield return new WaitForSeconds(duration);

        if (targetZoom == zoom)
        {
            CurrentPlayerLocation playerLocation = CurrentPlayerLocation.Instance;
            if (playerLocation != null && playerLocation.IsInShop())
            {
                targetZoom = shopZoom;
            }
            else
            {
                targetZoom = normalZoom;
            }
        }
    }
}