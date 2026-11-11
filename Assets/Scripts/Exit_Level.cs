
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Exit_Level : MonoBehaviour
{
	[Header("Exit Requirements")]
	[SerializeField] private int additionalMoneyRequired = 20;
	[SerializeField] private GameObject seeBehindPad;
	[SerializeField] private bool isTutorial;

	[Header("Upgrade Menu")]
	UpgradeSelectionUI UpgradeMenu => UpgradeSelectionUI.Instance;

	[Header("UI")]
	public TMP_Text objectiveText;

	DungeonCreator dungeonCreator;
	DungeonSpawnManager spawnManager;

	private int nextUpgradeMoney;
	private bool upgradeMenuOpen;
	private bool playerInsidePad;

	void Awake()
	{
		// Require additional money from the player's current balance.
		nextUpgradeMoney = PlayerStats.Money + additionalMoneyRequired;

		if (objectiveText != null)
			objectiveText.gameObject.SetActive(false);

		if (seeBehindPad != null)
			seeBehindPad.SetActive(false);

		StartCoroutine(WaitForPlayerCoroutine());

		if (!isTutorial)
		{
			//Nathan code, GetComponents from these two codes so that the dungeon can be fully reloaded in a new function.
			dungeonCreator = GameObject.FindGameObjectWithTag("DungeonCreator").GetComponent<DungeonCreator>();
			spawnManager = GameObject.FindGameObjectWithTag("PrefabSpawner").GetComponent<DungeonSpawnManager>();
		}
	}



	IEnumerator WaitForPlayerCoroutine()
	{
		while (PlayerStats.OnMoneyChanged == null)
			yield return null;

		PlayerStats.OnMoneyChanged.AddListener(OnMoneyChanged);

		OnMoneyChanged(PlayerStats.Money, PlayerStats.Money);
	}

	private void OnMoneyChanged(int prev, int newMoney)
	{
		UpdateExitVisibility();

		// If the player is already standing on the pad,
		// check whether they have reached the next milestone.
		if (playerInsidePad && !upgradeMenuOpen)
			TryOpenUpgradeMenu();
	}

	private void UpdateExitVisibility()
	{
		if (seeBehindPad != null)
			seeBehindPad.SetActive(PlayerStats.Money >= nextUpgradeMoney);
		//Nathan code, Adding this in after closing the Upgrade manu so that the dungeon reloads.
		//RebuildDungeon();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.transform.root.CompareTag("Player"))
			return;

		playerInsidePad = true;
		TryOpenUpgradeMenu();
	}

	private void TryOpenUpgradeMenu()
	{
		if (upgradeMenuOpen)
			return;

		if (PlayerStats.Money < nextUpgradeMoney)
		{
			int stillNeeded = nextUpgradeMoney - PlayerStats.Money;

			if (objectiveText != null)
			{
				objectiveText.text = "Need <color=green>$" + stillNeeded + "</color> more!";
				objectiveText.gameObject.SetActive(true);
			}

			return;
		}

		if (objectiveText != null)
			objectiveText.gameObject.SetActive(false);

		if (isTutorial)
		{
			LoadNextLevel();
			return;
		}

		upgradeMenuOpen = true;

		UpgradeMenu.OpenUpgradeMenu(() =>
		{
			// Require another additional amount after closing.
			nextUpgradeMoney = PlayerStats.Money + additionalMoneyRequired;

			upgradeMenuOpen = false;

			UpdateExitVisibility();

			if (!isTutorial)
			{
				//Nathan code, Adding this in after closing the Upgrade manu so that the dungeon reloads.
				RebuildDungeon();
			}
		});
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.transform.root.CompareTag("Player"))
			return;

		playerInsidePad = false;

		if (objectiveText != null)
			objectiveText.gameObject.SetActive(false);
	}

	private void LoadNextLevel()
	{
		SceneManager.LoadScene("Nathan Scene");
	}

	public void RebuildDungeon()
	{
		dungeonCreator.CreateDungeon();
		spawnManager.SpawnDungeonContents();
	}
	
	private void OnDestroy()
	{
		PlayerStats.OnMoneyChanged?.RemoveListener(OnMoneyChanged);
	}
}
