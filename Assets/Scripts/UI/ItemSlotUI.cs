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

    private PlayerInventory playerInventory;

    private ItemBase item;

    public void Initialize(ItemBase itemData)
    {
        item = itemData;

        itemIcon.sprite = itemData.icon;
        nameText.text = itemData.ItemName;

        // Show stats
        string stats = "";
        if (itemData.HealthModifierPercent != 0)
            stats += $"HP % Mod: {itemData.HealthModifierPercent:+0;-0}\n";
        if (itemData.DamageModifierPercent != 0)
            stats += $"DMG % Mod: {itemData.DamageModifierPercent:+0;-0}";

        statsText.text = stats;

        equipButton.onClick.AddListener(OnEquipButtonClicked);
    }

    private void Awake()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    private void OnEquipButtonClicked()
    {
        playerInventory.EquipItemToActiveCreature(item);
    }
}