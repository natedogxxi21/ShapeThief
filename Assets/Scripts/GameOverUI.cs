using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
	[SerializeField] private TMP_Text totalMoneyText;
	public void Start()
	{
		totalMoneyText.text = "Total Money Collected: $" + PlayerStats.TotalMoneyCollected;
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
