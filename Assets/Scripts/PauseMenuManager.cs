using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("References")]
    public GameObject pauseMenu;
    //public GameObject hudUI;
    public bool isPaused = false;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null )
            playerInput.actions["Pause"].performed += ctx => TogglePause();

        if (pauseMenu != null )
            pauseMenu.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true; 
    }

    public void ResumeGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;
        isPaused= false;
    }

}
