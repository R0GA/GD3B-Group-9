using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image creatureIcon;
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI creatureNameText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image elementIcon;
    [SerializeField] private GameObject activeHighlight;
    [SerializeField] private GameObject emptySlotPanel;

    [Header("Sprites")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite grassIcon;

    private int slotIndex;
    private CreatureData creatureData;
    private bool isActive;

    public void Initialize(int index)
    {
        slotIndex = index;

        if (slotNumberText != null)
        {
            slotNumberText.text = (index + 1).ToString();
        }

        SetEmpty(false);
    }

    public void SetCreatureData(CreatureData data, bool active)
    {
        creatureData = data;
        isActive = active;

        if (creatureData == null)
        {
            SetEmpty(active);
            return;
        }

        // Hide empty panel
        if (emptySlotPanel != null)
            emptySlotPanel.SetActive(false);

        // Show creature info
        creatureIcon.gameObject.SetActive(true);
        creatureIcon.sprite = data.icon;

        if (creatureNameText != null)
        {
            creatureNameText.text = data.DisplayName; // Use DisplayName instead of prefabName
            creatureNameText.gameObject.SetActive(true);
        }


        if (levelText != null)
        {
            levelText.text = $"Lv.{data.level}";
            levelText.gameObject.SetActive(true);
        }

        // Health bar
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true);
            healthBar.maxValue = data.maxHealth;
            healthBar.value = data.health;
        }

        // Element icon
        if (elementIcon != null)
        {
            elementIcon.sprite = GetElementSprite(data.elementType);
            elementIcon.gameObject.SetActive(true);
        }

        // Active highlight
        if (activeHighlight != null)
        {
            activeHighlight.SetActive(active);
        }
    }

    public void SetEmpty(bool active)
    {
        creatureData = null;
        isActive = active;

        // Show empty panel
        if (emptySlotPanel != null)
            emptySlotPanel.SetActive(true);

        // Hide creature info
        creatureIcon.gameObject.SetActive(false);

        if (creatureNameText != null)
            creatureNameText.gameObject.SetActive(false);

        if (levelText != null)
            levelText.gameObject.SetActive(false);

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        if (elementIcon != null)
            elementIcon.gameObject.SetActive(false);

        // Active highlight for empty slot
        if (activeHighlight != null)
        {
            activeHighlight.SetActive(active);
        }
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

    // Called when the slot is clicked
    public void OnSlotClicked()
    {
        HotbarUIManager.Instance.SwitchToCreature(slotIndex);
    }
}