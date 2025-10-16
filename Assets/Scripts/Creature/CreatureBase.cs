using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.UI;

public class CreatureBase : MonoBehaviour
{
    [Header("Creature Stats")]
    public float health;
    public float maxHealth;
    public float speed;
    public bool isPlayerCreature;
    public float attackSpeed;
    public float attackDamage;
    [SerializeField] public ElementType elementType;
    public Sprite icon;

    [Header("Level System")]
    [SerializeField] public int level = 1;
    [SerializeField] public int currentXP = 0;
    [SerializeField] public int xpToNextLevel = 100;
    [SerializeField] private AnimationCurve xpCurve = new AnimationCurve(new Keyframe(1, 50), new Keyframe(30, 800));
    [SerializeField] private AnimationCurve statGrowthCurve = new AnimationCurve(new Keyframe(1, 1.0f), new Keyframe(30, 3.0f));
    float playerCreatureModifier = 5;

    [Header("Base Stats (Level 1)")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseAttackDamage = 10f;

    // Example: effectiveness[attacker][defender] = multiplier
    private static readonly Dictionary<ElementType, Dictionary<ElementType, float>> effectiveness =
        new Dictionary<ElementType, Dictionary<ElementType, float>>
        {
            { ElementType.Fire, new Dictionary<ElementType, float> { { ElementType.Grass, 2.0f }, { ElementType.Water, 0.5f } } },
            { ElementType.Water, new Dictionary<ElementType, float> { { ElementType.Fire, 2.0f }, { ElementType.Grass, 0.5f } } },
            { ElementType.Grass, new Dictionary<ElementType, float> { { ElementType.Water, 2.0f }, { ElementType.Fire, 0.5f } } },
            { ElementType.None, new Dictionary<ElementType, float>{ { ElementType.Fire, 1.0f }, { ElementType.Water, 1.0f }, { ElementType.Grass, 1.0f }, { ElementType.None, 1.0f } } }
        };

    // Events for death and level up
    public System.Action<CreatureBase> OnDeath;
    public System.Action<CreatureBase, int> OnLevelUp;

    private void Awake()
    {
        // Scale stats based on level
        ScaleStatsWithLevel();
        health = maxHealth;
    }
    void Start()
    {
        ScaleStatsWithLevel();
        health = maxHealth;
    }

    private void ScaleStatsWithLevel()
    {
        float growthMultiplier = statGrowthCurve.Evaluate(level);

        maxHealth = baseMaxHealth * growthMultiplier;
        if (isPlayerCreature)
            maxHealth *= playerCreatureModifier;
        speed = baseSpeed;
        attackSpeed = baseAttackSpeed * (1 + (growthMultiplier - 1) * 0.5f); // Slower scaling for attack speed
        attackDamage = baseAttackDamage * growthMultiplier;

        

        // Update XP requirement for current level
        xpToNextLevel = Mathf.RoundToInt(xpCurve.Evaluate(level));
    }

    public void EquipItem(ItemBase item)
    {
        health += item.HealthModifier;
        if (item.ElementType == elementType)
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

        // Check for death
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isPlayerCreature)
        {
            // Player creature dies - send back to party with 0 HP
            health = 0;

            // Notify PlayerInventory to handle the death
            PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
            if (playerInventory != null)
            {
                playerInventory.HandleCreatureDeath(this);
            }
        }
        else
        {
            // Enemy creature dies - destroy and grant XP
            GrantXPToPlayer();
            Destroy(gameObject);
        }

        // Trigger death event
        OnDeath?.Invoke(this);
    }

    private void GrantXPToPlayer()
    {
        // Calculate XP reward based on level (higher level = more XP)
        int xpReward = Mathf.RoundToInt(10 * level * Random.Range(0.8f, 1.2f));

        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        if (playerInventory != null)
        {
            CreatureBase activeCreature = playerInventory.GetActiveCreature();
            if (activeCreature != null && activeCreature.isPlayerCreature)
            {
                activeCreature.GainXP(xpReward);
            }
        }
    }

    public void GainXP(int xpAmount)
    {
        if (level >= 30) return; // Max level reached

        currentXP += xpAmount;

        // Check for level up
        while (currentXP >= xpToNextLevel && level < 30)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;

        // Scale stats for new level
        ScaleStatsWithLevel();

        // Restore health on level up
        health = maxHealth;

        // Trigger level up event
        OnLevelUp?.Invoke(this, level);

        Debug.Log($"{gameObject.name} leveled up to level {level}!");
    }

    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
    }

    public void FullHeal()
    {
        health = maxHealth;
    }

    // Method to initialize creature with specific level (for enemies)
    public void InitializeWithLevel(int targetLevel)
    {
        level = Mathf.Clamp(targetLevel, 1, 30);
        ScaleStatsWithLevel();
        health = maxHealth;
    }
}

public enum ElementType
{
    None,
    Fire,
    Water,
    Grass
}