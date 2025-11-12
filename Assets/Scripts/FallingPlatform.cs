using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    [Header("Platform settings")]
    public float fallDelay = 0.5f;
    public float respawnDelay = 3f;

    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFalling = false;
    private AudioSource rockAudio;

    private void Start()
    {
        rockAudio = GetComponent<AudioSource>();

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Platform needs rigidbody!");
            return;
        }

        rb.isKinematic = true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFalling)
        {
            StartCoroutine(Fall());
            rockAudio.Play();
        }
    }

    private IEnumerator Fall()
    {
        isFalling = true;
        yield return new WaitForSeconds(fallDelay);

        rb.isKinematic = false;
        Debug.Log("Platform falling!");

        

        yield return new WaitForSeconds(respawnDelay);

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        Debug.Log("Platform has reset");
        isFalling = false;

        rockAudio.Stop();
    }
}
