using UnityEngine;

public class LootBag : CollectibleItem
{
    [Header("Loot Bag Settings")]
    public int gachaPacks = 5;
    public ParticleSystem collectParticles;

    protected override void Collect()
    {
        // Grant gacha packs
        GachaUIManager.Instance.AddPacks(gachaPacks);
        Debug.Log($"Collected Loot Bag - {gachaPacks} gacha packs added!");

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