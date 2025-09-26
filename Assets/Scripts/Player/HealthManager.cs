using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBar;
    public GameObject gameOverScreen;

    public void Start()
    {
        currentHealth = maxHealth;
        updateHealthBar();
    }

    public void Update()
    {
        if(currentHealth <= 0)
        {
            gameOverScreen.SetActive(true);
        }
    }

    public void updateHealth(float amount)
    {
        currentHealth += amount;
        updateHealthBar();
    }

    public void updateHealthBar()
    {
        float targetFillAmount = currentHealth / maxHealth;
        healthBar.fillAmount = targetFillAmount;
    }

    public void PlayerHit()
    {
        currentHealth = currentHealth = 10f;
        updateHealthBar();
    }
}
