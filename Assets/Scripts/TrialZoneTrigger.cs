using UnityEngine;
public class TrialZoneTrigger : MonoBehaviour
{
    public EnemyTrialManager trialManager;
    private bool triggered = false;
    public Collider myTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            trialManager.StartTrial();

           
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            myTrigger.enabled = false;
        }
            
    }
}
