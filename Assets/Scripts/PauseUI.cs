using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
	public void PauseGame()
	{
		Time.timeScale = 0f;
		Debug.Log("GAME PAUSED - Time Scale: " + Time.timeScale);
	}

	public void ResumeGame()
	{
		Time.timeScale = 1f;
	}
	
	public void MainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}
}
