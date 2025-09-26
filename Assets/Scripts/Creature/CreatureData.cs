using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CreatureData
{
    public string prefabName; // Or a reference to the prefab if needed
    public float health;
    public float maxHealth;
    public float speed;
    public float attackSpeed;
    public float attackDamage;
    public ElementType elementType;
    public List<string> equippedItemNames = new List<string>(); // Store item names or IDs

    // Add other fields as needed

    public CreatureData(CreatureBase creature)
    {
        prefabName = "Creatures/" + creature.gameObject.name.Replace("(Clone)", "").Trim();
        health = GetPrivateField<float>(creature, "health");
        maxHealth = GetPrivateField<float>(creature, "maxHealth");
        speed = GetPrivateField<float>(creature, "speed");
        attackSpeed = GetPrivateField<float>(creature, "attackSpeed");
        attackDamage = GetPrivateField<float>(creature, "attackDamage");
        elementType = GetPrivateField<ElementType>(creature, "elementType");
        // Populate equippedItemNames as needed
    }

    // Helper to get private fields via reflection
    private T GetPrivateField<T>(CreatureBase creature, string fieldName)
    {
        var field = typeof(CreatureBase).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)field.GetValue(creature);
    }
}