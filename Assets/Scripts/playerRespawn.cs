using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class playerRespawn : MonoBehaviour
{
    public Vector3 lastSafePosition;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    public Transform groundCheckPoint;
    public float respawnOffsetDistance = 0.5f;

    public GameObject respawnMessageUI; // Drag your Canva image here
    public int flashCount = 4;
    public float flashInterval = 0.5f;

    void Update()
    {
        if (Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer))
        {
            Vector3 offset = -transform.forward * respawnOffsetDistance;
            lastSafePosition = transform.position + offset;
            Debug.Log("Safe position updated with offset: " + lastSafePosition);
        }
    }

    public void RespawnToSafePosition()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = lastSafePosition;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        if (cc != null) cc.enabled = true;

        StartCoroutine(FlashRespawnMessage());
    }

    private IEnumerator FlashRespawnMessage()
    {
        if (respawnMessageUI != null)
        {
            for (int i = 0; i < flashCount; i++)
            {
                respawnMessageUI.SetActive(true);
                yield return new WaitForSeconds(flashInterval);
                respawnMessageUI.SetActive(false);
                yield return new WaitForSeconds(flashInterval);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lastSafePosition, 0.2f);
    }
}
