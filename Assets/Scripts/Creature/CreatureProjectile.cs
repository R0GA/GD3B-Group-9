using UnityEngine;

public class CreatureProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private LayerMask collisionLayers = ~0; // Default to all layers

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
        if (!isInitialized)
        {
            Destroy(gameObject);
            return;
        }

        // If target is null/destroyed, destroy the projectile
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target);

        // Check if reached target (using a small collision radius)
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            OnHit(target.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only process collisions if we're initialized and the collider is not the owner
        if (!isInitialized || other.gameObject == owner?.gameObject)
            return;

        // Check if we hit the target or something else we should damage
        if (other.transform == target || ShouldDamage(other.gameObject))
        {
            OnHit(other.gameObject);
        }
    }

    private bool ShouldDamage(GameObject hitObject)
    {
        // Check if the hit object is on a layer we should damage
        if (((1 << hitObject.layer) & collisionLayers) == 0)
            return false;

        // Don't damage friendly units
        if (owner != null)
        {
            CreatureBase hitCreature = hitObject.GetComponent<CreatureBase>();
            PlayerController hitPlayer = hitObject.GetComponent<PlayerController>();

            if (hitCreature != null)
            {
                // Don't damage allies
                if (owner.isPlayerCreature == hitCreature.isPlayerCreature)
                    return false;
            }
            else if (hitPlayer != null)
            {
                // Enemy projectiles can damage player, player projectiles cannot
                if (owner.isPlayerCreature)
                    return false;
            }
        }

        return true;
    }

    private void OnHit(GameObject hitObject)
    {
        // Apply damage to the hit object
        bool didDamage = false;

        // Try to damage as CreatureBase first
        CreatureBase targetCreature = hitObject.GetComponent<CreatureBase>();
        if (targetCreature != null)
        {
            targetCreature.TakeDamage(damage, elementType);
            didDamage = true;
        }
        else
        {
            // Try to damage as PlayerController
            PlayerController targetPlayer = hitObject.GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                targetPlayer.TakeDamage(damage, elementType);
                didDamage = true;
            }
        }

        // Spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Destroy the projectile
        Destroy(gameObject);

        // Debug log
        if (didDamage)
        {
            Debug.Log($"Projectile hit {hitObject.name} for {damage} damage!");
        }
        else
        {
            Debug.LogWarning($"Projectile hit {hitObject.name} but couldn't apply damage!");
        }
    }

    // Optional: Add a timeout to prevent projectiles from existing forever
    private void Start()
    {
        // Auto-destroy after 10 seconds if it hasn't hit anything
        Destroy(gameObject, 10f);
    }

    // Gizmos for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}