using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TimedTrialManager : MonoBehaviour
{
    [Header("References")]
    public GameObject[] enemyWaves;
    public GameObject successPanel;
    public GameObject failurePanel;  
    //public GameObject trialWall;
    public TrialTimer timerScript;   
    public int xpReward = 200;

    [Header("UI")]
    public Text waveNameText;
    public float waveNameDisplayTime = 3f;

    [Header("Wave Delay")]
    public float waveDelay = 2f;

    [Header("Audio")]
    public AudioSource trialAudioSource;

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

        if (successPanel != null)
            successPanel.SetActive(false);

        if (failurePanel != null)
            failurePanel.SetActive(false);

        /*if (trialWall != null)
            trialWall.SetActive(true);*/

        if (waveNameText != null)
            waveNameText.gameObject.SetActive(false);

        if (timerScript != null && timerScript.gameOverPanel != null)
            timerScript.gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!trialActive || trialComplete || waveTransitioning)
            return;

        
        if (timerScript != null && timerScript.timer <= 0)
        {
            OnTrialFailed();
            return;
        }

        //Remove all destroyed enemies
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

        if (trialAudioSource != null)
            trialAudioSource.Play();


        if (trialActive) return;

        trialActive = true;
        trialComplete = false;
        currentWave = 0;

        //Starts the timer
        if (timerScript != null)
        {
            timerScript.enabled = true;
        }

        ActivateWave(currentWave);

        Debug.Log("Timed Trial started! First wave activated.");
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

            foreach (Transform child in wave.transform)
            {
                if (child.gameObject != null)
                    enemies.Add(child.gameObject);
            }

            string waveName = wave.name;
            Debug.Log($"Wave {index + 1} - \"{wave.name}\" activated with {enemies.Count} enemies.");

            ShowWaveName(waveName);
        }
    }

    private void ShowWaveName(string name)
    {
        if (waveNameText != null)
        {
            waveNameText.text = $"Wave {currentWave + 1}: {name}";
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

    private void OnTrialComplete()
    {
        Debug.Log("Timed Trial complete! All waves cleared.");

        if (trialAudioSource != null)
            trialAudioSource.Stop();

        // HEAL AND REWARD THE PLAYER AND PARTY
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.GrantTrialRewardsToParty(xpReward);
        }

        if (successPanel != null)
            successPanel.SetActive(true);

        /*if (trialWall != null)
            trialWall.SetActive(false);*/

        if (timerScript != null)
            timerScript.enabled = false;
    }

    private void OnTrialFailed()
    {
        if (trialComplete) return;

        trialActive = false;
        trialComplete = true;
        Debug.Log("Timed Trial failed! Time ran out.");

        if (failurePanel != null)
            failurePanel.SetActive(true);

        /*if (trialWall != null)
            trialWall.SetActive(false);*/
    }

    public void Continue()
    {
        if (successPanel != null)
            successPanel.SetActive(false);
        if (failurePanel != null)
            failurePanel.SetActive(false);
    }
}
