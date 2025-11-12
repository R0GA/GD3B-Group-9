using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHolder : MonoBehaviour
{
    public static PlayerHolder Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern to ensure only one player instance exists
        if (Instance != null && Instance != this)
        {
            // Destroy the duplicate player
            Debug.Log("Destroying duplicate player instance");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

}