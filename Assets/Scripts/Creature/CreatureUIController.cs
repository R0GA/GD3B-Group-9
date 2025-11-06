using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreatureUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas creatureCanvas;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image levelBackground;
    [SerializeField] private GameObject friendlyIndicator;
    [SerializeField] private GameObject enemyIndicator;
    [SerializeField] private Image elementIcon;
    [SerializeField] private GameObject statusEffectsPanel;

    [Header("UI Settings")]
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float healthUpdateSpeed = 5f;
    [SerializeField] private bool showHealthNumbers = true;
    [SerializeField] private bool showLevel = true;

    [Header("Element Icons")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite grassIcon;
    [SerializeField] private Sprite defaultIcon;

    [Header("Colors")]
    [SerializeField] private Color friendlyColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color neutralColor = Color.yellow;

    private CreatureBase creatureBase;
    private Camera mainCamera;
    private float currentDisplayHealth;
    private bool isInitialized = false;

    private void Awake()
    {
        creatureBase = GetComponent<CreatureBase>();
        mainCamera = Camera.main;

        // Create UI if not assigned
        if (creatureCanvas == null)
            CreateUIElements();
    }

    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }

    private void CreateUIElements()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("CreatureCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = uiOffset;
        creatureCanvas = canvasGO.AddComponent<Canvas>();
        creatureCanvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Set canvas size
        RectTransform canvasRect = creatureCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 1f);

        // Create health bar background
        GameObject healthBG = new GameObject("HealthBackground");
        healthBG.transform.SetParent(canvasGO.transform);
        Image bgImage = healthBG.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = healthBG.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(180, 20);
        bgRect.localPosition = Vector3.zero;

        // Create health bar
        GameObject healthGO = new GameObject("HealthBar");
        healthGO.transform.SetParent(healthBG.transform);
        healthBar = healthGO.AddComponent<Slider>();
        Image fillImage = healthGO.AddComponent<Image>();
        fillImage.color = Color.green;

        RectTransform healthRect = healthGO.GetComponent<RectTransform>();
        healthRect.sizeDelta = new Vector2(176, 16);
        healthRect.localPosition = Vector3.zero;

        // Create health text
        GameObject healthTextGO = new GameObject("HealthText");
        healthTextGO.transform.SetParent(canvasGO.transform);
        healthText = healthTextGO.AddComponent<TextMeshProUGUI>();
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontSize = 12;
        healthText.color = Color.white;

        RectTransform textRect = healthTextGO.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(180, 20);
        textRect.localPosition = new Vector3(0, -25, 0);

        // Create level background
        GameObject levelBG = new GameObject("LevelBackground");
        levelBG.transform.SetParent(canvasGO.transform);
        levelBackground = levelBG.AddComponent<Image>();
        levelBackground.color = new Color(0.1f, 0.1f, 0.5f, 0.8f);
        RectTransform levelBGRect = levelBG.GetComponent<RectTransform>();
        levelBGRect.sizeDelta = new Vector2(40, 20);
        levelBGRect.localPosition = new Vector3(-70, 15, 0);

        // Create level text
        GameObject levelTextGO = new GameObject("LevelText");
        levelTextGO.transform.SetParent(levelBG.transform);
        levelText = levelTextGO.AddComponent<TextMeshProUGUI>();
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.fontSize = 10;
        levelText.color = Color.white;
        levelText.text = "1";

        RectTransform levelTextRect = levelTextGO.GetComponent<RectTransform>();
        levelTextRect.sizeDelta = new Vector2(40, 20);
        levelTextRect.localPosition = Vector3.zero;

        // Create friendly indicator
        GameObject friendlyGO = new GameObject("FriendlyIndicator");
        friendlyGO.transform.SetParent(canvasGO.transform);
        friendlyIndicator = friendlyGO;
        Image friendlyImage = friendlyGO.AddComponent<Image>();
        friendlyImage.color = friendlyColor;

        RectTransform friendlyRect = friendlyGO.GetComponent<RectTransform>();
        friendlyRect.sizeDelta = new Vector2(15, 15);
        friendlyRect.localPosition = new Vector3(75, 15, 0);

        // Create enemy indicator
        GameObject enemyGO = new GameObject("EnemyIndicator");
        enemyGO.transform.SetParent(canvasGO.transform);
        enemyIndicator = enemyGO;
        Image enemyImage = enemyGO.AddComponent<Image>();
        enemyImage.color = enemyColor;

        RectTransform enemyRect = enemyGO.GetComponent<RectTransform>();
        enemyRect.sizeDelta = new Vector2(15, 15);
        enemyRect.localPosition = new Vector3(75, 15, 0);
    }

    private void InitializeUI()
    {
        if (creatureBase == null) return;

        // Set initial values
        currentDisplayHealth = creatureBase.health;
        healthBar.maxValue = creatureBase.maxHealth;
        healthBar.value = currentDisplayHealth;

        // Update level display
        levelText.text = creatureBase.level.ToString();

        // Set faction indicators
        UpdateFactionIndicator();

        // Set element icon
        UpdateElementIcon();

        // Show/hide elements based on settings
        healthText.gameObject.SetActive(showHealthNumbers);
        levelBackground.gameObject.SetActive(showLevel);

        isInitialized = true;
    }

    private void SubscribeToEvents()
    {
        if (creatureBase != null)
        {
            creatureBase.OnDamageTaken += OnDamageTaken;
            creatureBase.OnHealed += OnHealed;
            creatureBase.OnLevelUp += OnLevelUp;
            creatureBase.OnDeath += OnDeath;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (creatureBase != null)
        {
            creatureBase.OnDamageTaken -= OnDamageTaken;
            creatureBase.OnHealed -= OnHealed;
            creatureBase.OnLevelUp -= OnLevelUp;
            creatureBase.OnDeath -= OnDeath;
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Make UI face camera
        if (mainCamera != null)
        {
            creatureCanvas.transform.rotation = Quaternion.LookRotation(
                creatureCanvas.transform.position - mainCamera.transform.position);
        }

        // Smooth health bar animation
        if (Mathf.Abs(currentDisplayHealth - creatureBase.health) > 0.1f)
        {
            currentDisplayHealth = Mathf.Lerp(currentDisplayHealth, creatureBase.health,
                healthUpdateSpeed * Time.deltaTime);
            UpdateHealthBar();
        }
        else
        {
            currentDisplayHealth = creatureBase.health;
            UpdateHealthBar();
        }
    }

    private void OnDamageTaken(CreatureBase creature)
    {
        UpdateHealthBar();

        // Flash effect (optional)
        StartCoroutine(FlashHealthBar(Color.red, 0.2f));
    }

    private void OnHealed(CreatureBase creature)
    {
        UpdateHealthBar();

        // Flash effect (optional)
        StartCoroutine(FlashHealthBar(Color.green, 0.2f));
    }

    private void OnLevelUp(CreatureBase creature, int newLevel)
    {
        levelText.text = newLevel.ToString();

        // Level up effect
        StartCoroutine(LevelUpAnimation());
    }

    private void OnDeath(CreatureBase creature)
    {
        // Hide UI or show death indicator
        creatureCanvas.gameObject.SetActive(false);
    }

    private void UpdateHealthBar()
    {
        healthBar.value = currentDisplayHealth;

        if (healthText != null && showHealthNumbers)
        {
            healthText.text = $"{Mathf.RoundToInt(currentDisplayHealth)} / {Mathf.RoundToInt(creatureBase.maxHealth)}";
        }

        // Change color based on health percentage
        float healthPercent = currentDisplayHealth / creatureBase.maxHealth;
        Image fillImage = healthBar.fillRect.GetComponent<Image>();

        if (fillImage != null)
        {
            if (healthPercent > 0.6f)
                fillImage.color = Color.green;
            else if (healthPercent > 0.3f)
                fillImage.color = Color.yellow;
            else
                fillImage.color = Color.red;
        }
    }

    private void UpdateFactionIndicator()
    {
        if (friendlyIndicator != null && enemyIndicator != null)
        {
            friendlyIndicator.SetActive(creatureBase.isPlayerCreature);
            enemyIndicator.SetActive(!creatureBase.isPlayerCreature);
        }
    }

    private void UpdateElementIcon()
    {
        if (elementIcon != null)
        {
            switch (creatureBase.elementType)
            {
                case ElementType.Fire:
                    elementIcon.sprite = fireIcon;
                    //elementIcon.color = Color.red;
                    break;
                case ElementType.Water:
                    elementIcon.sprite = waterIcon;
                    //elementIcon.color = Color.blue;
                    break;
                case ElementType.Grass:
                    elementIcon.sprite = grassIcon;
                    //elementIcon.color = Color.green;
                    break;
                default:
                    elementIcon.sprite = defaultIcon;
                    //elementIcon.color = Color.white;
                    break;
            }
        }
    }

    private System.Collections.IEnumerator FlashHealthBar(Color flashColor, float duration)
    {
        Image fillImage = healthBar.fillRect.GetComponent<Image>();
        if (fillImage == null) yield break;

        Color originalColor = fillImage.color;
        fillImage.color = flashColor;

        yield return new WaitForSeconds(duration);

        fillImage.color = originalColor;
    }

    private System.Collections.IEnumerator LevelUpAnimation()
    {
        if (levelBackground != null)
        {
            Vector3 originalScale = levelBackground.transform.localScale;
            float elapsedTime = 0f;
            float animationTime = 0.5f;

            while (elapsedTime < animationTime)
            {
                float scale = Mathf.Lerp(1f, 1.5f, Mathf.PingPong(elapsedTime * 4f, 1f));
                levelBackground.transform.localScale = originalScale * scale;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            levelBackground.transform.localScale = originalScale;
        }
    }

    // Public methods to configure UI
    public void SetUIOffset(Vector3 newOffset)
    {
        uiOffset = newOffset;
        if (creatureCanvas != null)
        {
            creatureCanvas.transform.localPosition = newOffset;
        }
    }

    public void ToggleHealthNumbers(bool show)
    {
        showHealthNumbers = show;
        if (healthText != null)
        {
            healthText.gameObject.SetActive(show);
        }
    }

    public void ToggleLevelDisplay(bool show)
    {
        showLevel = show;
        if (levelBackground != null)
        {
            levelBackground.gameObject.SetActive(show);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}