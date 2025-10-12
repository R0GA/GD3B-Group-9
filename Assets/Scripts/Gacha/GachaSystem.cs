using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    [Header("Possible Creatures")]
    [SerializeField] private List<CreatureBase> creaturePool;
    [Header("Possible Items")]
    [SerializeField] private List<ItemBase> itemPool;

    [Tooltip("Chance to get a creature (0-1). 0.5 = 50%")]
    [Range(0f, 1f)]
    [SerializeField] private float creatureChance = 0.5f;

    // Method for direct opening (existing functionality)
    public void OpenPack(PlayerInventory playerInventory)
    {
        if (Random.value < creatureChance && creaturePool.Count > 0)
        {
            var randomCreature = creaturePool[Random.Range(0, creaturePool.Count)];
            randomCreature.isPlayerCreature = true; // Ensure it's flagged as a player creature
            var creatureData = new CreatureData(randomCreature);
            playerInventory.AddCreatureData(creatureData);
        }
        else if (itemPool.Count > 0)
        {
            var randomItem = itemPool[Random.Range(0, itemPool.Count)];
            playerInventory.AddItem(randomItem);
        }
    }

    // New method for UI animation - returns the result without adding to inventory
    public object GetGachaResult()
    {
        if (Random.value < creatureChance && creaturePool.Count > 0)
        {
            var randomCreature = creaturePool[Random.Range(0, creaturePool.Count)];
            randomCreature.isPlayerCreature = true; // Ensure it's flagged as a player creature
            return new CreatureData(randomCreature);
        }
        else if (itemPool.Count > 0)
        {
            var randomItem = itemPool[Random.Range(0, itemPool.Count)];
            return randomItem;
        }
        return null;
    }

    // Get all possible results for the rolling animation
    public List<object> GetAllPossibleResults()
    {
        List<object> allResults = new List<object>();

        foreach (var creature in creaturePool)
        {
            creature.isPlayerCreature = true; // Ensure all are flagged as player creatures
            allResults.Add(new CreatureData(creature));
        }

        foreach (var item in itemPool)
        {
            allResults.Add(item);
        }

        return allResults;
    }
}