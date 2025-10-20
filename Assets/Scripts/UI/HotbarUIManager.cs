using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class HotbarUIManager : MonoBehaviour
{
    public static HotbarUIManager Instance;

    [Header("Hotbar UI References")]
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private HotbarSlotUI[] hotbarSlots = new HotbarSlotUI[3];
    [SerializeField] private TextMeshProUGUI switchHintText;

    private PlayerInventory playerInventory;
    private InventoryInput inventoryInput;

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
        playerInventory = PlayerInventory.Instance;
        inventoryInput = FindObjectOfType<InventoryInput>();

        // Initialize hotbar slots
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
            {
                hotbarSlots[i].Initialize(i);
            }
        }

        // Subscribe to inventory events for real-time updates
        if (playerInventory != null)
        {
            playerInventory.OnActiveCreatureStatsChanged += OnActiveCreatureStatsChanged;
        }

        RefreshHotbar();

        // Set up switch hint text
        if (switchHintText != null)
        {
            switchHintText.text = "Press 1, 2, 3 to switch creatures";
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerInventory != null)
        {
            playerInventory.OnActiveCreatureStatsChanged -= OnActiveCreatureStatsChanged;
        }
    }

    private void OnActiveCreatureStatsChanged(CreatureBase creature)
    {
        // Refresh hotbar when creature stats change
        RefreshHotbar();
    }

    private void Update()
    {
        // Handle number key input for switching creatures
        HandleHotbarInput();
    }

    private void HandleHotbarInput()
    {
        // Check for 1, 2, 3 key presses
        for (int i = 0; i < 3; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchToCreature(i);
            }
        }
    }

    public void SwitchToCreature(int slotIndex)
    {
        if (playerInventory == null) return;

        // Check if the slot index is valid and has a creature
        if (slotIndex >= 0 && slotIndex < playerInventory.PartyCreatureInventory.Count)
        {
            playerInventory.SwitchActiveCreature(slotIndex);
            RefreshHotbar();
        }
    }

    public void RefreshHotbar()
    {
        if (playerInventory == null) return;

        var party = playerInventory.PartyCreatureInventory;
        int activeIndex = playerInventory.ActivePartyIndex;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
            {
                if (i < party.Count)
                {
                    // For active creature, try to get live data from spawned instance
                    if (i == activeIndex && playerInventory.GetActiveCreature() != null)
                    {
                        // Use live creature data for active creature
                        CreatureBase liveCreature = playerInventory.GetActiveCreature();
                        CreatureData liveData = new CreatureData(liveCreature);
                        hotbarSlots[i].SetCreatureData(liveData, true);
                    }
                    else
                    {
                        // Use saved data for inactive creatures
                        hotbarSlots[i].SetCreatureData(party[i], i == activeIndex);
                    }
                }
                else
                {
                    hotbarSlots[i].SetEmpty(i == activeIndex);
                }
            }
        }
    }

    public void SetHotbarVisible(bool visible)
    {
        if (hotbarPanel != null)
        {
            hotbarPanel.SetActive(visible);
        }
    }

    // Call this when inventory or gacha UI is opened/closed
    public void OnInventoryStateChanged(bool inventoryOpen)
    {
        SetHotbarVisible(!inventoryOpen);
    }
}