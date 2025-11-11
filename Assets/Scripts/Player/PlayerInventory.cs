using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Player's Creatures")]
    [SerializeField] private List<CreatureData> mainCreatureInventory = new List<CreatureData>();
    [Header("Player's Party (Max 3)")]
    [SerializeField] private List<CreatureData> partyCreatureInventory = new List<CreatureData>(3);
    [Header("Player's Items")]
    [SerializeField] private List<ItemBase> items = new List<ItemBase>();

    private CreatureBase activeCreatureInstance;
    private int activePartyIndex = 0;
    private Transform playerTransform;

    // Track equipped items: ItemID -> Party Index
    private Dictionary<string, int> equippedItems = new Dictionary<string, int>();

    public IReadOnlyList<CreatureData> MainCreatureInventory => mainCreatureInventory;
    public IReadOnlyList<CreatureData> PartyCreatureInventory => partyCreatureInventory;
    public IReadOnlyList<ItemBase> Items => items;

    public int ActivePartyIndex => activePartyIndex;

    public System.Action<CreatureBase> OnActiveCreatureStatsChanged;


    private void Awake()
    {
        // Singleton pattern: ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Find the player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Start()
    {
        // Subscribe to creature events for real-time updates
        if (activeCreatureInstance != null)
        {
            SubscribeToCreatureEvents(activeCreatureInstance);
        }
    }

    private void SubscribeToCreatureEvents(CreatureBase creature)
    {
        if (creature == null) return;

        // Unsubscribe first to avoid duplicates
        UnsubscribeFromCreatureEvents(creature);

        // Subscribe to damage and healing events
        // Note: You'll need to add these events to CreatureBase if they don't exist
        creature.OnDamageTaken += OnCreatureStatsChanged;
        creature.OnHealed += OnCreatureStatsChanged;
        creature.OnLevelUp += OnCreatureStatsChanged;
    }

    private void UnsubscribeFromCreatureEvents(CreatureBase creature)
    {
        if (creature == null) return;

        creature.OnDamageTaken -= OnCreatureStatsChanged;
        creature.OnHealed -= OnCreatureStatsChanged;
        creature.OnLevelUp -= OnCreatureStatsChanged;
    }

    private void OnCreatureStatsChanged(CreatureBase creature)
    {
        // Notify UI to refresh
        OnActiveCreatureStatsChanged?.Invoke(creature);

        // Refresh hotbar
        HotbarUIManager.Instance?.RefreshHotbar();

        // Refresh inventory UI if it's open
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.gameObject.activeInHierarchy)
        {
            InventoryUIManager.Instance.RefreshActiveCreatureDisplayOnly();
        }
    }

    private void OnCreatureStatsChanged(CreatureBase creature, int newLevel)
    {
        OnCreatureStatsChanged(creature);
    }

    // Helper method to get creature by ID
    private CreatureData GetCreatureByID(string creatureID)
    {
        // Check party first
        var partyCreature = partyCreatureInventory.Find(c => c.creatureID == creatureID);
        if (partyCreature != null) return partyCreature;

        // Check main inventory
        return mainCreatureInventory.Find(c => c.creatureID == creatureID);
    }

    // Helper method to get creature index in party by ID
    private int GetPartyIndexByCreatureID(string creatureID)
    {
        for (int i = 0; i < partyCreatureInventory.Count; i++)
        {
            if (partyCreatureInventory[i].creatureID == creatureID)
                return i;
        }
        return -1;
    }

    // Add creature to main inventory (from gacha)
    public void AddCreature(CreatureBase creature)
    {
        if (creature != null)
        {
            mainCreatureInventory.Add(new CreatureData(creature));
        }
    }

    public void AddCreatureData(CreatureData data)
    {
        if (data != null)
        {
            mainCreatureInventory.Add(data);
        }
    }

    // Remove creature from main inventory by ID
    public void RemoveCreatureFromMainByID(string creatureID)
    {
        var creature = mainCreatureInventory.Find(c => c.creatureID == creatureID);
        if (creature != null)
        {
            mainCreatureInventory.Remove(creature);
        }
    }

    // Remove creature from main inventory by index
    public void RemoveCreatureFromMain(int index)
    {
        if (index >= 0 && index < mainCreatureInventory.Count)
        {
            mainCreatureInventory.RemoveAt(index);
        }
    }

    // Party management - MOVES creature from main to party by creature ID
    public bool AddCreatureToPartyByID(string creatureID)
    {
        if (partyCreatureInventory.Count >= 3)
            return false;

        // Find the creature in main inventory
        var creatureToMove = mainCreatureInventory.Find(c => c.creatureID == creatureID);
        if (creatureToMove == null)
            return false;

        // Remove from main inventory
        mainCreatureInventory.Remove(creatureToMove);

        // Add to party
        partyCreatureInventory.Add(creatureToMove);

        // If party was empty, set this as active and instantiate it
        if (partyCreatureInventory.Count == 1) // First creature added
        {
            activePartyIndex = 0;
            GetActiveCreature(); // This will instantiate the creature
        }

        return true;
    }

    // Party management - MOVES creature from main to party by index
    public bool AddCreatureToParty(int mainInventoryIndex)
    {
        if (mainInventoryIndex < 0 || mainInventoryIndex >= mainCreatureInventory.Count)
            return false;
        if (partyCreatureInventory.Count >= 3)
            return false;

        // Check if party was empty before adding
        bool wasPartyEmpty = partyCreatureInventory.Count == 0;

        // Get the creature data from main inventory
        CreatureData creatureToMove = mainCreatureInventory[mainInventoryIndex];

        // Remove from main inventory
        mainCreatureInventory.RemoveAt(mainInventoryIndex);

        // Add to party
        partyCreatureInventory.Add(creatureToMove);

        // If party was empty, set this as active and instantiate it
        if (wasPartyEmpty)
        {
            activePartyIndex = partyCreatureInventory.Count - 1;
            GetActiveCreature(); // This will instantiate the creature
        }

        return true;
    }

    // Remove creature from party by ID - MOVES it back to main inventory
    public bool RemoveCreatureFromPartyByID(string creatureID)
    {
        int partyIndex = GetPartyIndexByCreatureID(creatureID);
        if (partyIndex == -1)
            return false;

        return RemoveCreatureFromParty(partyIndex);
    }

    // Remove creature from party by index - MOVES it back to main inventory
    public bool RemoveCreatureFromParty(int partyIndex)
    {
        if (partyIndex < 0 || partyIndex >= partyCreatureInventory.Count)
            return false;

        // Get the creature data from party
        CreatureData creatureToMove = partyCreatureInventory[partyIndex];

        // If we're removing the active creature, save its current state first
        if (activeCreatureInstance != null && partyIndex == activePartyIndex)
        {
            // Update the creature data with the current state of the active instance
            creatureToMove = new CreatureData(activeCreatureInstance);

            // Clear any pending rewards since we're saving the current state
            creatureToMove.pendingReward.pendingXP = 0;
            creatureToMove.pendingReward.needsHealing = false;

            // Destroy the active instance
            Destroy(activeCreatureInstance.gameObject);
            activeCreatureInstance = null;
        }

        // Remove from party
        partyCreatureInventory.RemoveAt(partyIndex);

        // Add back to main inventory
        mainCreatureInventory.Add(creatureToMove);

        // Adjust activePartyIndex if needed
        if (activePartyIndex >= partyCreatureInventory.Count)
            activePartyIndex = Mathf.Clamp(partyCreatureInventory.Count - 1, 0, 2);

        return true;
    }

    // Direct swap between party slots (optional but useful)
    public bool SwapPartyCreatures(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= partyCreatureInventory.Count ||
            indexB < 0 || indexB >= partyCreatureInventory.Count)
            return false;

        var temp = partyCreatureInventory[indexA];
        partyCreatureInventory[indexA] = partyCreatureInventory[indexB];
        partyCreatureInventory[indexB] = temp;

        // Update active index if swapped with active creature
        if (indexA == activePartyIndex)
            activePartyIndex = indexB;
        else if (indexB == activePartyIndex)
            activePartyIndex = indexA;

        return true;
    }

    // Instantiate the active creature from party
    public CreatureBase GetActiveCreature()
    {
        // Don't spawn if active creature is dead
        if (ActivePartyCreatureData != null && ActivePartyCreatureData.health <= 0)
        {
            Debug.Log("Active creature is fainted and cannot be spawned!");
            return null;
        }

        if (activeCreatureInstance != null)
            return activeCreatureInstance;

        if (partyCreatureInventory.Count == 0)
            return null;

        var data = partyCreatureInventory[activePartyIndex];
        var prefab = Resources.Load<CreatureBase>(data.prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Creature prefab '{data.prefabName}' not found in Resources.");
            return null;
        }

        // Spawn near player
        Vector3 spawnPosition = GetSpawnPositionNearPlayer();
        activeCreatureInstance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // Set up the creature controller to follow player
        CreatureController creatureController = activeCreatureInstance.GetComponent<CreatureController>();
        if (creatureController != null && playerTransform != null)
        {
            creatureController.SetReturnPosition(spawnPosition);
        }

        SetCreatureFromData(activeCreatureInstance, data);

        // Subscribe to events for real-time updates
        SubscribeToCreatureEvents(activeCreatureInstance);

        return activeCreatureInstance;
    }

    // Update SwitchActiveCreature to handle event subscriptions
    public void SwitchActiveCreature(int newPartyIndex)
    {
        if (newPartyIndex < 0 || newPartyIndex >= partyCreatureInventory.Count || newPartyIndex == activePartyIndex)
            return;

        // Unsubscribe from current creature events
        if (activeCreatureInstance != null)
        {
            UnsubscribeFromCreatureEvents(activeCreatureInstance);
        }

        SaveActiveCreature();
        activePartyIndex = newPartyIndex;
        GetActiveCreature(); // This will subscribe to new creature events
    }

    // Update SaveActiveCreature to unsubscribe from events
    public void SaveActiveCreature()
    {
        if (activeCreatureInstance == null) return;

        // Clear any pending rewards for this creature since we're saving its current state
        if (partyCreatureInventory.Count > activePartyIndex)
        {
            partyCreatureInventory[activePartyIndex].pendingReward.pendingXP = 0;
            partyCreatureInventory[activePartyIndex].pendingReward.needsHealing = false;
        }

        partyCreatureInventory[activePartyIndex] = new CreatureData(activeCreatureInstance);
        Destroy(activeCreatureInstance.gameObject);
        activeCreatureInstance = null;
    }

    private Vector3 GetSpawnPositionNearPlayer()
    {
        if (playerTransform == null)
        {
            // Fallback to near the inventory object if player not found
            return transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        }

        // Calculate position around player with some randomness
        Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        Vector3 spawnPosition = playerTransform.position + randomOffset;

        // Ensure the position is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPosition, out hit, 3f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If NavMesh sampling fails, return the original position
        return spawnPosition;
    }


    // Switch active creature in party by creature ID
    public void SwitchActiveCreatureByID(string creatureID)
    {
        int newPartyIndex = GetPartyIndexByCreatureID(creatureID);
        if (newPartyIndex == -1 || newPartyIndex == activePartyIndex)
            return;

        SwitchActiveCreature(newPartyIndex);
    }

    private void SetCreatureFromData(CreatureBase creature, CreatureData data)
    {
        creature.SetID(data.creatureID);
        creature.maxHealth = data.maxHealth;
        creature.speed = data.speed;
        creature.attackSpeed = data.attackSpeed;
        creature.attackDamage = data.attackDamage;
        creature.elementType = data.elementType;
        creature.level = data.level;
        creature.currentXP = data.currentXP;
        creature.xpToNextLevel = data.xpToNextLevel;
        creature.icon = data.icon;
        creature.isPlayerCreature = data.isPlayerCreature;
        creature.health = data.health;

        // Apply evolution state
        if (data.hasEvolved)
        {
            creature.Evolve();
        }

        // Apply pending rewards if any
        if (data.pendingReward != null)
        {
            if (data.pendingReward.pendingXP > 0)
            {
                creature.GainXP(data.pendingReward.pendingXP);
                data.pendingReward.pendingXP = 0; // Clear pending XP
            }

            if (data.pendingReward.needsHealing)
            {
                creature.FullHeal();
                data.pendingReward.needsHealing = false; // Clear healing flag
            }
        }

        // Re-equip any items that were equipped to this creature using ItemID
        foreach (var itemID in data.equippedItemIDs)
        {
            var item = GetItemByID(itemID);
            if (item != null)
            {
                creature.EquipItem(item);
                equippedItems[itemID] = activePartyIndex;
            }
        }
    }

    public void HandleCreatureDeath(CreatureBase deadCreature)
    {
        if (deadCreature.isPlayerCreature)
        {
            // Save the dead creature state back to party
            if (activeCreatureInstance == deadCreature)
            {
                SaveActiveCreature();

                // Clear the active creature instance since it's dead
                activeCreatureInstance = null;
            }

            // Update the party data with 0 HP using creature ID
            for (int i = 0; i < partyCreatureInventory.Count; i++)
            {
                if (partyCreatureInventory[i].creatureID == deadCreature.CreatureID)
                {
                    partyCreatureInventory[i].health = 0;
                    break;
                }
            }
        }
    }

    // Item management
    public void AddItem(ItemBase item)
    {
        if (item != null)
        {
            items.Add(item);
        }
    }

    public void RemoveItem(ItemBase item)
    {
        if (item != null)
        {
            items.Remove(item);
        }
    }

    // Helper method to get item by ID
    private ItemBase GetItemByID(string itemID)
    {
        return items.Find(item => item.ItemID == itemID);
    }

    // Equip item to specific party creature by index
    public bool EquipItemToCreature(int partyIndex, ItemBase item)
    {
        if (item == null || partyIndex < 0 || partyIndex >= partyCreatureInventory.Count)
            return false;

        // Check if item is already equipped to another creature
        if (equippedItems.ContainsKey(item.ItemID))
        {
            Debug.LogWarning($"Item {item.ItemName} (ID: {item.ItemID}) is already equipped to another creature!");
            return false;
        }

        // Check if creature already has an item equipped
        var currentEquippedItem = GetEquippedItemForCreature(partyIndex);
        if (currentEquippedItem != null)
        {
            Debug.LogWarning($"Creature {partyCreatureInventory[partyIndex].creatureID} already has {currentEquippedItem.ItemName} equipped!");
            return false;
        }

        // Equip the item
        equippedItems[item.ItemID] = partyIndex;
        partyCreatureInventory[partyIndex].equippedItemIDs.Clear(); // Clear previous items (should only be one)
        partyCreatureInventory[partyIndex].equippedItemIDs.Add(item.ItemID);

        // Apply to active creature instance if it's the same party index
        if (activeCreatureInstance != null && activePartyIndex == partyIndex)
        {
            activeCreatureInstance.EquipItem(item);
        }

        Debug.Log($"Equipped {item.ItemName} (ID: {item.ItemID}) to creature {partyCreatureInventory[partyIndex].creatureID}");
        return true;
    }

    // Equip item to specific party creature by creature ID
    public bool EquipItemToCreatureByID(string creatureID, ItemBase item)
    {
        int partyIndex = GetPartyIndexByCreatureID(creatureID);
        if (partyIndex == -1)
            return false;

        return EquipItemToCreature(partyIndex, item);
    }

    // Equip item to active creature
    public bool EquipItemToActiveCreature(ItemBase item)
    {
        return EquipItemToCreature(activePartyIndex, item);
    }

    // Dequip item from specific party creature by index
    public bool DequipItemFromCreature(int partyIndex, ItemBase item)
    {
        if (item == null || partyIndex < 0 || partyIndex >= partyCreatureInventory.Count)
            return false;

        if (equippedItems.TryGetValue(item.ItemID, out int equippedIndex) && equippedIndex == partyIndex)
        {
            equippedItems.Remove(item.ItemID);
            partyCreatureInventory[partyIndex].equippedItemIDs.Remove(item.ItemID);

            if (activeCreatureInstance != null && activePartyIndex == partyIndex)
            {
                activeCreatureInstance.DequipItem(item);
            }

            Debug.Log($"Dequipped {item.ItemName} (ID: {item.ItemID}) from creature {partyCreatureInventory[partyIndex].creatureID}");
            return true;
        }
        return false;
    }

    // Dequip item from specific party creature by creature ID
    public bool DequipItemFromCreatureByID(string creatureID, ItemBase item)
    {
        int partyIndex = GetPartyIndexByCreatureID(creatureID);
        if (partyIndex == -1)
            return false;

        return DequipItemFromCreature(partyIndex, item);
    }

    // Dequip item from active creature
    public bool DequipItemFromActiveCreature(ItemBase item)
    {
        return DequipItemFromCreature(activePartyIndex, item);
    }

    // Dequip whatever item is equipped to a creature by index
    public bool DequipItemFromCreature(int partyIndex)
    {
        var item = GetEquippedItemForCreature(partyIndex);
        if (item != null)
        {
            return DequipItemFromCreature(partyIndex, item);
        }
        return false;
    }

    // Dequip whatever item is equipped to a creature by ID
    public bool DequipItemFromCreatureByID(string creatureID)
    {
        int partyIndex = GetPartyIndexByCreatureID(creatureID);
        if (partyIndex == -1)
            return false;

        return DequipItemFromCreature(partyIndex);
    }

    // Get the item equipped to a specific creature by index
    public ItemBase GetEquippedItemForCreature(int partyIndex)
    {
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value == partyIndex)
                return GetItemByID(kvp.Key);
        }
        return null;
    }

    // Get the item equipped to a specific creature by ID
    public ItemBase GetEquippedItemForCreatureByID(string creatureID)
    {
        int partyIndex = GetPartyIndexByCreatureID(creatureID);
        if (partyIndex == -1)
            return null;

        return GetEquippedItemForCreature(partyIndex);
    }

    // Get the item equipped to the active creature
    public ItemBase GetEquippedItemForActiveCreature()
    {
        return GetEquippedItemForCreature(activePartyIndex);
    }

    public bool IsItemEquipped(ItemBase item)
    {
        return equippedItems.ContainsKey(item.ItemID);
    }

    public IEnumerable<ItemBase> GetAvailableItems()
    {
        return items.Where(item => !IsItemEquipped(item));
    }

    public IEnumerable<ItemBase> GetEquippedItems()
    {
        return equippedItems.Keys.Select(itemID => GetItemByID(itemID));
    }

    public CreatureData ActivePartyCreatureData
    {
        get
        {
            if (partyCreatureInventory.Count == 0 || activePartyIndex < 0 || activePartyIndex >= partyCreatureInventory.Count)
                return null;
            return partyCreatureInventory[activePartyIndex];
        }
    }
    public void GrantTrialRewardsToParty(int xpReward)
    {
        // Grant Gacha Pack
        GachaUIManager.Instance.AddPacks(5);

        // Heal the player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.FullHeal();
        }

        // Grant rewards to all party creatures (both active and reserve)
        for (int i = 0; i < partyCreatureInventory.Count; i++)
        {
            if (i == activePartyIndex && activeCreatureInstance != null)
            {
                // Active creature - apply rewards immediately
                activeCreatureInstance.FullHeal();
                activeCreatureInstance.GainXP(xpReward);

                // Update the party data to reflect healing
                partyCreatureInventory[i].health = activeCreatureInstance.maxHealth;
            }
            else
            {
                // Reserve creature - store rewards for when they're spawned
                partyCreatureInventory[i].pendingReward.pendingXP += xpReward;
                partyCreatureInventory[i].pendingReward.needsHealing = true;
                partyCreatureInventory[i].health = partyCreatureInventory[i].maxHealth; // Heal immediately
            }
        }

        // Refresh UI to show updated stats
        HotbarUIManager.Instance?.RefreshHotbar();
        OnActiveCreatureStatsChanged?.Invoke(activeCreatureInstance);

        Debug.Log($"Granted {xpReward} XP to all party creatures and fully healed everyone!");
    }

    public void GrantXPToAllPartyCreatures(int xpAmount)
    {
        // Grant XP to all party creatures (both active and reserve)
        for (int i = 0; i < partyCreatureInventory.Count; i++)
        {
            if (i == activePartyIndex && activeCreatureInstance != null)
            {
                // Active creature - apply immediately
                activeCreatureInstance.GainXP(xpAmount);
            }
            else
            {
                // Reserve creature - store rewards for when they're spawned
                partyCreatureInventory[i].pendingReward.pendingXP += xpAmount;
            }
        }

        Debug.Log($"Granted {xpAmount} XP to all party creatures!");
    }

    public void HealParty()
    {
        // Heal the player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.FullHeal();
        }

        // Heal all party creatures
        for (int i = 0; i < partyCreatureInventory.Count; i++)
        {
            if (i == activePartyIndex && activeCreatureInstance != null)
            {
                // Active creature - heal immediately
                activeCreatureInstance.FullHeal();
                partyCreatureInventory[i].health = activeCreatureInstance.maxHealth;
            }
            else
            {
                // Reserve creature - mark for healing
                partyCreatureInventory[i].pendingReward.needsHealing = true;
                partyCreatureInventory[i].health = partyCreatureInventory[i].maxHealth;
            }
        }

        Debug.Log("Party fully healed!");
    }
    public void UpdateCreatureDataAfterEvolution(CreatureBase evolvedCreature)
    {
        // Update the stored data for the evolved creature
        for (int i = 0; i < partyCreatureInventory.Count; i++)
        {
            if (partyCreatureInventory[i].creatureID == evolvedCreature.CreatureID)
            {
                partyCreatureInventory[i] = new CreatureData(evolvedCreature);
                break;
            }
        }

        // Also update in main inventory if it's there
        for (int i = 0; i < mainCreatureInventory.Count; i++)
        {
            if (mainCreatureInventory[i].creatureID == evolvedCreature.CreatureID)
            {
                mainCreatureInventory[i] = new CreatureData(evolvedCreature);
                break;
            }
        }

        // Refresh UI
        RefreshUIAfterEvolution();
    }

    private void RefreshUIAfterEvolution()
    {
        // Refresh hotbar
        HotbarUIManager.Instance?.RefreshHotbar();

        // Refresh inventory UI if it's open
        if (InventoryUIManager.Instance != null && InventoryUIManager.Instance.gameObject.activeInHierarchy)
        {
            InventoryUIManager.Instance.RefreshAllDisplays();
        }

        // Notify about stats change
        OnActiveCreatureStatsChanged?.Invoke(GetActiveCreature());
    }
    public void ForceSaveActiveCreature()
    {
        if (activeCreatureInstance != null && activePartyIndex >= 0 && activePartyIndex < partyCreatureInventory.Count)
        {
            partyCreatureInventory[activePartyIndex] = new CreatureData(activeCreatureInstance);

            // Clear pending rewards since we're saving current state
            partyCreatureInventory[activePartyIndex].pendingReward.pendingXP = 0;
            partyCreatureInventory[activePartyIndex].pendingReward.needsHealing = false;
        }
    }
}