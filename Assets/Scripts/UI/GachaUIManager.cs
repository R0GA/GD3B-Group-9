using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUIManager : MonoBehaviour
{
    public static GachaUIManager Instance;

    [Header("Main References")]
    [SerializeField] private GachaSystem gachaSystem;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("UI Panels")]
    [SerializeField] private GameObject gachaMainPanel;
    [SerializeField] private GameObject rollingPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Rolling Animation")]
    [SerializeField] private Image rollingDisplayImage;
    [SerializeField] private TextMeshProUGUI rollingDisplayName;
    [SerializeField] private float initialRollSpeed = 0.1f;
    [SerializeField] private float slowDownDuration = 2f;
    [SerializeField] private int minRollCycles = 8;
    [SerializeField] private AudioSource rollSound;
    [SerializeField] private AudioSource resultSound;

    [Header("Result Display")]
    [SerializeField] private Image resultImage;
    [SerializeField] private TextMeshProUGUI resultName;
    [SerializeField] private TextMeshProUGUI resultDescription;
    [SerializeField] private GameObject creatureResultUI;
    [SerializeField] private GameObject itemResultUI;
    [SerializeField] private TextMeshProUGUI creatureStatsText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    [SerializeField] private ParticleSystem celebrationParticles;

    [Header("Pack Info")]
    [SerializeField] private TextMeshProUGUI packCountText;
    [SerializeField] private Button openPackButton;

    // Add rarity colors for more visual appeal
    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = Color.magenta;
    [SerializeField] private Color legendaryColor = Color.yellow;

    private List<object> allPossibleResults = new List<object>();
    private Coroutine rollingCoroutine;
    private object finalResult;
    private int availablePacks = 100; // You can modify this based on your game economy

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load all possible results for the rolling animation
        allPossibleResults = gachaSystem.GetAllPossibleResults();
        UpdatePackDisplay();

        // Set up button
        openPackButton.onClick.AddListener(StartGachaRoll);

        if(gameObject.active)
            gameObject.SetActive(false);
    }

    public bool IsActive()
    {
        return gachaMainPanel.activeInHierarchy || rollingPanel.activeInHierarchy || resultPanel.activeInHierarchy;
    }

    public void ShowGachaUI()
    {
        if (!PlayerController.IsInHub())
        {
            Debug.Log("Gacha can only be opened in the hub.");
               return;
       }
        if (!gameObject.active)
        {
            gameObject.SetActive(true);
            gachaMainPanel.SetActive(true);
            rollingPanel.SetActive(false);
            resultPanel.SetActive(false);
            UpdatePackDisplay();
        }
        else
        {
            HideGachaUI();
        }
        
    }

    public void HideGachaUI()
    {
        gachaMainPanel.SetActive(false);
        gameObject.SetActive(false);    
    }

    private void UpdatePackDisplay()
    {
        packCountText.text = $"Packs Available: {availablePacks}";
        openPackButton.interactable = availablePacks > 0;
    }

    public void StartGachaRoll()
    {
        if (availablePacks <= 0) return;

        availablePacks--;
        UpdatePackDisplay();

        // Get the actual result first (so we know what to land on)
        finalResult = gachaSystem.GetGachaResult();

        // Switch to rolling panel
        gachaMainPanel.SetActive(false);
        rollingPanel.SetActive(true);
        resultPanel.SetActive(false);

        // Start rolling animation
        if (rollingCoroutine != null)
            StopCoroutine(rollingCoroutine);
        rollingCoroutine = StartCoroutine(RollingAnimation());
    }

    private IEnumerator RollingAnimation()
    {
        if (rollSound != null)
            rollSound.Play();

        float timer = 0f;
        float currentSpeed = initialRollSpeed;
        int cyclesCompleted = 0;
        bool isSlowingDown = false;

        // Initial fast rolling
        while (cyclesCompleted < minRollCycles || !isSlowingDown)
        {
            // Display random item from pool
            var randomDisplay = allPossibleResults[Random.Range(0, allPossibleResults.Count)];
            UpdateRollingDisplay(randomDisplay);

            // Wait for current speed
            yield return new WaitForSeconds(currentSpeed);

            cyclesCompleted++;

            // Start slowing down after minimum cycles
            if (cyclesCompleted >= minRollCycles && !isSlowingDown)
            {
                isSlowingDown = true;
                timer = 0f;
            }

            // Gradually slow down
            if (isSlowingDown)
            {
                timer += currentSpeed;
                float progress = timer / slowDownDuration;
                currentSpeed = Mathf.Lerp(initialRollSpeed, initialRollSpeed * 3f, progress);

                // When we're very slow, break and show final result
                if (progress >= 0.9f)
                {
                    break;
                }
            }
        }

        // Final slow reveals
        for (int i = 0; i < 3; i++)
        {
            var randomDisplay = allPossibleResults[Random.Range(0, allPossibleResults.Count)];
            UpdateRollingDisplay(randomDisplay);
            yield return new WaitForSeconds(0.3f);
        }

        // Show the actual result
        UpdateRollingDisplay(finalResult);
        yield return new WaitForSeconds(0.5f);

        // Stop rolling sound, play result sound
        if (rollSound != null)
            rollSound.Stop();
        if (resultSound != null)
            resultSound.Play();

        // Show celebration effects
        if (celebrationParticles != null)
            celebrationParticles.Play();

        // Switch to result panel
        ShowResult();
    }

    private void UpdateRollingDisplay(object displayObject)
    {
        if (displayObject is CreatureData creatureData)
        {
            rollingDisplayImage.sprite = creatureData.icon;
            rollingDisplayName.text = creatureData.prefabName.Replace("Creatures/", "");
            rollingDisplayName.color = GetElementColor(creatureData.elementType);
        }
        else if (displayObject is ItemBase item)
        {
            rollingDisplayImage.sprite = item.icon;
            rollingDisplayName.text = item.ItemName;
            rollingDisplayName.color = Color.white;
        }
    }

    private Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => new Color(1f, 0.3f, 0.3f),
            ElementType.Water => new Color(0.3f, 0.5f, 1f),
            ElementType.Grass => new Color(0.3f, 0.8f, 0.3f),
            _ => Color.white
        };
    }

    private void ShowResult()
    {
        rollingPanel.SetActive(false);
        resultPanel.SetActive(true);

        if (finalResult is CreatureData creatureData)
        {
            creatureResultUI.SetActive(true);
            itemResultUI.SetActive(false);

            resultImage.sprite = creatureData.icon;
            resultName.text = creatureData.prefabName.Replace("Creatures/", "");
            resultName.color = GetElementColor(creatureData.elementType);

            creatureStatsText.text =
                $"Level: {creatureData.level}\n" +
                $"Health: {creatureData.health}/{creatureData.maxHealth}\n" +
                $"Damage: {creatureData.attackDamage}\n" +
                $"Element: {creatureData.elementType}";

            // Add to inventory
            playerInventory.AddCreatureData(creatureData);
        }
        else if (finalResult is ItemBase item)
        {
            creatureResultUI.SetActive(false);
            itemResultUI.SetActive(true);

            resultImage.sprite = item.icon;
            resultName.text = item.ItemName;
            resultName.color = Color.white;

            string stats = "";
            if (item.HealthModifierPercent != 0)
                stats += $"HP % Mod: {item.HealthModifierPercent:+#;-#}\n";
            if (item.DamageModifierPercent != 0)
                stats += $"Damage % Mod: {item.DamageModifierPercent:+#;-#}\n";
            stats += $"Element: {item.ElementType}";

            itemStatsText.text = stats;

            // Add to inventory
            playerInventory.AddItem(item);
        }
    }

    // Called by the "Continue" button in the result panel
    public void OnResultContinue()
    {
        resultPanel.SetActive(false);

        if (availablePacks > 0)
        {
            gachaMainPanel.SetActive(true);
        }
        else
        {
            HideGachaUI();
        }
    }

    // Method to add packs (for testing or in-game purchases)
    public void AddPacks(int count)
    {
        availablePacks += count;
        UpdatePackDisplay();
    }
}