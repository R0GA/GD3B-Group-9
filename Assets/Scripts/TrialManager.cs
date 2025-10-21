using UnityEngine;
using System.Collections.Generic;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    public TrialTimer trialTimer;
    public GameObject[] enemyWaves;     
    public GameObject successPanel;
    public GameObject failPanel;

    [Header("Wave Delay")]
    public float waveDelay = 2f;

    private List<GameObject> enemies = new List<GameObject>();
    private int currentWave = 0;
    private bool trialActive = false;
    private bool trialComplete = false;
    private bool waveTransitioning = false;

    void Start()
    {
        foreach (GameObject wave in enemyWaves)
        {
            if (wave != null)
                wave.SetActive(false);
        }

        
        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
        if (trialTimer != null) trialTimer.enabled = false;
    }

    void Update()
    {
        if (!trialActive || trialComplete || waveTransitioning) return;

        //Removes any destroyed enemies
        enemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        //Win Condition
        if (enemies.Count == 0 && currentWave < enemyWaves.Length)
        {
            currentWave++;

            if (currentWave > enemyWaves.Length)
            {
                waveTransitioning = true;
                Debug.Log($"Waiting {waveDelay} seconds before Wave {currentWave + 1}");
                Invoke(nameof(StartNextWave), waveDelay);
            }
            else
            {
                TrialSuccess();
            }
            
        }

        //Lose Condition
        if (trialTimer.timer <= 0 && !trialComplete)
        {
            TrialFail();
        }
    }

    
    public void StartTrial()
    {
        if (trialActive) return;

        trialActive = true;
        trialComplete = false;
        currentWave = 0;
        trialTimer.enabled = true;
        Debug.Log("Trial Started!");

        ActivateWave(currentWave);
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
        wave.SetActive(true);

        foreach (Transform child in wave.transform)
        {
            enemies.Add(child.gameObject);
        }

        Debug.Log($"Wave {index + 1} started with {enemies.Count} enemies!");
    }

    private void TrialSuccess()
    {
        trialComplete = true;
        trialActive = false;
        trialTimer.enabled = false;

        if (successPanel != null)
            successPanel.SetActive(true);

        Debug.Log("Trial Complete, all waves cleared before time");
    } 

    private void TrialFail()
    {
        trialComplete = true;
        trialActive = false;
        trialTimer.enabled = false;

        if (failPanel != null)
            failPanel.SetActive(true);

        Debug.Log("Trial failed, time ran out.");
    }
}
