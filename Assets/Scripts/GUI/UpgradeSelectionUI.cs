using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class UpgradeSelectionUI : MonoBehaviour
{
	public static UpgradeSelectionUI Instance;

	[Serializable]
	public class UpgradeData
	{
		public string title; [TextArea] public string description; public int index;
	}

	[Header("Refrences")]
	[SerializeField] private UpgradeManager upgradeManager;
	[SerializeField] private GameObject panel;
	[SerializeField] private UpgradeCardUI[] cards;

	private int RerollCost = 20;
	[SerializeField] private TMP_Text RerollText;

    [Header("Availble Upgrades")]
	[SerializeField] private UpgradeData[] upgrades;


	private Action onMenuClosed;
	private void Awake()
	{
        GenerateRandomUpgrade();
        Instance = this;
		panel.SetActive(false);
       
    }

	public void OpenUpgradeMenu(Action closedCallback = null)
	{
		onMenuClosed = closedCallback;
		panel.SetActive(true);
        GenerateRandomUpgrade();
		RerollText.text = ($"Reroll: ${RerollCost}");
		Time.timeScale = 0f;

	}

	public void GenerateRandomUpgrade()
	{
		if (upgrades.Length == 0 || cards.Length == 0)
			return;

		UpgradeData chosen = upgrades[UnityEngine.Random.Range(0, upgrades.Length)];

		for (int i = 0; i < cards.Length; i++)
		{
			if (i == 0)
			{
				cards[i].gameObject.SetActive(true);

				cards[i].Setup(upgradeManager, chosen.index, chosen.title, chosen.description);
			}
			else
			{
				cards[i].gameObject.SetActive(false);
			}
		}
	}
	public void RerollUpgrade()
	{

		if (PlayerStats.Money >= RerollCost)
		{
			PlayerStats.ChangeMoney(-RerollCost);
			GenerateRandomUpgrade();
		}
		else
		{
			return;
		}
        if (RerollText == null)
            return;
        if (PlayerStats.Money < RerollCost)
        {
            RerollText.color = Color.red;
        }
        else
        {
            RerollText.color = Color.black;
        }
    }
	public void CloseUpgradeMenu()
	{
		panel.SetActive(false);
		Time.timeScale = 1.0f;

		Action callback = onMenuClosed;
        onMenuClosed = null;
        callback?.Invoke();
    }

}