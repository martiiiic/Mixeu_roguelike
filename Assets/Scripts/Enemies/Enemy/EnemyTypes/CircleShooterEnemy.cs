using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleShooterEnemy : RangedEnemy
{
    [Header("Circle Shooter Settings")]
    public int numberOfProjectiles = 8;
    private float baseAngleOffset = 0f;

    protected override void HandleEnemyBehavior()
    {
        if (Time.time > lastShotTime + shootingCooldown + Random.Range(-0.5f, 0.4f))
        {
            ShootProjectileCircle();
            lastShotTime = Time.time;
        }

        MaintainDistanceFromPlayer();
        AvoidOtherRangedEnemies();
        AddUnpredictableMovement();
    }

    protected virtual void ShootProjectileCircle()
    {
        if (projectilePrefab == null) return;

        float angleStep = 360f / numberOfProjectiles;
        float gapAngle = 0f;

        baseAngleOffset += Random.Range(10f, 30f);
        if (baseAngleOffset >= 360f) baseAngleOffset -= 360f;

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = baseAngleOffset + i * angleStep;

            if (angle % 360 > gapAngle && angle % 360 < 360f - gapAngle)
            {
                float angleRad = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
                Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();

                if (projectileRb != null)
                {
                    projectileRb.velocity = direction * 10f;
                    LaserSoundSource.Play();
                }
            }
        }
    }
}
