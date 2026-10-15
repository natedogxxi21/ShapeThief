using UnityEngine;

public class SceneKeypad : MonoBehaviour
{
	[SerializeField] GameObject keypadGUI;

	public void Open()
	{
		keypadGUI.SetActive(true);
	}
}