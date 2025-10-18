using UnityEngine;
using UnityEngine.InputSystem; 

public class TimeTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject promptUI;        
    public GameObject timerObject;
    public GameObject timerText;
    public GameObject finalEnemies;
    public Collider myTrigger;

    private bool playerInRange = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (timerObject != null)
            timerObject.SetActive(false); 
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

    private void Update()
    {
        
        bool eKeyPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool eastButtonPressed = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;

        
        if (playerInRange && ( eKeyPressed || eastButtonPressed))
        {
            Debug.Log("Interaction input detected! Starting trial...");

            if (timerObject != null)
                timerObject.SetActive(true);

            if (finalEnemies != null)
                finalEnemies.SetActive(true);

            if (timerText != null)
                timerText.SetActive(true);

            if (promptUI != null)
                promptUI.SetActive(false);

            var trialManager = FindObjectOfType<TrialManager>();
            if (trialManager != null)
                trialManager.StartTrial();

            if (myTrigger != null)
                myTrigger.enabled = false;
        }
    }
}
