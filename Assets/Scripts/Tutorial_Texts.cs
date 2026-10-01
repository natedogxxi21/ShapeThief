using TMPro;
using UnityEngine;

public class Tutorial_Text : MonoBehaviour
{
	public TMP_Text tutorialText;
	[TextArea(2, 5)]
	public string message;

	private void OnTriggerEnter(Collider other)
	{
		tutorialText.gameObject.SetActive(true);
		tutorialText.text = message;
	}

	private void OnTriggerExit(Collider other)
	{
		tutorialText.gameObject.SetActive(false);
	}
}
