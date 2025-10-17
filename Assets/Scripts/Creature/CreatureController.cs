using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CreatureController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private bool isMelee = true;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float rangedOptimalDistance = 5f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Combat Settings")]
    [SerializeField] private CreatureProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    private CreatureBase creatureBase;
    private NavMeshAgent navMeshAgent;
    private Transform currentTarget;
    private Transform playerTransform;
    private bool isInCombat = false;
    private float lastAttackTime = 0f;
    private Vector3 returnPosition;
    private bool hasReturnPosition = false;

    private void Awake()
    {
        creatureBase = GetComponent<CreatureBase>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Find player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;

            // If this is a player creature, set up following
            if (creatureBase.isPlayerCreature)
            {
                // Set initial return position near player
                Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                returnPosition = playerTransform.position + randomOffset;

                // Ensure it's on NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(returnPosition, out hit, 3f, NavMesh.AllAreas))
                {
                    returnPosition = hit.position;
                }

                hasReturnPosition = true;

                // Configure NavMeshAgent for following
                if (navMeshAgent != null)
                {
                    navMeshAgent.stoppingDistance = 2f; // Comfortable distance from player
                }
            }
        }

        SetupNavMeshAgent();
    }

    private void SetupNavMeshAgent()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = creatureBase.speed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.angularSpeed = 360f;
            navMeshAgent.acceleration = 8f;
        }
    }

    private void Update()
    {
        if (creatureBase.health <= 0) return;

        // For player creatures, always check if we should return to player
        if (creatureBase.isPlayerCreature && !isInCombat)
        {
            ReturnToPlayer();
        }

        FindTarget();
        HandleCombatBehavior();
    }

    private void FindTarget()
    {
        // Clear current target if it's dead or null
        if (currentTarget != null)
        {
            CreatureBase targetCreature = currentTarget.GetComponent<CreatureBase>();
            PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();

            bool isDead = false;
            if (targetCreature != null)
                isDead = targetCreature.health <= 0;
            else if (targetPlayer != null)
                isDead = targetPlayer.health <= 0;

            if (isDead)
            {
                currentTarget = null;
                isInCombat = false;
            }
        }

        // Find new target if none exists
        if (currentTarget == null)
        {
            LayerMask targetLayerMask = GetTargetLayerMask();
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, targetLayerMask);
            float closestDistance = Mathf.Infinity;
            Transform closestTarget = null;

            foreach (var hitCollider in hitColliders)
            {
                CreatureBase potentialTarget = hitCollider.GetComponent<CreatureBase>();
                PlayerController potentialPlayer = hitCollider.GetComponent<PlayerController>();

                bool isValid = false;
                if (potentialTarget != null)
                    isValid = IsValidTarget(potentialTarget);
                else if (potentialPlayer != null && !creatureBase.isPlayerCreature)
                    isValid = potentialPlayer.health > 0;

                if (isValid)
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = hitCollider.transform;
                    }
                }
            }

            if (closestTarget != null)
            {
                currentTarget = closestTarget;
                isInCombat = true;
            }
            else
            {
                isInCombat = false;
                // Don't call ReturnToPlayer here anymore - we call it in Update
            }
        }
    }

    private void HandleCombatBehavior()
    {
        if (!isInCombat || currentTarget == null)
        {
            // Don't stop the agent if we're a player creature - let it continue following
            if (!creatureBase.isPlayerCreature && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = true;
            }
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (isMelee)
        {
            HandleMeleeCombat(distanceToTarget);
        }
        else
        {
            HandleRangedCombat(distanceToTarget);
        }
    }

    private LayerMask GetTargetLayerMask()
    {
        if (creatureBase.isPlayerCreature)
        {
            // Player creatures target enemies
            return LayerMask.GetMask("EnemyCreature");
        }
        else
        {
            // Enemy creatures target player and player creatures
            return LayerMask.GetMask("Player", "PlayerCreature");
        }
    }

    private bool IsValidTarget(CreatureBase target)
    {
        // Player creatures target enemies, enemy creatures target player or player creatures
        if (creatureBase.isPlayerCreature)
        {
            return !target.isPlayerCreature;
        }
        else
        {
            // Enemy creatures can target both player creatures AND the player themselves
            return target.isPlayerCreature || target == GetPlayerActiveCreature() || IsPlayerObject(target.gameObject);
        }
    }

    // Helper method to check if this is the main player object
    private bool IsPlayerObject(GameObject obj)
    {
        PlayerController playerController = obj.GetComponent<PlayerController>();
        return playerController != null;
    }

    private CreatureBase GetPlayerActiveCreature()
    {
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        return playerInventory != null ? playerInventory.GetActiveCreature() : null;
    }

    private void HandleMeleeCombat(float distanceToTarget)
    {
        if (distanceToTarget <= attackRange)
        {
            // Stop moving and attack
            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;

            TryAttack();
        }
        else
        {
            // Move towards target
            if (navMeshAgent.isActiveAndEnabled && currentTarget != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(currentTarget.position);
            }
        }
    }

    private void HandleRangedCombat(float distanceToTarget)
    {
        if (distanceToTarget <= attackRange && distanceToTarget >= rangedOptimalDistance * 0.8f)
        {
            // In optimal range - stop and attack
            if (navMeshAgent.isActiveAndEnabled)
                navMeshAgent.isStopped = true;

            TryAttack();
        }
        else if (distanceToTarget < rangedOptimalDistance * 0.8f)
        {
            // Too close - move away
            if (navMeshAgent.isActiveAndEnabled && currentTarget != null)
            {
                navMeshAgent.isStopped = false;
                Vector3 directionAway = (transform.position - currentTarget.position).normalized;
                Vector3 targetPosition = transform.position + directionAway * rangedOptimalDistance;
                navMeshAgent.SetDestination(targetPosition);
            }
        }
        else
        {
            // Too far - move closer
            if (navMeshAgent.isActiveAndEnabled && currentTarget != null)
            {
                navMeshAgent.isStopped = false;

                // Calculate position at optimal distance from target
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                Vector3 optimalPosition = currentTarget.position - directionToTarget * rangedOptimalDistance;
                navMeshAgent.SetDestination(optimalPosition);
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + 1f / creatureBase.attackSpeed)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        if (isMelee)
        {
            PerformMeleeAttack();
        }
        else
        {
            PerformRangedAttack();
        }
    }

    private void PerformMeleeAttack()
    {
        if (currentTarget != null)
        {
            // Try to damage as creature first
            CreatureBase targetCreature = currentTarget.GetComponent<CreatureBase>();
            if (targetCreature != null)
            {
                targetCreature.TakeDamage(creatureBase.attackDamage, creatureBase.elementType);
            }
            else
            {
                // If not a creature, try to damage as player
                PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();
                if (targetPlayer != null)
                {
                    targetPlayer.TakeDamage(creatureBase.attackDamage, creatureBase.elementType);
                }
            }

            // Optional: Play attack animation or effects
            StartCoroutine(MeleeAttackAnimation());
        }
    }

    private void PerformRangedAttack()
    {
        if (projectilePrefab != null && currentTarget != null)
        {
            CreatureProjectile projectile = Instantiate(projectilePrefab,
                projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position,
                Quaternion.identity);

            projectile.Initialize(creatureBase.attackDamage, creatureBase.elementType, currentTarget, creatureBase);
        }
        else
        {
            // Fallback: direct damage if no projectile
            if (currentTarget != null)
            {
                CreatureBase targetCreature = currentTarget.GetComponent<CreatureBase>();
                PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();

                if (targetCreature != null)
                {
                    targetCreature.TakeDamage(creatureBase.attackDamage, creatureBase.elementType);
                }
                else if (targetPlayer != null)
                {
                    targetPlayer.TakeDamage(creatureBase.attackDamage, creatureBase.elementType);
                }
            }
        }
    }

    private void ReturnToPlayer()
    {
        if (playerTransform == null || !hasReturnPosition) return;

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // If we're too far from player, move towards them
        if (distanceToPlayer > 3f) // Increased from 1.5f to 3f
        {
            // Update return position to be near player (not on top of them)
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            returnPosition = playerTransform.position - directionToPlayer * 2f; // Stay 2 units away

            // Ensure the position is on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(returnPosition, out hit, 3f, NavMesh.AllAreas))
            {
                returnPosition = hit.position;
            }

            if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(returnPosition);

                // Debug: Draw the path
                Debug.DrawLine(transform.position, returnPosition, Color.green);
            }
        }
        else
        {
            // We're close enough to player, stop moving
            if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
            }
        }
    }

    private IEnumerator MeleeAttackAnimation()
    {
        // Simple attack animation - you can replace this with your actual animation
        Vector3 originalPosition = transform.position;
        if (currentTarget != null)
        {
            Vector3 attackDirection = (currentTarget.position - transform.position).normalized * 0.3f;
            float attackTime = 0.1f;
            float elapsedTime = 0f;

            while (elapsedTime < attackTime)
            {
                transform.position = Vector3.Lerp(originalPosition, originalPosition + attackDirection, elapsedTime / attackTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;
            while (elapsedTime < attackTime)
            {
                transform.position = Vector3.Lerp(originalPosition + attackDirection, originalPosition, elapsedTime / attackTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPosition;
        }
    }

    // Gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Ranged optimal distance
        if (!isMelee)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedOptimalDistance);
        }

        // Current target line
        if (currentTarget != null)
        {
            Gizmos.color = isInCombat ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    // Public methods for external control
    public void SetTarget(Transform target)
    {
        currentTarget = target;
        isInCombat = target != null;
    }

    public void SetReturnPosition(Vector3 position)
    {
        returnPosition = position;
        hasReturnPosition = true;
    }

    public bool IsInCombat()
    {
        return isInCombat;
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}