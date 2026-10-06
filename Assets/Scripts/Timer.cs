using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 60f;
    public bool timerIsRunning = false;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public GameObject GameOverPanel;

    [Header("Guard Settings")]
    public GameObject player;

    private GuardBehaviour[] guards;
    private void Start()
    {
        guards = FindObjectsByType<GuardBehaviour>();

        timerIsRunning = true;
        DisplayTime(timeRemaining);
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                timeRemaining = Mathf.Max(0f, timeRemaining);
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("You Got Caught");
                timeRemaining = 0;
                timerIsRunning = false;
                AlertAllGuards();
            }
        }
    }
    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    void AlertAllGuards()
    {
        foreach (GuardBehaviour guard in guards)
        {
            guard.Chase(player);
        }
            
    }
   /* void TriggerGameOver()
    {
        GameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }
   */

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}