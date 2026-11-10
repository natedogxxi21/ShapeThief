using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Exit_Level : MonoBehaviour
{
	[Header("Exit Requirements")]
	[SerializeField] private int additionalMoneyRequired = 50;
	[SerializeField] GameObject seeBehindPad;
	[SerializeField] bool isTutorial;

	[Header("Upgrade Menu")]
	UpgradeSelectionUI UpgradeMenu => UpgradeSelectionUI.Instance;

	[Header("UI")]
	public TMP_Text objectiveText;

	private int moneyAtLevelStart;
	private bool exitActivated;

	void Start()
	{
		moneyAtLevelStart = PlayerStats.Money;
		if (objectiveText != null)
			objectiveText.gameObject.SetActive(false);

		StartCoroutine(WaitForPlayerCoroutine());
	}

	IEnumerator WaitForPlayerCoroutine()
	{
		while (PlayerStats.OnMoneyChanged == null)
		{
			yield return null;
		}

		PlayerStats.OnMoneyChanged.AddListener(OnMoneyChanged);

		OnMoneyChanged(PlayerStats.Money, PlayerStats.Money);
	}


	private void OnMoneyChanged(int prev, int newMoney)
	{
		if (seeBehindPad != null)
			seeBehindPad.SetActive(newMoney >= moneyAtLevelStart + additionalMoneyRequired);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.transform.root.CompareTag("Player"))
			return;
		if (exitActivated)
			return;

		int requiredTotal = moneyAtLevelStart + additionalMoneyRequired;

		if (PlayerStats.Money >= requiredTotal)
		{
			if (isTutorial)
			{
				LoadNextLevel();
			}
			else
			{
				exitActivated = true;
				UpgradeMenu.OpenUpgradeMenu(LoadNextLevel);
			}
		}
		else
		{
			int stillNeeded = requiredTotal - PlayerStats.Money;

			objectiveText.text = "Need <color=green>$" + stillNeeded + "</color> more!";
			objectiveText.gameObject.SetActive(true);
		}
	}

	private void LoadNextLevel()
	{
		SceneManager.LoadScene("Nathan Scene");
	}


	private void OnTriggerExit(Collider other)
	{
		if (!other.transform.root.CompareTag("Player"))
			return;

		objectiveText.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (PlayerStats.OnMoneyChanged != null)
			PlayerStats.OnMoneyChanged.RemoveListener(OnMoneyChanged);
	}
}
