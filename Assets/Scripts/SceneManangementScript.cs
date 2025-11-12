using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManangementScript : MonoBehaviour
{
    [Header("Hub settings")]
    public GameObject portalPanel;

    [Header("Start settings")]
    public GameObject controlsPanel;
    public GameObject creditsPanel;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            portalPanel.SetActive(true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            portalPanel.SetActive(false);
        }
    }

    public void Levels(string sceneName)
{
    Debug.Log($"Loading scene: {sceneName}");

    if (sceneName == "Hub" && PlayerRespawnManager.Instance != null)
    {
        PlayerRespawnManager.Instance.ClearCheckpoint(); // Only clear checkpoint
        Debug.Log("Clearing checkpoint for Hub transition.");
    }

    Time.timeScale = 1f;
    SceneManager.LoadScene(sceneName);
}

    public void PortalHidden()
    {
        portalPanel.SetActive(false);
    }

    public void ShowControls()
    {
        controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
