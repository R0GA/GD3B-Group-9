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

    public void OpenPack(PlayerInventory playerInventory)
    {
        if (Random.value < creatureChance && creaturePool.Count > 0)
        {
            // Give a random creature as data, not as an instance
            var randomCreature = creaturePool[Random.Range(0, creaturePool.Count)];
            var creatureData = new CreatureData(randomCreature);
            playerInventory.AddCreatureData(creatureData);
        }
        else if (itemPool.Count > 0)
        {
            // Give a random item
            var randomItem = itemPool[Random.Range(0, itemPool.Count)];
            playerInventory.AddItem(randomItem);
        }
    }
}