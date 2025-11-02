using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyTrialManager : MonoBehaviour
{
    [Header("References")]
    public GameObject[] enemyWaves;
    public GameObject successPanel;
    public GameObject trialStartText;
    public GameObject trialWall;
    public int xpReward = 200;
    public Transform rewardSpawn;

    [Header("UI")]
    public Text waveNameText;
    public float waveNameDisplayTime = 3f;

    [Header("Wave Delay")]
    public float waveDelay = 2f;

    [Header("Audio")]
    //public AudioSource trialMusic;

    private List<GameObject> enemies = new List<GameObject>();
    private int currentWave = 0;
    private bool trialActive = false;
    private bool trialComplete = false;
    private bool waveTransitioning = false;
    void Start()
    {
        for (int i = 0; i < enemyWaves.Length; i++)
        {
            if (enemyWaves[i] != null)
            {
                if (i == 0) continue;
                enemyWaves[i].SetActive(false);
            }
        }

        if (successPanel != null)
            successPanel.SetActive(false);

        if (trialWall != null)
            trialWall.SetActive(true);

        if (trialStartText != null)
            trialStartText.SetActive(false);

        if (waveNameText != null)
            waveNameText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!trialActive || trialComplete || waveTransitioning)
            return;

        //Removes destroyed or inactive enemies
        enemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        
        if (enemies.Count == 0 && currentWave < enemyWaves.Length)
        {
            string defeatedWaveName = enemyWaves[currentWave].name;
            Debug.Log($"{defeatedWaveName} defeated!");

            currentWave++;

            if (currentWave < enemyWaves.Length)
            {
                waveTransitioning = true;
                Debug.Log($"Waiting {waveDelay} seconds before next wave");
                Invoke(nameof(StartNextWave), waveDelay);
            }
            else
            {
                trialComplete = true;
                OnTrialComplete();
            }
                
        }
    }

    public void StartTrial()
    {

        
        //trialMusic.Play();


        if (trialActive) return;

        trialActive = true;
        trialComplete = false;
        currentWave = 0;

        Debug.Log("Trial started!");

        if (trialStartText != null)
        {
            trialStartText.SetActive(true);
            Invoke(nameof(HideTrialStartText), 2f);
        }

        RegisterActiveEnemies(enemyWaves[0]);
        
    }

    private void StartNextWave()
    {
        waveTransitioning = false;
        ActivateWave(currentWave);
    }

    private void ActivateWave(int index)
    {
        if (index >= enemyWaves.Length) return;

        enemies.Clear();

        GameObject wave = enemyWaves[index];
        if (wave != null)
        {
            wave.SetActive(true);
            RegisterActiveEnemies(wave);

            string waveName = wave.name;
            Debug.Log($" Wave {index + 1} - \"{wave.name}\" activated with {enemies.Count} enemies.");

            ShowWaveName(waveName);
        }


    
    }

    private void RegisterActiveEnemies(GameObject wave)
    {
        foreach (Transform child in wave.transform)
        {
            if (child.gameObject != null)
                enemies.Add(child.gameObject);
        }
    }

    private void ShowWaveName(string name)
    {
        if (waveNameText != null)
        {
            waveNameText.text = $"{name}";
            waveNameText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideWaveName));
            Invoke(nameof(HideWaveName), waveNameDisplayTime);
        }
    }

    private void HideWaveName()
    {
        if (waveNameText != null)
            waveNameText.gameObject.SetActive(false);
    }

    private void HideTrialStartText()
    {
        if (trialStartText != null)
            trialStartText.SetActive(false);
    }
    private void OnTrialComplete()
    {
        Debug.Log("Trial complete! All waves cleared.");

        //trialMusic.Stop();
        

        // HEAL AND REWARD THE PLAYER AND PARTY
        if (PlayerInventory.Instance != null)
        {
            SpawnTrialRewards();
        }

        if (successPanel != null)
        {
            successPanel.SetActive(true);
            Debug.Log("Success panel activated.");
        }

        if (trialWall != null)
        {
            trialWall.SetActive(false);
            Debug.Log("Trial wall deactivated.");
        }
    }

    public void Continue()
    {
        if (successPanel != null)
            successPanel.SetActive(false);
        
    }
    private void SpawnTrialRewards()
    {
        Vector3 rewardSpawnPosition = GetRewardSpawnPosition();

        // Spawn Super XP Orb
        GameObject superXPOrbPrefab = Resources.Load<GameObject>("Collectibles/SuperXPOrb");
        if (superXPOrbPrefab != null)
        {
            Instantiate(superXPOrbPrefab, rewardSpawnPosition, Quaternion.identity);
        }

        // Spawn Heal Item (offset position)
        GameObject healItemPrefab = Resources.Load<GameObject>("Collectibles/HealItem");
        if (healItemPrefab != null)
        {
            Vector3 healSpawnPos = rewardSpawnPosition + new Vector3(2f, 0, 0);
            Instantiate(healItemPrefab, healSpawnPos, Quaternion.identity);
        }

        // Spawn Loot Bag (offset position)
        GameObject lootBagPrefab = Resources.Load<GameObject>("Collectibles/LootBag");
        if (lootBagPrefab != null)
        {
            Vector3 lootSpawnPos = rewardSpawnPosition + new Vector3(-2f, 0, 0);
            Instantiate(lootBagPrefab, lootSpawnPos, Quaternion.identity);
        }
    }

    private Vector3 GetRewardSpawnPosition()
    {
        GameObject rewardSpawnPoint;

        if (rewardSpawn != null)
            rewardSpawnPoint = rewardSpawn.gameObject;
        else
            rewardSpawnPoint = gameObject;

        if (rewardSpawnPoint != null)
        {
            return rewardSpawnPoint.transform.position;
        }

        // Fallback to trial manager position
        return transform.position;
    }
}
