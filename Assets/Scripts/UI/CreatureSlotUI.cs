using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image creatureIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image elementIcon;
    [SerializeField] private Button actionButton;
    [SerializeField] private Button setActiveButton;
    [SerializeField] private GameObject emptySlotPanel;

    [Header("Sprites")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite grassIcon;

    private CreatureData creatureData;
    private bool isInParty;
    private int slotIndex;
    private bool isEmptySlot;

    public void Initialize(CreatureData data, bool inParty, int index)
    {
        creatureData = data;
        isInParty = inParty;
        slotIndex = index;
        isEmptySlot = false;

        UpdateDisplay();
        SetupButtons();
    }

    public void InitializeAsEmptyPartySlot(int index)
    {
        creatureData = null;
        isInParty = true;
        slotIndex = index;
        isEmptySlot = true;

        UpdateDisplay();
        SetupButtons();
    }

    private void UpdateDisplay()
    {
        if (isEmptySlot)
        {
            // Show empty slot
            creatureIcon.gameObject.SetActive(false);
            nameText.gameObject.SetActive(false);
            levelText.gameObject.SetActive(false);
            healthBar.gameObject.SetActive(false);
            elementIcon.gameObject.SetActive(false);

            if (emptySlotPanel != null)
                emptySlotPanel.SetActive(true);

            return;
        }

        if (emptySlotPanel != null)
            emptySlotPanel.SetActive(false);

        if (creatureData == null) return;

        creatureIcon.gameObject.SetActive(true);
        nameText.gameObject.SetActive(true);
        levelText.gameObject.SetActive(true);
        healthBar.gameObject.SetActive(true);
        elementIcon.gameObject.SetActive(true);

        creatureIcon.sprite = creatureData.icon;
        nameText.text = creatureData.DisplayName; // Use DisplayName instead of prefabName
        levelText.text = $"Lv. {creatureData.level}";

        // Health bar
        healthBar.maxValue = creatureData.maxHealth;
        healthBar.value = creatureData.health;

        // Element icon
        elementIcon.sprite = GetElementSprite(creatureData.elementType);
    }


    private Sprite GetElementSprite(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => fireIcon,
            ElementType.Water => waterIcon,
            ElementType.Grass => grassIcon,
            _ => null
        };
    }

    private void SetupButtons()
    {
        // Clear existing listeners
        actionButton.onClick.RemoveAllListeners();
        setActiveButton?.onClick.RemoveAllListeners();

        if (isEmptySlot)
        {
            actionButton.gameObject.SetActive(false);
            setActiveButton?.gameObject.SetActive(false);
            return;
        }

        actionButton.gameObject.SetActive(true);

        if (isInParty)
        {
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Remove";
            actionButton.onClick.AddListener(RemoveFromParty);

            if (setActiveButton != null)
            {
                setActiveButton.gameObject.SetActive(true);
                setActiveButton.onClick.AddListener(SetAsActive);
            }
        }
        else
        {
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Add to Party";
            actionButton.onClick.AddListener(AddToParty);

            if (setActiveButton != null)
            {
                setActiveButton.gameObject.SetActive(false);
            }
        }
    }

    private void AddToParty()
    {
        InventoryUIManager.Instance.AddCreatureToParty(slotIndex);
    }

    private void RemoveFromParty()
    {
        InventoryUIManager.Instance.RemoveCreatureFromParty(slotIndex);
    }

    private void SetAsActive()
    {
        InventoryUIManager.Instance.SetActiveCreature(slotIndex);
    }
}