using UnityEngine;
using System.Collections.Generic;

public class TrialManager : MonoBehaviour
{
    [Header("References")]
    public TrialTimer trialTimer;             
    public GameObject enemiesParent;     
    public GameObject successPanel;
    public GameObject failPanel;

    private List<GameObject> enemies = new List<GameObject>();
    private bool trialActive = false;
    private bool trialComplete = false;

    void Start()
    {
        if (enemiesParent != null)
        {
            foreach (Transform child in enemiesParent.transform)
            {
                enemies.Add(child.gameObject);
            }
        }

        
        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
    }

    void Update()
    {
        if (!trialActive || trialComplete) return;

        //Removes any destroyed enemies
        enemies.RemoveAll(e => e == null);

        //Win Condition
        if (enemies.Count == 0 && trialTimer.timer > 0)
        {
            trialComplete = true;
            trialActive = false;
            trialTimer.enabled = false;
            successPanel.SetActive(true);
            Debug.Log("Trial Complete: All enemies defeated!");
        }

        //Lose Condition
        if (trialTimer.timer <= 0 && enemies.Count > 0)
        {
            trialComplete = true;
            trialActive = false;
            failPanel.SetActive(true);
            Debug.Log("Trial Failed: Time ran out!");
        }
    }

    
    public void StartTrial()
    {
        trialActive = true;
        trialComplete = false;
        trialTimer.enabled = true;
        Debug.Log("Trial Started!");
    }
}
