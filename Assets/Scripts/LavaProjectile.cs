using UnityEngine;

public class LavaProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playercontroller = collision.gameObject.GetComponent<PlayerController>();
            if (playercontroller != null)
            {
                playercontroller.TakeDamage(damage);
                Debug.Log("Hit by lava!");
            }
        }
    }
}
