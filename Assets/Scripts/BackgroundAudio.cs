using UnityEngine;

public class BackgroundAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }
}
