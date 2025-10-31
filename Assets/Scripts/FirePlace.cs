using UnityEngine;

public class FirePlace : MonoBehaviour
{
    public float healPerSecond = 3f;
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
                dimPanel.SetActive(false);
                InvokeRepeating(nameof(HealPlayer), 1f, 1f);
                Debug.Log("You are warming up, health regenerating!");
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
                dimPanel.SetActive(true);
                CancelInvoke(nameof(HealPlayer));
                playerController = null;
            }
        }
    }

    private void HealPlayer()
    {
        if (isPlayerInZone && playerController != null)
        {
            playerController.Heal(healPerSecond);
        }
    }
}
