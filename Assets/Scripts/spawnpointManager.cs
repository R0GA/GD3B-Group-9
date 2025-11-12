using UnityEngine;
using UnityEngine.InputSystem;

public class spawnpointManager : MonoBehaviour
{
    [SerializeField] private Transform hubSpawnPoint;

    void Start()
    {
        if (PlayerRespawnManager.Instance != null && PlayerRespawnManager.Instance.player != null)
        {
            var player = PlayerRespawnManager.Instance.player;

            // Reposition
            player.transform.position = hubSpawnPoint.position;

            // Re-enable movement and input
            player.GetComponent<CharacterController>().enabled = true;
            player.GetComponent<PlayerInput>().enabled = true;
            player.GetComponent<PlayerController>().enabled = true;

            Debug.Log("Player repositioned and re-enabled in Hub.");
        }
    }
}
