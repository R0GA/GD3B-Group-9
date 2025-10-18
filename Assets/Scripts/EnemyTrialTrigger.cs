using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyTrialTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject promptUI;
    public GameObject enemyParent;

    private bool playerInRange = false;
    private bool trialStarted = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (enemyParent != null)
            enemyParent.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && !trialStarted && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartTrial();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    private void StartTrial()
    {
        trialStarted = true;
        playerInRange = false;

        if (promptUI != null)
            promptUI.SetActive(false);

        if (enemyParent != null)
            enemyParent.SetActive(true);

        Debug.Log("Trial started and Enemies are activated");

        gameObject.SetActive(false);
    }
}
