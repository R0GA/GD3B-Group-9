using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/ItemBase")]
public class ItemBase : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private int healthModifier;
    [SerializeField] private int damageModifier;
    [SerializeField] private ElementType elementType;

    public string ItemName => itemName;
    public int HealthModifier => healthModifier;
    public int DamageModifier => damageModifier;
    public ElementType ElementType => elementType;
}