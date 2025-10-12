// CreatureSpawner.cs
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private bool spawnAsEnemy = true;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxSpawnCount = 5;

    private float lastSpawnTime;
    private int currentSpawnCount;

    private void Update()
    {
        if (spawnAsEnemy && currentSpawnCount < maxSpawnCount && Time.time >= lastSpawnTime + spawnInterval)
        {
            SpawnCreature();
            lastSpawnTime = Time.time;
        }
    }

    public void SpawnCreature()
    {
        GameObject creatureObj = Instantiate(creaturePrefab, transform.position, Quaternion.identity);
        CreatureBase creature = creatureObj.GetComponent<CreatureBase>();
        CreatureController controller = creatureObj.GetComponent<CreatureController>();

        if (creature != null)
        {
            creature.isPlayerCreature = !spawnAsEnemy;

            if (!spawnAsEnemy && controller != null)
            {
                // If spawning as player creature, set return position
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    controller.SetReturnPosition(player.transform.position);
                }
            }
        }

        currentSpawnCount++;
    }

    // Call this when a creature dies to allow respawning
    public void OnCreatureDied()
    {
        currentSpawnCount--;
    }
}