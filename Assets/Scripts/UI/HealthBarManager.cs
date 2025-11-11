using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    [Header("Health Bar References")]
    public Slider healthSlider;
    public PlayerController playerTarget;

    private void Start()
    {
        // Auto-find player if not assigned
        if (playerTarget == null)
        {
            playerTarget = FindObjectOfType<PlayerController>();
        }

        // Set up health bar
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = GetMaxHealth();
            healthSlider.value = GetCurrentHealth();
        }
    }

    private void Update()
    {
        if (healthSlider != null)
        {
            healthSlider.value = GetCurrentHealth();
            healthSlider.maxValue = GetMaxHealth();
        }
    }

    private float GetCurrentHealth()
    {
        if (playerTarget != null) return playerTarget.health;
        return 0;
    }

    private float GetMaxHealth()
    {
        if (playerTarget != null) return playerTarget.maxHealth;
        return 100;
    }
}