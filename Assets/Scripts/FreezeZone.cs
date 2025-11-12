using UnityEngine;

public class FreezeZone : MonoBehaviour
{
    public float damagePerSecond = 2f;
    public GameObject dimPanel;
    private PlayerController playerController;
    private bool isPlayerInZone = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null )
            {
                isPlayerInZone = true;
                dimPanel.SetActive(true);
                InvokeRepeating(nameof(ApplyDamage), 1f, 1f);
                Debug.Log("It's cold! You are taking damage!");
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerController != null)
            {
                isPlayerInZone = false;
                dimPanel.SetActive(false);
                CancelInvoke(nameof(ApplyDamage));
                playerController = null;
            }
        }
    }

    private void ApplyDamage()
    {
        if (isPlayerInZone && playerController != null)
        {
            playerController.TakeDamage(damagePerSecond);
        }
    }
}
