using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class Exit_Level : MonoBehaviour
{
	[SerializeField] int requiredMoney = 10;
	[SerializeField] string nextSceneName;
	[SerializeField] GameObject seeBehindPad;
	public TMP_Text objectiveText;

	void Start() => StartCoroutine(WaitForPlayerCoroutine());

	IEnumerator WaitForPlayerCoroutine()
	{
		while (PlayerStats.OnMoneyChanged == null)
		{
			yield return null;
		}

		PlayerStats.OnMoneyChanged.AddListener(OnMoneyChanged);
	}

	private void OnMoneyChanged(int prev, int newMoney) => seeBehindPad.SetActive(newMoney >= requiredMoney);

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Something entered the exit pad!");
		Debug.Log("Tag: " + other.gameObject.tag);

		if (!other.transform.root.CompareTag("Player"))
			return;

		if (PlayerStats.Money >= requiredMoney)
		{
			SceneManager.LoadScene(nextSceneName);
		}
		else
		{
			Debug.Log("You need" + requiredMoney + " money to exit");
			objectiveText.text = "You still need $" + (requiredMoney - PlayerStats.Money) + " more before you can leave";
			objectiveText.gameObject.SetActive(true);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		objectiveText.gameObject.SetActive(false);
	}
}