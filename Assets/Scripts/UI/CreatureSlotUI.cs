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

    [Header("Sprites")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite grassIcon;

    private CreatureData creatureData;
    private bool isInParty;
    private int slotIndex;

    public void Initialize(CreatureData data, bool inParty, int index)
    {
        creatureData = data;
        isInParty = inParty;
        slotIndex = index;

        UpdateDisplay();
        SetupButtons();
    }

    private void UpdateDisplay()
    {
        if (creatureData == null) return;

        creatureIcon.sprite = creatureData.icon;
        nameText.text = creatureData.prefabName.Replace("Creatures/", "");
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

        if (isInParty)
        {
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Remove";
            actionButton.onClick.AddListener(RemoveFromParty);

            if (setActiveButton != null)
            {
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