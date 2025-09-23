using UnityEngine;
using UnityEngine.Rendering;

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
    [SerializeField] private int captureChallenge;

    private void Awake()
    {
        if (isPlayerCreature)
        {
            maxHealth *= playerCreatureModifier;
            attackDamage *= playerCreatureModifier;
        }
        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public void TryCapture()
    {
        if (!isPlayerCreature)
        {
            int random = Random.Range(0, 100);
            if (random > captureChallenge)
            {
                isPlayerCreature = true;
                //further capture logic will come here eventually
            }
        }
    }

}
