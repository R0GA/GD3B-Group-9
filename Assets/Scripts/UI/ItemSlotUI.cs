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

    private ItemBase item;

    public void Initialize(ItemBase itemData)
    {
        item = itemData;

        itemIcon.sprite = itemData.icon;
        nameText.text = itemData.ItemName;

        // Show stats
        string stats = "";
        if (itemData.HealthModifier != 0)
            stats += $"HP: {itemData.HealthModifier:+0;-0}\n";
        if (itemData.DamageModifier != 0)
            stats += $"DMG: {itemData.DamageModifier:+0;-0}";

        statsText.text = stats;

        equipButton.onClick.AddListener(OnEquipButtonClicked);
    }

    private void OnEquipButtonClicked()
    {
        // Open equipment panel or show which creatures can equip this
        Debug.Log($"Equip {item.ItemName} - This would open an equipment interface");
    }
}