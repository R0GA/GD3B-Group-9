using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public Image flashImage;
    public float flashDuration = 0.2f;
    public Color flashColor = new Color(1, 0, 0, 0.5f);
    private bool isFlashing = false;

    private void Awake()
    {
        if (flashImage != null)
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
    }

    public void Flash()
    {
        if (!isFlashing)
            StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;
        float timer = 0f;

        //Fade in
        while (timer < flashDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, flashColor.a, timer / (flashDuration / 2f));
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        //Fade out
        timer = 0f;
        while (timer < flashDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0, timer / (flashDuration / 2f));
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        isFlashing = false;
    }
}

