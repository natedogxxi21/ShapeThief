using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
public class UpgradeSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeData
    {
        public string title; [TextArea] public string description; public int index;
    }
    [Header("Refrences")]
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private GameObject panel;
    [SerializeField] private UpgradeCardUI[] cards;

    [Header("Availble Upgrades")]
    [SerializeField] private UpgradeData[] upgrades;

    private Action onMenuClosed;
    private void Start()
    {
        panel.SetActive(false);
    }

    public void OpenUpgradeMenu(Action closedCallback = null)
    {
        onMenuClosed = closedCallback;
        panel.SetActive(true);

        List<UpgradeData> availble = new List<UpgradeData>(upgrades);

        for (int i = 0; i < cards.Length; i++)
        {
            if (availble.Count == 0)
            {
                cards[i].gameObject.SetActive(true);
                continue;
            }
            int randomIndex = UnityEngine.Random.Range(0, availble.Count);

            UpgradeData chosen = availble[randomIndex];
            availble.RemoveAt(randomIndex);

            cards[i].Setup(upgradeManager, chosen.index, chosen.title, chosen.description);
        }
    }


    public void CloseUpgradeMenu()
    {
       panel.SetActive(false);
        Action callback = onMenuClosed;
        onMenuClosed = null;
        callback?.Invoke();
    }
}