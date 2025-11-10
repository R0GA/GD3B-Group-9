using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CreatureData
{
    public string prefabName;
    public float health;
    public float maxHealth;
    public float speed;
    public float attackSpeed;
    public float attackDamage;
    public int level;
    public int currentXP;
    public int xpToNextLevel;
    public Sprite icon;
    public ElementType elementType;
    public List<string> equippedItemIDs = new List<string>();
    public string creatureID; // Unique ID for this creature instance
    public bool isPlayerCreature;
    public bool hasEvolved;
    public string evolvedCreatureName;

    public CreatureData(CreatureBase creature)
    {
        prefabName = "Creatures/" + creature.basePrefabName;
        health = creature.health;
        maxHealth = creature.maxHealth;
        speed = creature.speed;
        attackSpeed = creature.attackSpeed;
        attackDamage = creature.attackDamage;
        elementType = creature.elementType;
        level = creature.level;
        currentXP = creature.currentXP;
        xpToNextLevel = creature.xpToNextLevel;
        icon = creature.icon;
        creatureID = creature.CreatureID;
        isPlayerCreature = creature.isPlayerCreature;
        hasEvolved = creature.hasEvolved; // Store evolution state
        evolvedCreatureName = creature.evolvedCreatureName; // Store evolved name

        // Get equipped item ID if any
        var equippedItem = creature.GetEquippedItem();
        if (equippedItem != null)
        {
            equippedItemIDs.Clear();
            equippedItemIDs.Add(equippedItem.ItemID);
        }
    }

    public string DisplayName
    {
        get
        {
            // If creature has evolved and has an evolved name, use that
            if (hasEvolved && !string.IsNullOrEmpty(evolvedCreatureName))
            {
                return evolvedCreatureName;
            }
            // Otherwise use the base name from prefab
            return prefabName.Replace("Creatures/", "");
        }
    }

    [System.Serializable]
    public class PendingReward
    {
        public int pendingXP = 0;
        public bool needsHealing = false;
    }

    public PendingReward pendingReward = new PendingReward();
}

