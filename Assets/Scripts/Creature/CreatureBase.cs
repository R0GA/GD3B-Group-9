using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

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
    public ParticleSystem levelParticles;
    public AudioSource levelAudio;

    [Header("Base Stats (Level 1)")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseAttackDamage = 10f;

    // Item management
    private ItemBase equippedItem;
    private float baseMaxHealthWithoutItem;
    private float baseAttackDamageWithoutItem;
    private float currentMaxHealth; // Track current max health for health addition calculation

    [Header("Other")]
    // Unique ID for each creature instance
    [SerializeField] private string creatureID;
    [SerializeField] private ParticleSystem hurtParticles;
    [SerializeField] private Image friendlyIndicator;

    [Header("Damage Numbers")]
    [SerializeField] private GameObject floatingDamageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0, 1.5f, 0);

    // Example: effectiveness[attacker][defender] = multiplier
    private static readonly Dictionary<ElementType, Dictionary<ElementType, float>> effectiveness =
        new Dictionary<ElementType, Dictionary<ElementType, float>>
        {
            { ElementType.Fire, new Dictionary<ElementType, float> { { ElementType.Grass, 2.0f }, { ElementType.Water, 0.5f }, { ElementType.None, 1.0f }, { ElementType.Fire, 1.0f }, } },
            { ElementType.Water, new Dictionary<ElementType, float> { { ElementType.Fire, 2.0f }, { ElementType.Grass, 0.5f }, { ElementType.None, 1.0f }, { ElementType.Water, 1.0f } } },
            { ElementType.Grass, new Dictionary<ElementType, float> { { ElementType.Water, 2.0f }, { ElementType.Fire, 0.5f }, { ElementType.None, 1.0f }, { ElementType.Grass, 1.0f } } },
            { ElementType.None, new Dictionary<ElementType, float>{ { ElementType.Fire, 1.0f }, { ElementType.Water, 1.0f }, { ElementType.Grass, 1.0f }, { ElementType.None, 1.0f } } }
        };

    // Events for death and level up
    public System.Action<CreatureBase> OnDeath;
    public System.Action<CreatureBase, int> OnLevelUp;
    public System.Action<CreatureBase> OnDamageTaken;
    public System.Action<CreatureBase> OnHealed;

    private void Awake()
    {
        // Store base stats before any item modifications
        baseMaxHealthWithoutItem = baseMaxHealth;
        baseAttackDamageWithoutItem = baseAttackDamage;
        currentMaxHealth = baseMaxHealth;

        // Generate unique ID if not already set
        if (string.IsNullOrEmpty(creatureID))
        {
            GenerateNewID();
        }

        if (hurtParticles != null)
        {
            hurtParticles.Stop();
            var main = hurtParticles.main;
            switch (elementType)
            {
                case ElementType.Fire:
                    main.startColor = Color.red;
                    break;
                case ElementType.Water:
                    main.startColor = Color.blue;
                    break;
                case ElementType.Grass:
                    main.startColor = Color.green;
                    break;
                default:
                    main.startColor = Color.white;
                    break;
            }
        }

        // Scale stats based on level
        ScaleStatsWithLevel();
        if (!isPlayerCreature)
            health = maxHealth;
    }

    void Start()
    {
        ScaleStatsWithLevel();
        if (!isPlayerCreature)
            health = maxHealth;

        if (isPlayerCreature)
        {
            if (friendlyIndicator != null)
                friendlyIndicator.color = Color.green;
        }
        else
        {
            if (friendlyIndicator != null)
                friendlyIndicator.color = Color.red;
        }
    }

    // Generate a new unique ID for this creature
    public void GenerateNewID()
    {
        creatureID = Guid.NewGuid().ToString();
    }

    // Set a specific ID (for loading saved games)
    public void SetID(string id)
    {
        creatureID = id;
    }

    public string CreatureID => creatureID;

    public void ScaleStatsWithLevel()
    {
        float growthMultiplier = statGrowthCurve.Evaluate(level);

        // Calculate base stats without item
        float calculatedMaxHealth = baseMaxHealthWithoutItem * growthMultiplier;
        if (isPlayerCreature)
            calculatedMaxHealth *= playerCreatureModifier;

        float calculatedAttackDamage = baseAttackDamageWithoutItem * growthMultiplier;
        float calculatedAttackSpeed = baseAttackSpeed * (1 + (growthMultiplier - 1) * 0.5f);

        // Apply item bonuses if equipped
        if (equippedItem != null)
        {
            calculatedMaxHealth *= (1 + equippedItem.HealthModifierPercent);
            if (equippedItem.ElementType == elementType)
            {
                calculatedAttackDamage *= (1 + equippedItem.DamageModifierPercent);
            }
        }

        // Store the old max health for health addition calculation
        float oldMaxHealth = maxHealth;

        maxHealth = calculatedMaxHealth;
        attackDamage = calculatedAttackDamage;
        attackSpeed = calculatedAttackSpeed;
        speed = baseSpeed;

        // Update XP requirement for current level
        xpToNextLevel = Mathf.RoundToInt(xpCurve.Evaluate(level));

        // Store current max health for future calculations
        currentMaxHealth = maxHealth;
    }

    public void EquipItem(ItemBase item)
    {
        if (equippedItem != null)
        {
            Debug.LogWarning($"Creature {creatureID} already has {equippedItem.ItemName} equipped. Dequip first.");
            return;
        }

        // Store old max health before equipping
        float oldMaxHealth = maxHealth;

        equippedItem = item;
        Debug.Log($"Equipped {item.ItemName} to creature {creatureID}");

        // Recalculate stats with new item
        ScaleStatsWithLevel();

        // Calculate health increase and add to current health
        float healthIncrease = maxHealth - oldMaxHealth;
        if (healthIncrease > 0)
        {
            health += healthIncrease;
            Debug.Log($"Added {healthIncrease} health from item. New health: {health}/{maxHealth}");
        }
    }

    public void DequipItem(ItemBase item)
    {
        if (equippedItem == item)
        {
            // Store old max health before dequipping
            float oldMaxHealth = maxHealth;

            Debug.Log($"Dequipped {item.ItemName} from creature {creatureID}");
            equippedItem = null;

            // Recalculate stats without the item
            ScaleStatsWithLevel();

            // Calculate health decrease but don't let health drop below 1
            float healthDecrease = oldMaxHealth - maxHealth;
            if (healthDecrease > 0)
            {
                health = Mathf.Max(health - healthDecrease, 1);
                Debug.Log($"Removed {healthDecrease} health from item. New health: {health}/{maxHealth}");
            }
        }
        else
        {
            Debug.LogWarning($"Item {item.ItemName} is not equipped to creature {creatureID}.");
        }
    }

    public ItemBase GetEquippedItem()
    {
        return equippedItem;
    }

    public bool HasEquippedItem()
    {
        return equippedItem != null;
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

        // SPAWN FLOATING DAMAGE NUMBER
        SpawnDamageNumber(scaledDamage, attackerType, multiplier);

        // Trigger damage event
        OnDamageTaken?.Invoke(this);

        if (hurtParticles != null)
            hurtParticles.Play();

        // Check for death
        if (health <= 0)
        {
            Die();
        }
    }

    private void SpawnDamageNumber(float damage, ElementType damageType, float effectivenessMultiplier)
    {
        if (floatingDamageNumberPrefab == null)
        {
            // Try to load the prefab from Resources if not assigned
            floatingDamageNumberPrefab = Resources.Load<GameObject>("FloatingDamageNumber");
            if (floatingDamageNumberPrefab == null)
            {
                Debug.LogWarning("FloatingDamageNumber prefab not found in Resources!");
                return;
            }
        }

        // Calculate spawn position (above the creature with offset)
        Vector3 spawnPosition = transform.position + damageNumberOffset;

        // Instantiate the damage number
        GameObject damageNumberObj = Instantiate(floatingDamageNumberPrefab, spawnPosition, Quaternion.identity);
        FloatingDamageNumber damageNumber = damageNumberObj.GetComponent<FloatingDamageNumber>();

        if (damageNumber != null)
        {
            damageNumber.Initialize(damage, damageType, effectivenessMultiplier, spawnPosition);
        }
    }

    // Update Heal method to trigger event
    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        OnHealed?.Invoke(this);
    }

    // Update LevelUp method to trigger event
    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        levelParticles?.Play();
        levelAudio?.Play();

        // Store old max health before leveling up
        float oldMaxHealth = maxHealth;

        // Scale stats for new level
        ScaleStatsWithLevel();

        // Add the health increase from level up to current health
        float healthIncrease = maxHealth - oldMaxHealth;
        health += healthIncrease;

        // Ensure health doesn't exceed max health
        health = Mathf.Min(health, maxHealth);

        Debug.Log($"{gameObject.name} (ID: {creatureID}) leveled up to level {level}! Gained {healthIncrease} health.");

        // Trigger level up event
        OnLevelUp?.Invoke(this, level);
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
            // Enemy creature dies - drop XP orb and destroy
            DropXPOrb();
            Destroy(gameObject);
        }

        // Trigger death event
        OnDeath?.Invoke(this);
    }

    private void GrantXPToPlayer()
    {
        // Calculate XP reward based on level (higher level = more XP)
        int xpReward = Mathf.RoundToInt(10 * level * UnityEngine.Random.Range(0.8f, 1.2f));

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
    private void DropXPOrb()
    {
        // Calculate XP reward based on level
        int xpReward = Mathf.RoundToInt(10 * level * UnityEngine.Random.Range(0.8f, 1.2f));

        // Load XP orb prefab
        GameObject xpOrbPrefab = Resources.Load<GameObject>("Collectibles/XPOrb");
        if (xpOrbPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0, 0.5f, 0);
            GameObject xpOrb = Instantiate(xpOrbPrefab, spawnPosition, Quaternion.identity);

            XPOrb xpOrbComponent = xpOrb.GetComponent<XPOrb>();
            if (xpOrbComponent != null)
            {
                xpOrbComponent.xpAmount = xpReward;
            }
        }
        else
        {
            Debug.LogWarning("XPOrb prefab not found in Resources/Collectibles/");
        }
    }
}

public enum ElementType
{
    None,
    Fire,
    Water,
    Grass
}