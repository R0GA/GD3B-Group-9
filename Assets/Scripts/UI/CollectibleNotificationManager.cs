using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CollectibleNotificationManager : MonoBehaviour
{
    public static CollectibleNotificationManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject notificationPrefab;
    public Transform notificationPanel;
    public int maxNotifications = 5;

    [Header("Notification Settings")]
    public float notificationDuration = 3f;
    public float fadeDuration = 0.5f;

    private Queue<CollectibleNotification> activeNotifications = new Queue<CollectibleNotification>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowNotification(string message, Sprite icon = null, Color? color = null)
    {
        if (notificationPrefab == null || notificationPanel == null)
        {
            Debug.LogWarning("Notification UI references not set!");
            return;
        }

        // Create new notification
        GameObject notificationObj = Instantiate(notificationPrefab, notificationPanel);
        CollectibleNotification notification = notificationObj.GetComponent<CollectibleNotification>();

        if (notification != null)
        {
            notification.Initialize(message, icon, color ?? Color.white, notificationDuration, fadeDuration);
            activeNotifications.Enqueue(notification);

            // Remove oldest notification if we exceed max
            if (activeNotifications.Count > maxNotifications)
            {
                CollectibleNotification oldest = activeNotifications.Dequeue();
                if (oldest != null)
                    oldest.FadeOut();
            }
        }
    }

    // Helper methods for specific collectible types
    public void ShowXPNotification(int xpAmount, bool isSuperXP = false)
    {
        string message = isSuperXP ?
            $"SUPER +{xpAmount} XP for all party creatures!" :
            $"+{xpAmount} XP!";

        Color color = isSuperXP ? Color.yellow : new Color(0.2f, 0.8f, 0.2f); // Green for normal, yellow for super
        ShowNotification(message, null, color);
    }

    public void ShowGachaNotification(int packAmount)
    {
        string message = $"+{packAmount} Creature Searches!";
        ShowNotification(message, null, new Color(0.8f, 0.2f, 0.8f)); // Purple
    }

    public void ShowHealNotification()
    {
        ShowNotification("Party Fully Healed!", null, new Color(0.2f, 0.8f, 0.8f)); // Cyan
    }
    public void ShowFaintNotification(string creatureName)
    {
        string message = $"{creatureName} fainted!";
        Color color = new Color(0.8f, 0.2f, 0.2f); // Red color for negative events
        ShowNotification(message, null, color);
    }
}