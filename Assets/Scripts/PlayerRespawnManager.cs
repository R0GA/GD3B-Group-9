using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager Instance;

    [Header("Respawn UI")]
    public GameObject respawnPanel;
    public Button respawnButton;
    public Button returnToHubButton;

    [Header("References")]
    public PlayerController player;

    private Vector3 lastCheckpointPosition;
    private string checkpointSceneName;
    private bool hasCheckpoint = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        if (respawnButton != null)
            respawnButton.onClick.AddListener(RespawnPlayer);

        if (returnToHubButton != null)
            returnToHubButton.onClick.AddListener(ReturnToHub);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReconnectUI());
        StartCoroutine(ReenablePlayerComponents());
        Time.timeScale = 1f;
    }

    private IEnumerator ReconnectUI()
    {
        yield return null; // wait one frame

        GameObject panelObj = GameObject.FindWithTag("deathPanel");
        if (panelObj != null)
        {
            respawnPanel = panelObj;
            respawnPanel.SetActive(false);
            Debug.Log("Respawn panel re-linked after scene load.");
        }
        else
        {
            Debug.LogWarning("Respawn panel not found after scene load.");
        }

        if (respawnPanel != null)
        {
            Button[] buttons = respawnPanel.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name.Contains("Respawn"))
                {
                    respawnButton = btn;
                    respawnButton.onClick.RemoveAllListeners();
                    respawnButton.onClick.AddListener(RespawnPlayer);
                }
                else if (btn.name.Contains("ReturnToHub"))
                {
                    returnToHubButton = btn;
                    returnToHubButton.onClick.RemoveAllListeners();
                    returnToHubButton.onClick.AddListener(ReturnToHub);
                }
            }
        }
    }

    private IEnumerator ReenablePlayerComponents()
    {
        yield return null; // wait one frame

        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            var input = player.GetComponent<PlayerInput>();
            var charController = player.GetComponent<CharacterController>();

            if (charController != null) charController.enabled = true;
            if (input != null)
            {
                input.enabled = true;
                input.ActivateInput(); // critical for jump to work
            }
            if (controller != null) controller.enabled = true;

            player.isDead = false;
            Debug.Log("Player components re-enabled after scene load.");
        }
    }

    public void SetCheckpoint(Vector3 checkpointPos)
    {
        lastCheckpointPosition = checkpointPos;
        checkpointSceneName = SceneManager.GetActiveScene().name;
        hasCheckpoint = true;

        Debug.Log($"Checkpoint set at {checkpointPos} in scene: {checkpointSceneName}");
    }

    public void HandlePlayerDeath(PlayerController playerRef)
    {
        player = playerRef;

        if (respawnPanel == null)
        {
            GameObject panelObj = GameObject.FindWithTag("deathPanel");
            if (panelObj != null)
            {
                respawnPanel = panelObj;
                Debug.Log("Respawn panel re-linked during death.");
            }
        }

        if (respawnPanel != null)
            respawnPanel.SetActive(true);
        else
            Debug.LogWarning("Respawn panel not found!");

        Time.timeScale = 0f;

        if (player != null)
        {
            player.GetComponent<PlayerController>().enabled = false;
            Debug.Log("PlayerController disabled due to death.");
        }
    }

    private void RespawnPlayer()
    {
        if (player == null) return;

        Time.timeScale = 1f;
        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        string currentScene = SceneManager.GetActiveScene().name;

        if (hasCheckpoint && currentScene != checkpointSceneName)
        {
            Debug.Log($"Loading checkpoint scene: {checkpointSceneName}");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(checkpointSceneName);
            while (!asyncLoad.isDone)
                yield return null;

            yield return new WaitForSecondsRealtime(0.1f);
        }

        if (player != null)
        {
            player.transform.position = hasCheckpoint ? lastCheckpointPosition : Vector3.zero;
            player.health = player.maxHealth;
            player.FullHeal();
            player.isDead = false;
            player.gameObject.SetActive(true);
            player.GetComponent<CharacterController>().enabled = true;
            player.GetComponent<PlayerInput>().enabled = true;
            player.GetComponent<PlayerController>().enabled = true;

            Debug.Log("Player respawned at checkpoint.");
        }
    }

    public void ReturnToHub()
    {
        Debug.Log("Returning to Hub. Resetting trial state and clearing checkpoint.");
        ClearCheckpoint();
        SceneManager.LoadScene("Hub");
    }

    public void ResetPlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
            Debug.Log("Trial player destroyed.");
        }
    }

    public void ClearCheckpoint()
    {
        hasCheckpoint = false;
        lastCheckpointPosition = Vector3.zero;
        checkpointSceneName = "";

        Debug.Log("Checkpoint data cleared.");
    }
}
