using UnityEngine;

public class LavaZone : MonoBehaviour
{
    public float damagePerSecond = 5f;
    private PlayerController playerController;
    private ScreenFlash screenFlash;
    private bool isPlayerInZone = false;
    private AudioSource sizzleAudio;

    private void Start()
    {
       screenFlash = FindObjectOfType<ScreenFlash>();
        sizzleAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null )
            {
                isPlayerInZone = true;
                InvokeRepeating(nameof(ApplyLavaDamage), 0f, 1f);
                Debug.Log("You touched the lava!");

                if (sizzleAudio != null && !sizzleAudio.isPlaying)
                    sizzleAudio.Play();
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
                CancelInvoke(nameof(ApplyLavaDamage));
                playerController = null;

                if (sizzleAudio != null && sizzleAudio.isPlaying)
                    sizzleAudio.Stop();
            }
        }
    }

    private void ApplyLavaDamage()
    {
        if (isPlayerInZone && playerController != null)
        {
            playerController.TakeDamage(damagePerSecond);

            if (screenFlash != null)
                screenFlash.Flash();


        }
    }
}
