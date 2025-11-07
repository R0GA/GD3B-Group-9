using UnityEngine;

public class SuperXPOrb : CollectibleItem
{
    [Header("Super XP Orb Settings")]
    public int xpAmount = 100;
    public ParticleSystem collectParticles;

    protected override void Collect()
    {
        // Grant XP to all party creatures
        PlayerInventory.Instance.GrantXPToAllPartyCreatures(xpAmount);
        Debug.Log($"Collected SUPER {xpAmount} XP for all party creatures!");

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