using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] GameObject mainMenuPanel;
	[SerializeField] GameObject playPanel;
	[SerializeField] GameObject creditsPanel;

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
}
