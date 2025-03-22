using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBar : MonoBehaviour
{
    public Slider slider;
    public Slider easeSlider;
    public int maxHealth;
    public int Health;
    public float lerpSpeed = 0.05f;
    public int Damage;
    public GameObject BossBar_;
    public string BossName;
    public TextMeshProUGUI Name;

    private void Start()
    {
        BossBar_.SetActive(false);
    }
    private void Update()
    {
        if(slider.value != Health)
        {
            slider.value = Health;
        }
        if (slider.value != easeSlider.value)
        {
            easeSlider.value = Mathf.Lerp(easeSlider.value, slider.value, lerpSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(Damage);
        }
    }

    public void TakeDamage(int damage)
    {
        lerpSpeed = Mathf.Abs(0.01f * maxHealth / (damage));
        Health -= damage;
    }

    public void ShowBossBar()
    {
        BossBar_.SetActive(true);
        Name.text = BossName;
    }
    public void HideBossBar()
    {
        BossBar_.SetActive(false);
    }
}
