using UnityEngine;

public class XPOrb : CollectibleItem
{
    [Header("XP Orb Settings")]
    public int xpAmount = 10;
    public ParticleSystem collectParticles;

    protected override void Collect()
    {
        // Grant XP to active creature
        CreatureBase activeCreature = PlayerInventory.Instance.GetActiveCreature();
        if (activeCreature != null)
        {
            activeCreature.GainXP(xpAmount);
            Debug.Log($"Collected {xpAmount} XP!");
        }

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