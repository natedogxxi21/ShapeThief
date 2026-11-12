using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
	[SerializeField] private TMP_Text totalMoneyText;

	[SerializeField] private TMP_Text levelHighScoreText;

	public void Start()
	{
		totalMoneyText.text = "Total Money Collected: $" + PlayerStats.TotalMoneyCollected;
		levelHighScoreText.text = "Highest Level: " + Exit_Level.levelHighScore;
		gameObject.SetActive(true);
	}
	public void RestartGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(0);
        PlayerStats.ResetMoney();

	}

	public void QuitGame()
	{

	Application.Quit();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif

	}
}
