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
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame) //New Input System check
        {
            if (timerObject != null)
            {
                timerObject.SetActive(true);
                finalEnemies.SetActive(true);
                timerText.SetActive(true);
            }
                 

            if (promptUI != null)
                promptUI.SetActive(false);

            FindObjectOfType<TrialManager>().StartTrial();

            myTrigger.enabled = false;
            //gameObject.SetActive(false);
        }
    }
}
