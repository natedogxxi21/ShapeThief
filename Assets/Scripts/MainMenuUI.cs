using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.PlayerLoop;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] GameObject mainMenuPanel;
	[SerializeField] GameObject playPanel;
	[SerializeField] GameObject creditsPanel;
	[SerializeField] TMP_Text HighScoreMoney;
	[SerializeField] TMP_Text LevelScore;


	public void Start()
	{
		HighScoreMoney.text = "Most Money Collected: $" + PlayerStats.HighestMoneyCollected;
		LevelScore.text = "Highest Level Reached: " + Exit_Level.levelHighScore;
		gameObject.SetActive(true);
	}
	public void Play()
	{
		playPanel.SetActive(true);
		mainMenuPanel.SetActive(false);
	}

	public void StartGame(bool tutorial)
	{
		SceneManager.LoadScene(tutorial ? "Tutorial Scene" : "Nathan Scene");
	}

	public void QuitGame()
	{
		Application.Quit();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

	public void OpenCredits()
	{
		creditsPanel.SetActive(true);
		mainMenuPanel.SetActive(false);
	}

	public void CloseCredits()
	{
		creditsPanel.SetActive(false);
		mainMenuPanel.SetActive(true);
	}

	public void DeleteHighScore()
	{
        PlayerStats.DeleteHighScore();
        HighScoreMoney.text = "Most Money Collected: $" + PlayerStats.HighestMoneyCollected;
		LevelScore.text = "Highest Level Reached: " + Exit_Level.levelHighScore;
    }
}
