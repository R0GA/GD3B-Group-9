using UnityEngine;

public class HealItem : CollectibleItem
{
    [Header("Heal Item Settings")]
    public ParticleSystem collectParticles;

    protected override void Collect()
    {
        // Heal player and all party creatures
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.FullHeal();
        }

        PlayerInventory.Instance.HealParty();
        Debug.Log("Collected Heal Item - Party fully healed!");

        // Visual/Audio feedback
        if (collectParticles != null)
        {
            Instantiate(collectParticles, transform.position, Quaternion.identity);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}