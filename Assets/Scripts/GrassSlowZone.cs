using UnityEngine;

public class GrassSlowZone : MonoBehaviour
{
    [Header("Speed settings")]
    public float slowMultiplier = 0.6f;

    private PlayerController playerController;
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    private bool isSlowed = false;
    private AudioSource grassAudio;

    private void Start()
    {
        grassAudio = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();

            if (playerController != null && !isSlowed)
            {
                originalWalkSpeed = playerController.walkSpeed;
                originalSprintSpeed = playerController.sprintSpeed;

                playerController.walkSpeed *= slowMultiplier;
                playerController.sprintSpeed *= slowMultiplier;

                isSlowed = true;
                Debug.Log("You have entered thick grass, you have slowed down!");

                if (grassAudio != null && !grassAudio.isPlaying)
                    grassAudio.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerController != null && isSlowed)
        {
            playerController.walkSpeed = originalWalkSpeed;
            playerController.sprintSpeed = originalSprintSpeed;

            isSlowed = false;
            Debug.Log("You have exited thick grass.");

            if (grassAudio != null && grassAudio.isPlaying)
                grassAudio.Stop();
        }
    }
}
