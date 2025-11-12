using UnityEngine;
using UnityEngine.UI;

public class TimeExtention : MonoBehaviour
{
    public float addedTIme = 10.0f;

    public TrialTimer trialTimer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            trialTimer.AddTime();
            Destroy(gameObject);
        }
    }
}
