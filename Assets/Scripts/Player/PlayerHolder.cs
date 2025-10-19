using UnityEngine;

public class PlayerHolder : MonoBehaviour
{
    private void Awake()
    {
        // Prevent this GameObject (and its children) from being destroyed on scene load
        DontDestroyOnLoad(gameObject);
    }
}
