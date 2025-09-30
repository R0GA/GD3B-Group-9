using UnityEngine;

public class CreatureCard : MonoBehaviour
{
    public GameObject creature;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            creature.SetActive(true);
            Destroy(gameObject);
        }
    }
}
