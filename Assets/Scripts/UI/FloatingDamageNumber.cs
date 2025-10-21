using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingDamageNumber : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro textMesh; // 3D TextMeshPro

    [Header("Animation Settings")]
    public float floatSpeed = 2f;
    public float floatHeight = 1.5f;
    public float lifeTime = 1.5f;

    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color fireColor = Color.red;
    public Color waterColor = Color.blue;
    public Color grassColor = Color.green;
    public Color superEffectiveColor = Color.yellow;
    public Color notVeryEffectiveColor = Color.gray;

    private Vector3 startPosition;
    private float startTime;
    private Transform mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    public void Initialize(float damage, ElementType damageType, float effectivenessMultiplier, Vector3 spawnPosition)
    {
        startPosition = spawnPosition;
        transform.position = spawnPosition;

        // Set the damage text (same as before)
        string damageText = Mathf.RoundToInt(damage).ToString();

        if (effectivenessMultiplier > 1.5f)
        {
            damageText += "!!!";
            textMesh.color = superEffectiveColor;
            textMesh.fontSize *= 1.2f;
        }
        else if (effectivenessMultiplier > 1f)
        {
            damageText += "!";
            textMesh.color = GetElementColor(damageType);
        }
        else if (effectivenessMultiplier < 1f)
        {
            damageText += "...";
            textMesh.color = notVeryEffectiveColor;
            textMesh.fontSize *= 0.9f;
        }
        else
        {
            textMesh.color = normalColor;
        }

        textMesh.text = damageText;
        startTime = Time.time;

        StartCoroutine(FloatAnimation());
    }

    private Color GetElementColor(ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire: return fireColor;
            case ElementType.Water: return waterColor;
            case ElementType.Grass: return grassColor;
            default: return normalColor;
        }
    }

    private void Update()
    {
        // Billboard effect for 3D Text - always face the camera
        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);
        }
    }

    private IEnumerator FloatAnimation()
    {
        while (Time.time - startTime < lifeTime)
        {
            float elapsed = Time.time - startTime;
            float progress = elapsed / lifeTime;

            // Float upward
            Vector3 newPosition = startPosition + Vector3.up * (floatHeight * progress);
            transform.position = newPosition;

            // Fade out
            Color color = textMesh.color;
            color.a = 1f - progress;
            textMesh.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}