using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    private TextMeshPro Damage;
    private Rigidbody2D rb;
    private WeaponTypes wt;

    private GameManager manager;

    private PlayerStats Stats;

    private float Gravity;

    private void Awake()
    {

        Vector3 randomOffset = new Vector3(
        Random.Range(-0.5f, 0.5f),
        Random.Range(-0.5f, 0.5f),
        -6
    );
        transform.position += randomOffset;


        Damage = GetComponent<TextMeshPro>(); // Assign the TextMeshPro component
        if (Damage == null)
        {
            return;
        }

        manager = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        wt = FindObjectOfType<WeaponTypes>();
        Stats = FindObjectOfType<PlayerStats>();
        float WaitTime = Mathf.Abs(Mathf.Log10(2.5f + Stats.AttackDamage * wt.DamageMultiplier / Stats.AttackSpeed * wt.AttackSpeedMultiplier - ((Stats.AttackDamage * 0.05f) - Stats.AttackSpeed))) * 0.5f;
        Gravity = Mathf.Abs((1 / WaitTime * 0.5f) - 0.1f);
        StartCoroutine(Dissapear(WaitTime, Gravity));
    }

    private IEnumerator Dissapear(float time, float gravity)
    {
        yield return new WaitForSeconds(0.0125f);
        float Alpha = Damage.color.a;
        float elapsedTime = 0f;
        float value = (Random.value < 0.5f) ? -1f : 1f;

        if (rb != null)
        {
            float attackDamage = Mathf.Max(1, Stats.AttackDamage);
            float damageMultiplier = Mathf.Max(0.1f, wt.DamageMultiplier);
            float enemyHealthMultiplier = Mathf.Max(0.1f, manager.enemyHealthMultiplier);
            float safeTime = Mathf.Max(0.01f, time);

            float upwardForce = Mathf.Log10((5 + attackDamage * damageMultiplier) * 10f) + 1f / safeTime;
            float sidewaysForce = value * Mathf.Log10((5 + attackDamage * damageMultiplier)) * 2 * (Mathf.Log(attackDamage) / enemyHealthMultiplier);

            if (!float.IsNaN(upwardForce) && !float.IsNaN(sidewaysForce))
            {
                rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
                rb.AddForce(Vector2.right * sidewaysForce * 0.5f, ForceMode2D.Impulse);
            }

            rb.gravityScale = gravity;
        }

        float FontMultiplier = Damage.fontSize * Mathf.Log10(10 + int.Parse(Damage.text) / manager.enemyHealthMultiplier);
        Damage.fontSize = FontMultiplier;

        yield return new WaitForSeconds(time);

        while (Alpha > 0)
        {
            elapsedTime += Time.deltaTime;
            Alpha = Mathf.Lerp(0.75f, 0, elapsedTime / time);
            Damage.color = new Color(Damage.color.r, Damage.color.g, Damage.color.b, Alpha);
            yield return null;
        }

        Destroy(gameObject);


    }

    void Update()
    {
        DamageIndicator[] DamageIndicators = FindObjectsOfType<DamageIndicator>();
        if (DamageIndicators.Length > 50)
        {
            Destroy(gameObject);
        }
    }
}
