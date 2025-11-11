using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CollectibleNotification : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI messageText;
    public Image iconImage;
    public CanvasGroup canvasGroup;

    private float displayTime;
    private float fadeTime;

    public void Initialize(string message, Sprite icon, Color color, float duration, float fadeDuration)
    {
        messageText.text = message;
        messageText.color = color;

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.color = color;
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        displayTime = duration;
        fadeTime = fadeDuration;

        StartCoroutine(NotificationLifecycle());
    }

    private IEnumerator NotificationLifecycle()
    {
        // Fade in
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float timer = 0f;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeTime);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // Wait for display time
        yield return new WaitForSeconds(displayTime);

        // Fade out and destroy
        FadeOut();
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        if (canvasGroup != null)
        {
            float timer = 0f;
            float startAlpha = canvasGroup.alpha;

            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeTime);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}