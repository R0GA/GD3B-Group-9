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

    public CreatureData(CreatureBase creature)
    {
        prefabName = "Creatures/" + creature.gameObject.name.Replace("(Clone)", "").Trim();
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

        // Get equipped item ID if any
        var equippedItem = creature.GetEquippedItem();
        if (equippedItem != null)
        {
            equippedItemIDs.Clear();
            equippedItemIDs.Add(equippedItem.ItemID);
        }
    }
}