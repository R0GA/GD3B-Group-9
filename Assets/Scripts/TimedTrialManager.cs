using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TimedTrialManager : MonoBehaviour
{
    [Header("References")]
    public GameObject[] enemyWaves;
    public GameObject successPanel;
    public GameObject timerText;
    public GameObject trialStartText;
    public GameObject trialWall;
    public GameObject invisibleWall;
    public TrialTimer trialTimer;
    public int xpReward = 500;
    public Transform rewardSpawn;

    [Header("UI")]
    public GameObject bossIntroPanel;
    public Button startTrialButton; 
    public Text waveNameText;
    public float waveNameDisplayTime = 3f;

    [Header("Wave Delay")]
    public float waveDelay = 2f;

    [Header("Audio")]
    public AudioSource trialMusic;

    private List<GameObject> enemies = new List<GameObject>();
    private int currentWave = 0;
    private bool trialActive = false;
    private bool trialComplete = false;
    private bool waveTransitioning = false;
    private bool trialFailed = false;

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

        if (timerText  != null)
            timerText.SetActive(false);

        if (waveNameText != null)
            waveNameText.gameObject.SetActive(false);

        if (trialTimer != null)
            trialTimer.gameObject.SetActive(false);

        if (bossIntroPanel != null)
            bossIntroPanel.SetActive(false);

        if (invisibleWall != null)
            invisibleWall.SetActive(true); 

        if (startTrialButton != null)
            startTrialButton.onClick.AddListener(OnStartTrialButtonPressed);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !trialActive)
        {
            if (bossIntroPanel != null)
                bossIntroPanel.SetActive(true);

            if (invisibleWall != null)
                invisibleWall.SetActive(true);
        }
    }

    public void OnStartTrialButtonPressed()
    {
        if (bossIntroPanel != null)
            bossIntroPanel.SetActive(false);

        if (invisibleWall != null)
            invisibleWall.SetActive(false);

        StartTrial();
    }

    public void StartTrial()
    {
        trialMusic.Play();

        if (trialActive) return;

        trialActive = true;
        trialComplete = false;
        trialFailed = false;
        currentWave = 0;

        Debug.Log("Boss trial started!");

        if (timerText  != null)
        {
            timerText.SetActive(true);
        }

        if (trialStartText != null)
        {
            trialStartText.SetActive(true);
            Invoke(nameof(HideTrialStartText), 2f);
        }

        if (trialTimer != null)
        {
            trialTimer.gameObject.SetActive(true);
        }

        RegisterActiveEnemies(enemyWaves[0]);
    }

    void Update()
    {
        if (!trialActive || trialComplete || waveTransitioning || trialFailed)
            return;

        enemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        if (trialTimer != null && trialTimer.timer <= 0 && !trialComplete)
        {
            TrialFailed();
            return;
        }

        if (enemies.Count == 0 && currentWave < enemyWaves.Length)
        {
            currentWave++;
            if (currentWave < enemyWaves.Length)
            {
                waveTransitioning = true;
                Invoke(nameof(StartNextWave), waveDelay);
            }
            else
            {
                trialComplete = true;
                OnTrialComplete();
            }
        }
    }

    private void StartNextWave()
    {
        waveTransitioning = false;
        ActivateWave(currentWave);
        if (trialTimer != null)
            trialTimer.AddTime();
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
            ShowWaveName(wave.name);
        }
    }

    private void RegisterActiveEnemies(GameObject wave)
    {
        foreach (Transform child in wave.transform)
            enemies.Add(child.gameObject);
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
        waveNameText.gameObject.SetActive(false);
    }
    private void HideTrialStartText()
    {
        trialStartText.SetActive(false);
    }

    private void OnTrialComplete()
    {
        trialMusic.Stop();

        Debug.Log("Boss defeated!");
        timerText?.SetActive(false);
        trialTimer?.gameObject.SetActive(false);
        successPanel?.SetActive(true);
        trialWall?.SetActive(false);

        // HEAL AND REWARD THE PLAYER AND PARTY
        if (PlayerInventory.Instance != null)
            SpawnTrialRewards();
    }

    private void TrialFailed()
    {
        Debug.Log("Boss trial failed — time ran out!");
        trialFailed = true;
        trialActive = false;

        foreach (var wave in enemyWaves)
            if (wave != null) wave.SetActive(false);

        if (trialTimer != null && trialTimer.gameOverPanel != null)
            trialTimer.gameOverPanel.SetActive(true);
    }

    public void Continue()
    {
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
