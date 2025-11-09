using UnityEngine;
using System.Collections;

public class QuicksandZone : MonoBehaviour
{
    [Header("Quicksand settings")]
    public float stuckDuration = 3f;

    private PlayerController playerController;
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    private bool isPlayerStuck = false;
    private AudioSource mudAudio;

    private void Start()
    {
        mudAudio = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerStuck)
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null )
            {
                StartCoroutine(PlayerStuck());
            }
        }
    }

    private IEnumerator PlayerStuck()
    {
        isPlayerStuck = true;

        originalWalkSpeed = playerController.walkSpeed;
        originalSprintSpeed = playerController.sprintSpeed;

        playerController.walkSpeed = 0f;
        playerController.sprintSpeed = 0f;

        mudAudio.Play();

        Debug.Log("You're stuck in quicksand");

        yield return new WaitForSeconds(stuckDuration);

        playerController.walkSpeed = originalWalkSpeed;
        playerController.sprintSpeed = originalSprintSpeed; 
        isPlayerStuck = false;

        mudAudio.Stop();

        Debug.Log("Out of quicksand.");
    }
}
