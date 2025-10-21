using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyTrialManager : MonoBehaviour
{
    [Header("References")]
    public GameObject[] enemyWaves;
    public GameObject successPanel;
    public GameObject trialWall;

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

        if (trialWall != null)
            trialWall.SetActive(true);

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

        if (trialAudioSource != null)
            trialAudioSource.Play();


        if (trialActive) return;

        trialActive = true;
        trialComplete = false;
        currentWave = 0;

        


        ActivateWave(currentWave);

        Debug.Log("Trial started! First wave activated.");
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
            Debug.Log($" Wave {index + 1} - \"{wave.name}\" activated with {enemies.Count} enemies.");

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
        Debug.Log("Trial complete! All waves cleared.");

        if (trialAudioSource != null)
            trialAudioSource.Stop();


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
}
