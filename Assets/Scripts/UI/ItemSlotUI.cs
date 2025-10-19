using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button dequipButton;
    [SerializeField] private Toggle equippedCheckbox; // Added

    private PlayerInventory playerInventory;

    private ItemBase item;

    public void Initialize(ItemBase itemData)
    {
        item = itemData;

        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        itemIcon.sprite = itemData.icon;
        nameText.text = itemData.ItemName;

        // Show stats
        string stats = "";
        if (itemData.HealthModifierPercent != 0)
            stats += $"HP % Mod: {itemData.HealthModifierPercent:+0;-0}\n";
        if (itemData.DamageModifierPercent != 0)
            stats += $"DMG % Mod: {itemData.DamageModifierPercent:+0;-0}";

        statsText.text = stats;

        // Ensure no duplicate listeners
        equipButton.onClick.RemoveAllListeners();
        dequipButton.onClick.RemoveAllListeners();

        equipButton.onClick.AddListener(OnEquipButtonClicked);
        dequipButton.onClick.AddListener(OnDequipButtonClicked);

        // Update checkbox based on current equip state (safe-guard null)
        if (equippedCheckbox != null)
        {
            bool isEquipped = playerInventory != null && playerInventory.IsItemEquipped(item);
            equippedCheckbox.isOn = isEquipped;
            // Make it an indicator by default (not interactable). Remove or change if you want toggle behavior.
            equippedCheckbox.interactable = false;
        }
    }

    private void Awake()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    private void OnEquipButtonClicked()
    {
        if (playerInventory == null || item == null) return;

        bool success = playerInventory.EquipItemToActiveCreature(item);
        if (equippedCheckbox != null && success)
            equippedCheckbox.isOn = true;
    }

    private void OnDequipButtonClicked()
    {
        if (playerInventory == null || item == null) return;

        bool success = playerInventory.DequipItemFromActiveCreature(item);
        if (equippedCheckbox != null && success)
            equippedCheckbox.isOn = false;
    }
}