using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawnManager.Instance.SetCheckpoint(transform.position);
            Debug.Log("Checkpoint reached at: " + transform.position);
        }
    }
}
