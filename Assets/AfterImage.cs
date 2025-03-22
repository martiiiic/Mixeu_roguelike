using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [Tooltip("How long the after image should last before completely fading")]
    public float fadeTime = 0.5f;

    [Tooltip("The curve that controls how the sprite fades out")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private SpriteRenderer spriteRenderer;
    private Color initialColor;
    private float elapsedTime = 0f;
    private bool fading = false;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            initialColor = spriteRenderer.color;
            StartFading();
        }
        else
        {
            Debug.LogError("AfterImage script requires a SpriteRenderer component");
        }
    }
    void Update()
    {
        if (fading)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= fadeTime)
            {
                Destroy(gameObject);
                return;
            }

            float normalizedTime = elapsedTime / fadeTime;
            float alpha = fadeCurve.Evaluate(1 - normalizedTime);

            Color newColor = initialColor;
            newColor.a = alpha;
            spriteRenderer.color = newColor;
        }
    }

    public void StartFading()
    {
        fading = true;
        elapsedTime = 0f;
    }
}