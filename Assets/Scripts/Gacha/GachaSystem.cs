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
            var randomCreaturePrefab = creaturePool[Random.Range(0, creaturePool.Count)];
            // Create an instance to generate unique ID
            var creatureInstance = Instantiate(randomCreaturePrefab);
            creatureInstance.isPlayerCreature = true;
            creatureInstance.GenerateNewID(); // Generate unique ID

            var creatureData = new CreatureData(creatureInstance);
            playerInventory.AddCreatureData(creatureData);

            // Destroy the instance since we only needed it for data
            Destroy(creatureInstance.gameObject);
        }
        else if (itemPool.Count > 0)
        {
            var randomItem = Instantiate(itemPool[Random.Range(0, itemPool.Count)]); // Create instance copy
            randomItem.GenerateNewID(); // Generate unique ID
            playerInventory.AddItem(randomItem);
        }
    }

    // New method for UI animation - returns the result without adding to inventory
    public object GetGachaResult()
    {
        if (Random.value < creatureChance && creaturePool.Count > 0)
        {
            var randomCreaturePrefab = creaturePool[Random.Range(0, creaturePool.Count)];
            // Create an instance to generate unique ID
            var creatureInstance = Instantiate(randomCreaturePrefab);
            creatureInstance.isPlayerCreature = true;
            creatureInstance.GenerateNewID(); // Generate unique ID

            var creatureData = new CreatureData(creatureInstance);

            // Destroy the instance since we only needed it for data
            Destroy(creatureInstance.gameObject);

            return creatureData;
        }
        else if (itemPool.Count > 0)
        {
            var randomItem = Instantiate(itemPool[Random.Range(0, itemPool.Count)]); // Create instance copy
            randomItem.GenerateNewID(); // Generate unique ID
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
            // Create an instance to generate unique ID
            var creatureInstance = Instantiate(creature);
            creatureInstance.isPlayerCreature = true;
            creatureInstance.GenerateNewID(); // Generate unique ID

            var creatureData = new CreatureData(creatureInstance);
            allResults.Add(creatureData);

            // Destroy the instance since we only needed it for data
            Destroy(creatureInstance.gameObject);
        }

        foreach (var item in itemPool)
        {
            var itemCopy = Instantiate(item); // Create instance copy for display
            itemCopy.GenerateNewID(); // Generate unique ID
            allResults.Add(itemCopy);
        }

        return allResults;
    }
}