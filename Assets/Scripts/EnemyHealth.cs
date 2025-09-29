using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int health;
    public GameObject creatureCard;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if(health <= 0)
        {
            Destroy(this.gameObject);
            creatureCard.SetActive(true);

        }
    }
}
