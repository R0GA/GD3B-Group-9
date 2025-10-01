using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    enum AIState
    {
        Idle, Patrolling, Chasing, Attacking
    }

    [Header("Health")]
    [SerializeField] private int health;

    [Header("Patrol")]
    [SerializeField] private Transform wayPoints;
    [SerializeField] private float waitAtPoint = 2f;
    private int currentWaypoint;
    private float waitCounter;

    [Header("Components")]
    //[SerializeField] Animator animator;
    NavMeshAgent agent; 

    [Header("AI States")]
    [SerializeField] private AIState currentState;

    [Header("Chasing")]
    [SerializeField] private float chaseRange;

    [Header("Alerted")] //Time until enemy returns to patrol state
    [SerializeField] private float alertedTime;
    private float timeSinceLastSawPlayer;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackTime = 2f;
    public GameObject Projectile;
    public Transform spawnPoint;
    public float shootTime;
    public float projectileSpeed;
    private float timeToAttack;
    private GameObject player;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");

        waitCounter = waitAtPoint;
        timeSinceLastSawPlayer = alertedTime;
        timeToAttack = attackTime;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(this.gameObject);
            

        }
    }
    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case AIState.Idle:

                if (waitCounter > 0)
                {
                    waitCounter -= Time.deltaTime;
                }
                else
                {
                    currentState = AIState.Patrolling;
                    agent.SetDestination(wayPoints.GetChild(currentWaypoint).position);
                }

                if (distanceToPlayer <= chaseRange)
                {
                    currentState = AIState.Chasing;
                }

                break;

            case AIState.Patrolling:

                if (agent.remainingDistance <= 0.2f)
                {
                    currentWaypoint++;
                    if (currentWaypoint >= wayPoints.childCount)
                    {
                        currentWaypoint = 0;
                    }

                    agent.SetDestination(wayPoints.GetChild(currentWaypoint).position);
                }

                if (distanceToPlayer <= chaseRange)
                {
                    currentState = AIState.Chasing;
                    Debug.Log("Chasing");
                }

                break;

            case AIState.Chasing:

                agent.SetDestination(player.transform.position);
                if (distanceToPlayer > chaseRange)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                    timeSinceLastSawPlayer -= Time.deltaTime;

                    if (timeSinceLastSawPlayer <= 0)
                    {
                        currentState = AIState.Idle;
                        timeSinceLastSawPlayer = alertedTime;
                        agent.isStopped = false;
                    }
                }

                if (distanceToPlayer <= attackRange)
                {
                    currentState = AIState.Attacking;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }

                break;

            case AIState.Attacking:

                transform.LookAt(player.transform.position, Vector3.up);

                timeToAttack -= Time.deltaTime;

                if (timeToAttack <= 0)
                {
                    shootTime += Time.deltaTime;

                    
                    
                        shootTime = 0;
                        var projectile = Instantiate(Projectile, spawnPoint.position, spawnPoint.rotation);

                        var rb = projectile.GetComponent<Rigidbody>();
                        rb.linearVelocity = spawnPoint.forward * projectileSpeed;

                        Destroy(projectile, 2f);
                    
                    timeToAttack = attackTime;
                    Debug.Log("Attacking");
                }

                if (distanceToPlayer > attackRange)
                {
                    currentState = AIState.Chasing;
                    agent.isStopped = false;
                }

                break;
        }


    }
}
