using UnityEngine;
using UnityEngine.UI;

public class TimeTrigger : MonoBehaviour
{
    public GameObject timerManager;
    public Text timerText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timerManager.SetActive(true);
            timerText.gameObject.SetActive(true);
            
        }
    }
}
