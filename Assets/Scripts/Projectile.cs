using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;

    private void Start()
    {
        Destroy(gameObject, 10);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
