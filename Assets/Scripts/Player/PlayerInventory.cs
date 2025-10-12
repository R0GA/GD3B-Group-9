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

    // Party management - MOVES creature from main to party
    public bool AddCreatureToParty(int mainInventoryIndex)
    {
        if (mainInventoryIndex < 0 || mainInventoryIndex >= mainCreatureInventory.Count)
            return false;
        if (partyCreatureInventory.Count >= 3)
            return false;

        // Get the creature data from main inventory
        CreatureData creatureToMove = mainCreatureInventory[mainInventoryIndex];

        // Remove from main inventory
        mainCreatureInventory.RemoveAt(mainInventoryIndex);

        // Add to party
        partyCreatureInventory.Add(creatureToMove);
        return true;
    }

    // Remove creature from party - MOVES it back to main inventory
    public bool RemoveCreatureFromParty(int partyIndex)
    {
        if (partyIndex < 0 || partyIndex >= partyCreatureInventory.Count)
            return false;

        // Get the creature data from party
        CreatureData creatureToMove = partyCreatureInventory[partyIndex];

        // Remove from party
        partyCreatureInventory.RemoveAt(partyIndex);

        // Add back to main inventory
        mainCreatureInventory.Add(creatureToMove);

        // Adjust activePartyIndex if needed
        if (activePartyIndex >= partyCreatureInventory.Count)
            activePartyIndex = Mathf.Clamp(partyCreatureInventory.Count - 1, 0, 2);

        // If we removed the active creature, destroy its instance
        if (activeCreatureInstance != null && partyIndex == activePartyIndex)
        {
            Destroy(activeCreatureInstance.gameObject);
            activeCreatureInstance = null;
        }

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
        creature.currentXP = data.currentXP;
        creature.xpToNextLevel = data.xpToNextLevel;
        creature.icon = data.icon;

        // Re-equip any items that were equipped to this creature
        foreach (var itemName in data.equippedItemNames)
        {
            var item = items.Find(i => i.ItemName == itemName);
            if (item != null)
            {
                creature.EquipItem(item);
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

            // Update the party data with 0 HP
            for (int i = 0; i < partyCreatureInventory.Count; i++)
            {
                if (partyCreatureInventory[i].prefabName.Contains(deadCreature.gameObject.name.Replace("(Clone)", "").Trim()))
                {
                    partyCreatureInventory[i].health = 0;
                    break;
                }
            }
        }
    }

    // Item management remains mostly the same
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

            if (activeCreatureInstance != null && activePartyIndex == partyIndex)
            {
                activeCreatureInstance.DequipItem(item);
            }
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