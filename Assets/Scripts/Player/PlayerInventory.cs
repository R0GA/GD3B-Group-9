using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerInventory : MonoBehaviour
{
    [Header("Player's Creatures")]
    [SerializeField] private List<CreatureData> creatureDatas = new List<CreatureData>();
    [Header("Player's Items")]
    [SerializeField] private List<ItemBase> items = new List<ItemBase>();

    private CreatureBase activeCreatureInstance;
    private int activeCreatureIndex = 0;

    private Dictionary<ItemBase, int> equippedItems = new Dictionary<ItemBase, int>();

    public IReadOnlyList<CreatureData> CreatureDatas => creatureDatas;
    public IReadOnlyList<ItemBase> Items => items;

    public void AddCreature(CreatureBase creature)
    {
        if (creature != null)
        {
            creatureDatas.Add(new CreatureData(creature));
        }
    }

    public void RemoveCreature(int index)
    {
        if (index >= 0 && index < creatureDatas.Count)
        {
            creatureDatas.RemoveAt(index);
        }
    }

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

    public void AddCreatureData(CreatureData data)
    {
        if (data != null)
        {
            creatureDatas.Add(data);
        }
    }

    // Instantiate the active creature from data
    public CreatureBase GetActiveCreature()
    {
        if (activeCreatureInstance != null)
            return activeCreatureInstance;

        if (creatureDatas.Count == 0)
            return null;

        var data = creatureDatas[activeCreatureIndex];
        var prefab = Resources.Load<CreatureBase>(data.prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Creature prefab '{data.prefabName}' not found in Resources.");
            return null;
        }
        activeCreatureInstance = Instantiate(prefab);
        // Set stats from data
        SetCreatureFromData(activeCreatureInstance, data);
        return activeCreatureInstance;
    }

    // Save the current active creature's state back to data
    public void SaveActiveCreature()
    {
        if (activeCreatureInstance == null) return;
        creatureDatas[activeCreatureIndex] = new CreatureData(activeCreatureInstance);
        Destroy(activeCreatureInstance.gameObject);
        activeCreatureInstance = null;
    }

    // Switch active creature
    public void SwitchActiveCreature(int newIndex)
    {
        if (newIndex < 0 || newIndex >= creatureDatas.Count || newIndex == activeCreatureIndex)
            return;

        SaveActiveCreature();
        activeCreatureIndex = newIndex;
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
        // Equip items as needed
    }

    public bool EquipItemToCreature(int creatureIndex, ItemBase item)
    {
        if (item == null || creatureIndex < 0 || creatureIndex >= creatureDatas.Count)
            return false;

        if (equippedItems.ContainsKey(item))
            return false; // Already equipped elsewhere

        equippedItems[item] = creatureIndex;
        // Optionally, update CreatureData to track equipped items
        creatureDatas[creatureIndex].equippedItemNames.Add(item.ItemName);
        
        
        if (activeCreatureInstance != null)
        {
            // Only now call EquipItem on the instantiated CreatureBase
            activeCreatureInstance.EquipItem(item);
        }
        
        return true;
    }

    public bool DequipItemFromCreature(int creatureIndex, ItemBase item)
    {
        if (item == null || creatureIndex < 0 || creatureIndex >= creatureDatas.Count)
            return false;

        if (equippedItems.TryGetValue(item, out int equippedIndex) && equippedIndex == creatureIndex)
        {
            equippedItems.Remove(item);
            creatureDatas[creatureIndex].equippedItemNames.Remove(item.ItemName);
            return true;
        }
        return false;
    }

    // Helper to check if an item is equipped
    public bool IsItemEquipped(ItemBase item)
    {
        return equippedItems.ContainsKey(item);
    }

    public IEnumerable<ItemBase> GetAvailableItems()
    {
        return items.Where(item => !IsItemEquipped(item));
    }
}