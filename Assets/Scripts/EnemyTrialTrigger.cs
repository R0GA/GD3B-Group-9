using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyTrialTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject promptUI;
    public GameObject enemyParent;
    public Collider myTrigger;

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
        if (playerInRange && !trialStarted)
        {
            bool keyboardPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            bool controllerPressed = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

            if (keyboardPressed || controllerPressed)
            {
                StartTrial();
            }
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

        var trialManager = GetComponent<EnemyTrialManager>();
        if (trialManager != null)
            trialManager.StartTrial();

        Debug.Log("Trial started and Enemies are activated");

        myTrigger.enabled = false;
        //gameObject.SetActive(false);
    }
}
