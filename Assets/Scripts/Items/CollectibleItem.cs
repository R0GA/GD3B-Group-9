using UnityEngine;

public abstract class CollectibleItem : MonoBehaviour
{
    [Header("Collectible Settings")]
    public float collectionRange = 2f;
    public float moveSpeed = 5f;
    public AudioClip collectSound;

    protected Transform playerTransform;
    protected bool isCollecting = false;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (isCollecting && playerTransform != null)
        {
            // Move towards player
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                moveSpeed * Time.deltaTime
            );

            // Check if reached player
            if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
            {
                Collect();
                Destroy(gameObject);
            }
        }
        else if (playerTransform != null &&
                 Vector3.Distance(transform.position, playerTransform.position) <= collectionRange)
        {
            // Start collecting when player is in range
            isCollecting = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
            Destroy(gameObject);
        }
    }

    protected abstract void Collect();

    protected void ShowNotification(string message, Sprite icon = null, Color? color = null)
    {
        CollectibleNotificationManager.Instance?.ShowNotification(message, icon, color);
    }
}