using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/ItemBase")]
public class ItemBase : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private float healthModifierPercent = 0f;
    [SerializeField] private float damageModifierPercent = 0f;
    [SerializeField] private ElementType elementType;
    public Sprite icon;

    // Unique ID for each item instance
    [SerializeField] private string itemID;

    public string ItemName => itemName;
    public float HealthModifierPercent => healthModifierPercent;
    public float DamageModifierPercent => damageModifierPercent;
    public ElementType ElementType => elementType;
    public string ItemID => itemID;

    // Method to generate and set a unique ID
    public void GenerateNewID()
    {
        itemID = Guid.NewGuid().ToString();
    }

    // Method to set a specific ID (for loading saved games)
    public void SetID(string id)
    {
        itemID = id;
    }
}