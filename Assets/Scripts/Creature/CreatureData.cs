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
    public List<string> equippedItemNames = new List<string>();

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
        // Populate equippedItemNames as needed
    }
}