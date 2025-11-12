using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager Instance;

    [Header("Respawn UI")]
    public GameObject respawnPanel;
    public Button respawnButton;
    //public Button quitButton;

    [Header("References")]
    public PlayerController player;

    private Vector3 lastCheckpointPosition;
    private bool hasCheckpoint = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        if (respawnButton != null)
            respawnButton.onClick.AddListener(RespawnPlayer);

        /*if (quitButton != null)
            quitButton.onClick.AddListener(QuitToHub);*/
    }

    public void SetCheckpoint(Vector3 checkpointPos)
    {
        lastCheckpointPosition = checkpointPos;
        hasCheckpoint = true;
    }

    public void HandlePlayerDeath(PlayerController playerRef)
    {
        player = playerRef;

        if (respawnPanel != null)
            respawnPanel.SetActive(true);

        Time.timeScale = 0f; // Pause the game
    }

    private void RespawnPlayer()
    {
        if (player == null) return;

        Time.timeScale = 1f;
        respawnPanel.SetActive(false);

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if (hasCheckpoint)
            player.transform.position = lastCheckpointPosition;
        else
            player.transform.position = Vector3.zero; // fallback

        player.health = player.maxHealth;
        player.FullHeal();
        player.isDead = false;
        player.gameObject.SetActive(true);
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<PlayerInput>().enabled = true;

        Debug.Log("Player respawned at checkpoint!");
    }

    /*private void QuitToHub()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
    }*/
}
