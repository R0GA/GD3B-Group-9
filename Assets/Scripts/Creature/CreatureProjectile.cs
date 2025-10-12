// Projectile.cs
using UnityEngine;

public class CreatureProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private GameObject impactEffect;

    private float damage;
    private ElementType elementType;
    private Transform target;
    private CreatureBase owner;
    private bool isInitialized = false;

    public void Initialize(float projectileDamage, ElementType projectileElement, Transform projectileTarget, CreatureBase projectileOwner)
    {
        damage = projectileDamage;
        elementType = projectileElement;
        target = projectileTarget;
        owner = projectileOwner;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target);

        // Check if reached target
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        if (target != null)
        {
            CreatureBase targetCreature = target.GetComponent<CreatureBase>();
            if (targetCreature != null)
            {
                targetCreature.TakeDamage(damage, elementType);
            }
        }

        // Spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}