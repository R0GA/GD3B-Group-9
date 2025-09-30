using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject creatureSlotPrefab;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("UI Containers")]
    [SerializeField] private Transform partySlotsContainer;
    [SerializeField] private Transform inventorySlotsContainer;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private Transform activeCreatureDisplay;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI activeCreatureName;
    [SerializeField] private Image activeCreatureIcon;
    [SerializeField] private Slider activeCreatureHealth;
    [SerializeField] private TextMeshProUGUI partyCountText;

    private PlayerInventory playerInventory;
    private List<GameObject> currentInventorySlots = new List<GameObject>();
    private List<GameObject> currentPartySlots = new List<GameObject>();
    private List<GameObject> currentItemSlots = new List<GameObject>();

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
        playerInventory = FindObjectOfType<PlayerInventory>();
        RefreshAllDisplays();
    }

    public void RefreshAllDisplays()
    {
        RefreshPartyDisplay();
        RefreshInventoryDisplay();
        RefreshItemsDisplay();
        RefreshActiveCreatureDisplay();
        UpdatePartyCount();
    }

    private void RefreshPartyDisplay()
    {
        // Clear existing slots
        foreach (var slot in currentPartySlots)
        {
            Destroy(slot);
        }
        currentPartySlots.Clear();

        // Create party slots
        for (int i = 0; i < playerInventory.PartyCreatureInventory.Count; i++)
        {
            var creatureData = playerInventory.PartyCreatureInventory[i];
            var slotObj = Instantiate(creatureSlotPrefab, partySlotsContainer);
            var slotUI = slotObj.GetComponent<CreatureSlotUI>();
            slotUI.Initialize(creatureData, true, i);
            currentPartySlots.Add(slotObj);
        }

        // Add empty party slots if needed
        for (int i = playerInventory.PartyCreatureInventory.Count; i < 3; i++)
        {
            var slotObj = Instantiate(creatureSlotPrefab, partySlotsContainer);
            var slotUI = slotObj.GetComponent<CreatureSlotUI>();
            slotUI.InitializeAsEmptyPartySlot(i);
            currentPartySlots.Add(slotObj);
        }
    }

    private void RefreshInventoryDisplay()
    {
        // Clear existing slots
        foreach (var slot in currentInventorySlots)
        {
            Destroy(slot);
        }
        currentInventorySlots.Clear();

        // Create inventory slots
        for (int i = 0; i < playerInventory.MainCreatureInventory.Count; i++)
        {
            var creatureData = playerInventory.MainCreatureInventory[i];
            var slotObj = Instantiate(creatureSlotPrefab, inventorySlotsContainer);
            var slotUI = slotObj.GetComponent<CreatureSlotUI>();
            slotUI.Initialize(creatureData, false, i);
            currentInventorySlots.Add(slotObj);
        }
    }

    private void RefreshItemsDisplay()
    {
        // Clear existing slots
        foreach (var slot in currentItemSlots)
        {
            Destroy(slot);
        }
        currentItemSlots.Clear();

        // Create item slots
        foreach (var item in playerInventory.Items)
        {
            var slotObj = Instantiate(itemSlotPrefab, itemsContainer);
            var slotUI = slotObj.GetComponent<ItemSlotUI>();
            slotUI.Initialize(item);
            currentItemSlots.Add(slotObj);
        }
    }

    private void RefreshActiveCreatureDisplay()
    {
        var activeData = playerInventory.ActivePartyCreatureData;
        if (activeData != null)
        {
            activeCreatureName.text = activeData.prefabName.Replace("Creatures/", "");
            activeCreatureIcon.sprite = activeData.icon;
            activeCreatureHealth.maxValue = activeData.maxHealth;
            activeCreatureHealth.value = activeData.health;
        }
        else
        {
            activeCreatureName.text = "No Active Creature";
            activeCreatureIcon.sprite = null;
            activeCreatureHealth.value = 0;
        }
    }

    private void UpdatePartyCount()
    {
        if (partyCountText != null)
        {
            partyCountText.text = $"{playerInventory.PartyCreatureInventory.Count}/3";
        }
    }

    // Public methods called by UI buttons
    public void AddCreatureToParty(int inventoryIndex)
    {
        if (playerInventory.AddCreatureToParty(inventoryIndex))
        {
            RefreshAllDisplays();
        }
        else
        {
            // Show error message (party full)
            Debug.LogWarning("Party is full! Remove a creature first.");
            // You could add a UI notification here
        }
    }

    public void RemoveCreatureFromParty(int partyIndex)
    {
        if (playerInventory.RemoveCreatureFromParty(partyIndex))
        {
            RefreshAllDisplays();
        }
    }

    public void SetActiveCreature(int partyIndex)
    {
        playerInventory.SwitchActiveCreature(partyIndex);
        RefreshActiveCreatureDisplay();
    }

    public void ToggleInventory()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
        if (gameObject.activeInHierarchy)
        {
            RefreshAllDisplays();
        }
    }
}