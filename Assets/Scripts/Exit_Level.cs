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

	[Header("Upgrade Menu")]
	[SerializeField] private UpgradeSelectionUI upgradeMenu;

	[Header("UI")]
	public TMP_Text objectiveText;

	private bool transitionStarted;

	void Start()
	{
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

		OnMoneyChanged(0,PlayerStats.Money);
	}

    private void OnDestroy()
    {
        if (PlayerStats.OnMoneyChanged != null)
            PlayerStats.OnMoneyChanged.RemoveListener(OnMoneyChanged);
    }

    private void OnMoneyChanged(int prev, int newMoney)
    {
        if (seeBehindPad != null)
            seeBehindPad.SetActive(newMoney >= requiredMoney);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player"))
            return;

        if (transitionStarted)
            return;

        if (PlayerStats.Money >= requiredMoney)
        {
            if (upgradeMenu == null)
            {
                Debug.LogError("Upgrade menu is not assigned!");
                return;
            }

            transitionStarted = true;

            if (objectiveText != null)
                objectiveText.gameObject.SetActive(false);

            // Open the upgrade menu and wait for it to close.
            upgradeMenu.OpenUpgradeMenu(LoadNextScene);
        }
        else
        {
            Debug.Log("You need " + requiredMoney + " money to exit.");

            if (objectiveText != null)
            {
                objectiveText.text =
                    "You still need $" +
                    (requiredMoney - PlayerStats.Money) +
                    " more before you can leave";

                objectiveText.gameObject.SetActive(true);
            }
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player"))
            return;

        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }
}
