using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManangementScript : MonoBehaviour
{
    public GameObject portalPanel;
    

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
        SceneManager.LoadScene(sceneName);
    }

    public void BackToHub()
    {
        portalPanel.SetActive(false);
    }

}
