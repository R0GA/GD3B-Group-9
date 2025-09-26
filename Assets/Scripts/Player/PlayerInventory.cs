using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Player's Creatures")]
    [SerializeField] private List<CreatureBase> creatures = new List<CreatureBase>();
    [Header("Player's Items")]
    [SerializeField] private List<ItemBase> items = new List<ItemBase>();
        
    public IReadOnlyList<CreatureBase> Creatures => creatures;
    public IReadOnlyList<ItemBase> Items => items;

    // Add a creature to the inventory
    public void AddCreature(CreatureBase creature)
    {
        if (creature != null && !creatures.Contains(creature))
        {
            creatures.Add(creature);
        }
    }

    // Remove a creature from the inventory
    public void RemoveCreature(CreatureBase creature)
    {
        if (creature != null)
        {
            creatures.Remove(creature);
        }
    }

    // Add an item to the inventory
    public void AddItem(ItemBase item)
    {
        if (item != null)
        {
            items.Add(item);
        }
    }

    // Remove an item from the inventory
    public void RemoveItem(ItemBase item)
    {
        if (item != null)
        {
            items.Remove(item);
        }
    }

    // Example: Get the first creature (could be used as "active" creature)
    public CreatureBase GetFirstCreature()
    {
        return creatures.Count > 0 ? creatures[0] : null;
    }
}