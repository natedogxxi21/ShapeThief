using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Ui : MonoBehaviour
{

    private void Start()
    {
        InstructionPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
        GameOverPanel.SetActive(false);
    }
    public void OnEnable()
    {
        Time.timeScale = 0f;
        Debug.Log("GAME PAUSED - Time Scale: " + Time.timeScale);
    }


    public void CloseUpgradeScreen()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("GAME RESUMED - Time Scale: " + Time.timeScale);
    }
    public void Upgrade1()
    {
        //Upgrade Code will go here but for now it just closes ui!
        CloseUpgradeScreen();
    }
    public void Upgrade2()
    {
        //Upgrade Code will go here but for now it just closes ui!
        CloseUpgradeScreen();
    }
    public void Upgrade3()
    {
        //Upgrade Code will go here but for now it just closes ui!
        CloseUpgradeScreen();
    }
    public void PauseGame()
    {
        CloseUpgradeScreen();
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
    public GameObject InstructionPanel;
    public GameObject MainMenuPanel;
    public GameObject GameOverPanel;
    public void OpenInstructions()
    {
        InstructionPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
        GameOverPanel.SetActive(false);
    }
    public void CloseInstructions()
    {
        InstructionPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
        GameOverPanel.SetActive(false);
    }

    //When timer runs out it opens game over panel 
}
