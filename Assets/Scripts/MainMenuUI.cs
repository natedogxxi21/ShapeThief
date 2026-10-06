using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
	public GameObject MainMenuPanel;
	public GameObject InstructionPanel;

	public void StartGame()
	{
		SceneManager.LoadScene("Tutorial Scene");
	}

	public void QuitGame()
	{
		Application.Quit();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

	public void OpenInstructions()
	{
		InstructionPanel.SetActive(true);
		MainMenuPanel.SetActive(false);
	}

	public void CloseInstructions()
	{
		InstructionPanel.SetActive(false);
		MainMenuPanel.SetActive(true);
	}
}
