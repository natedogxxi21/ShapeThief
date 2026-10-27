using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Exit_Level : MonoBehaviour
{
    [Header("Exit Requirements")]
    [SerializeField] private int additionalMoneyRequired = 50;
	[SerializeField] string nextSceneName;
	[SerializeField] GameObject seeBehindPad;

	[Header("Upgrade Menu")]
	[SerializeField] private UpgradeSelectionUI upgradeMenu;

	[Header("UI")]
	public TMP_Text objectiveText;

    private int moneyAtLevelStart;
    private bool exitActivated;

	void Start()
	{
        moneyAtLevelStart = PlayerStats.Money;
		if(objectiveText != null) 
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

		OnMoneyChanged(PlayerStats.Money,PlayerStats.Money);
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

       if(PlayerStats.Money >= requiredTotal)
        {
            if(upgradeMenu == null)
            {
                return;
            }
            exitActivated = true;
            upgradeMenu.OpenUpgradeMenu(LoadNextLevel);
        }
        else
        {
            int stillNeeded = requiredTotal - PlayerStats.Money;

            objectiveText.text = "You Still need$ " + stillNeeded + "more before you can leave";
            objectiveText.gameObject.SetActive(true);
        }
    }
    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextSceneName);
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
