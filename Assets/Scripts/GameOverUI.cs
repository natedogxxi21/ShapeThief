using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
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
