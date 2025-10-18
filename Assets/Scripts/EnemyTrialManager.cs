using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyTrialManager : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyParent; 
    public GameObject successPanel;
    public GameObject trialWall;

    private List<GameObject> enemies = new List<GameObject>();
    private bool trialActive = false;
    private bool trialComplete = false;

    void Start()
    {
        if (enemyParent != null)
        {
            
            foreach (Transform child in enemyParent.transform)
            {
                if (child.gameObject != null)
                    enemies.Add(child.gameObject);
            }
        }

        if (successPanel != null)
            successPanel.SetActive(false);

        if (trialWall != null)
            trialWall.SetActive(true);
    }

    void Update()
    {
        if (!trialActive || trialComplete)
            return;

        //Removes destroyed or inactive enemies
        enemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        
        if (enemies.Count == 0)
        {
            trialComplete = true;
            OnTrialComplete();
        }
    }

    public void StartTrial()
    {
        trialActive = true;
        trialComplete = false;

        //Refreshes enemy list for more enemies in case
        enemies.Clear();
        if (enemyParent != null)
        {
            foreach (Transform child in enemyParent.transform)
            {
                if (child.gameObject != null)
                    enemies.Add(child.gameObject);
            }
        }

        Debug.Log($"Trial started with {enemies.Count} enemies");
    }

    private void OnTrialComplete()
    {
        Debug.Log("Trial complete! All enemies defeated.");

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
