using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class CreatureBase : MonoBehaviour
{
    [Header("Creature Stats")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float speed;
    [SerializeField] private bool isPlayerCreature;
    [SerializeField] private float playerCreatureModifier;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackDamage;
    //[SerializeField] private int captureChallenge;
    [SerializeField] private ElementType elementType; // For single type
   

    // Example: effectiveness[attacker][defender] = multiplier
    private static readonly Dictionary<ElementType, Dictionary<ElementType, float>> effectiveness =
        new Dictionary<ElementType, Dictionary<ElementType, float>>
        {
            { ElementType.Fire, new Dictionary<ElementType, float> { { ElementType.Grass, 2.0f }, { ElementType.Water, 0.5f } } },
            { ElementType.Water, new Dictionary<ElementType, float> { { ElementType.Fire, 2.0f }, { ElementType.Grass, 0.5f } } },
            { ElementType.Grass, new Dictionary<ElementType, float> { { ElementType.Water, 2.0f }, { ElementType.Fire, 0.5f } } },
            // etc.
        };

    private void Awake()
    {
        if (isPlayerCreature)
        {
            maxHealth *= playerCreatureModifier;
            attackDamage *= playerCreatureModifier;
        }
        health = maxHealth;
    }

    public void EquipItem(ItemBase item)
    {
        health += item.HealthModifier;
        if(item.ElementType == elementType)
        {
            attackDamage += item.DamageModifier;
        }
    }

    public void DequipItem(ItemBase item)
    {
        health -= item.HealthModifier;
        if (item.ElementType == elementType)
        {
            attackDamage -= item.DamageModifier;
        }
    }


    public void TakeDamage(float damage, ElementType attackerType)
    {
        float multiplier = 1.0f;
        if (effectiveness.TryGetValue(attackerType, out var defenderDict))
        {
            if (defenderDict.TryGetValue(elementType, out float value))
            {
                multiplier = value;
            }
        }
        float scaledDamage = damage * multiplier;
        health -= scaledDamage;
        health = Mathf.Clamp(health, 0, maxHealth);
    }
}

public enum ElementType
{
    None,
    Fire,
    Water,
    Grass
}
