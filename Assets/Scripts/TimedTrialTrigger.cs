using UnityEngine;
using UnityEngine.InputSystem;

public class TimedTrialTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject promptUI;
    public GameObject enemyParent;
    public GameObject timerObject;       
    public Collider myTrigger;

    private bool playerInRange = false;
    private bool trialStarted = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (enemyParent != null)
            enemyParent.SetActive(false);

        if (timerObject != null)
            timerObject.SetActive(false); 
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

        if (timerObject != null)
            timerObject.SetActive(true); 

       
        var trialManager = enemyParent.GetComponent<TimedTrialManager>();
        if (trialManager != null)
            trialManager.StartTrial();

        Debug.Log("Timed trial started! Enemies and timer are active.");

        myTrigger.enabled = false;
    }
}
