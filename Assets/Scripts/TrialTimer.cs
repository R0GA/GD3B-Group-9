using UnityEngine;
using UnityEngine.UI;

public class TrialTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timer = 10.0f;
    public float addTime = 10.0f;
    public float timeDisplayFrame = 2.0f;

    [Header("UI References")]
    public Text timerText;
    public Text addTimeText;
    public GameObject gameOverPanel;

    [Header("Debug")]
    public bool testingGUI = false;

    private bool showGUI = false;

    

    void Start()
    {
        if (addTimeText != null)
            addTimeText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (timerText == null)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0)
            timer = 0;
        
        if( timer <= 0)
        {
            gameOverPanel.SetActive(true);
        }

        timerText.text = timer.ToString("00");

        if (showGUI)
        {
            if (addTimeText != null)
            {
                addTimeText.gameObject.SetActive(true);
                addTimeText.text = "+" + addTime.ToString("0");
                Invoke(nameof(HideAddTimeText), timeDisplayFrame);
            }
        }
    }

    void HideAddTimeText()
    {
        showGUI = false;
        if (addTimeText != null)
            addTimeText.gameObject.SetActive(false);
    }

    //Optional testing GUI for debugging
    void OnGUI()
    {
        if (testingGUI)
        {
            GUI.Box(new Rect(10, 10, 60, 30), timer.ToString("0"));

            if (showGUI)
            {
                GUI.Box(new Rect(10, 50, 60, 30), "+" + addTime.ToString("0"));
            }
        }
    }

    //Calls this to show the "+time" popup
    public void AddTime()
    {
        timer += addTime;
        showGUI = true;
    }
}
