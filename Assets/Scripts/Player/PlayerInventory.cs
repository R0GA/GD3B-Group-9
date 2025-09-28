using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerInventory : MonoBehaviour
{
    [Header("Player's Creatures")]
    [SerializeField] private List<CreatureData> mainCreatureInventory = new List<CreatureData>();
    [Header("Player's Party (Max 3)")]
    [SerializeField] private List<CreatureData> partyCreatureInventory = new List<CreatureData>(3);
    [Header("Player's Items")]
    [SerializeField] private List<ItemBase> items = new List<ItemBase>();

    private CreatureBase activeCreatureInstance;
    private int activePartyIndex = 0;

    private Dictionary<ItemBase, int> equippedItems = new Dictionary<ItemBase, int>();

    public IReadOnlyList<CreatureData> MainCreatureInventory => mainCreatureInventory;
    public IReadOnlyList<CreatureData> PartyCreatureInventory => partyCreatureInventory;
    public IReadOnlyList<ItemBase> Items => items;

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

    // Remove creature from main inventory
    public void RemoveCreatureFromMain(int index)
    {
        if (index >= 0 && index < mainCreatureInventory.Count)
        {
            mainCreatureInventory.RemoveAt(index);
        }
    }

    // Party management
    public bool AddCreatureToParty(int mainInventoryIndex)
    {
        if (mainInventoryIndex < 0 || mainInventoryIndex >= mainCreatureInventory.Count)
            return false;
        if (partyCreatureInventory.Count >= 3)
            return false;
        if (partyCreatureInventory.Contains(mainCreatureInventory[mainInventoryIndex]))
            return false;

        partyCreatureInventory.Add(mainCreatureInventory[mainInventoryIndex]);
        return true;
    }

    public bool RemoveCreatureFromParty(int partyIndex)
    {
        if (partyIndex < 0 || partyIndex >= partyCreatureInventory.Count)
            return false;
        partyCreatureInventory.RemoveAt(partyIndex);
        // Adjust activePartyIndex if needed
        if (activePartyIndex >= partyCreatureInventory.Count)
            activePartyIndex = Mathf.Clamp(partyCreatureInventory.Count - 1, 0, 2);
        return true;
    }

    // Instantiate the active creature from party
    public CreatureBase GetActiveCreature()
    {
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
        activeCreatureInstance = Instantiate(prefab);
        SetCreatureFromData(activeCreatureInstance, data);
        return activeCreatureInstance;
    }

    public void SaveActiveCreature()
    {
        if (activeCreatureInstance == null) return;
        partyCreatureInventory[activePartyIndex] = new CreatureData(activeCreatureInstance);
        Destroy(activeCreatureInstance.gameObject);
        activeCreatureInstance = null;
    }

    // Switch active creature in party
    public void SwitchActiveCreature(int newPartyIndex)
    {
        if (newPartyIndex < 0 || newPartyIndex >= partyCreatureInventory.Count || newPartyIndex == activePartyIndex)
            return;

        SaveActiveCreature();
        activePartyIndex = newPartyIndex;
        GetActiveCreature();
    }

    private void SetCreatureFromData(CreatureBase creature, CreatureData data)
    {
        creature.health = data.health;
        creature.maxHealth = data.maxHealth;
        creature.speed = data.speed;
        creature.attackSpeed = data.attackSpeed;
        creature.attackDamage = data.attackDamage;
        creature.elementType = data.elementType;
        creature.level = data.level;
        creature.icon = data.icon;  
        // Equip items as needed
    }

    // Item management remains unchanged
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

    public bool EquipItemToCreature(int partyIndex, ItemBase item)
    {
        if (item == null || partyIndex < 0 || partyIndex >= partyCreatureInventory.Count)
            return false;

        if (equippedItems.ContainsKey(item))
            return false; // Already equipped elsewhere

        equippedItems[item] = partyIndex;
        partyCreatureInventory[partyIndex].equippedItemNames.Add(item.ItemName);

        if (activeCreatureInstance != null && activePartyIndex == partyIndex)
        {
            activeCreatureInstance.EquipItem(item);
        }

        return true;
    }

    public bool DequipItemFromCreature(int partyIndex, ItemBase item)
    {
        if (item == null || partyIndex < 0 || partyIndex >= partyCreatureInventory.Count)
            return false;

        if (equippedItems.TryGetValue(item, out int equippedIndex) && equippedIndex == partyIndex)
        {
            equippedItems.Remove(item);
            partyCreatureInventory[partyIndex].equippedItemNames.Remove(item.ItemName);
            return true;
        }
        return false;
    }

    public bool IsItemEquipped(ItemBase item)
    {
        return equippedItems.ContainsKey(item);
    }

    public IEnumerable<ItemBase> GetAvailableItems()
    {
        return items.Where(item => !IsItemEquipped(item));
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
}