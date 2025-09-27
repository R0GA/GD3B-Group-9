using UnityEngine;
using UnityEngine.AI;

public class CreatureFollow : MonoBehaviour
{
    public Transform target;
    NavMeshAgent nav;

    private void Start()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        nav.SetDestination(target.position);
    }
}
