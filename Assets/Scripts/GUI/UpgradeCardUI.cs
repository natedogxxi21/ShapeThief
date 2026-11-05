using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button upgradeButton;

    private UpgradeManager manager;
    private int upgradeIndex;
    private int currentCost;

    public void Setup(UpgradeManager upgradeManager, int Index, string title, string description)
    {
        manager = upgradeManager;
        upgradeIndex = Index;
        currentCost = manager.GetUpgradeCost(Index);
        titleText.text = title;
        descriptionText.text = description;
        costText.text = $"${currentCost}";
        UpdateCost();
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(BuyUpgrade);
    }

    private void Update()
    {
        UpdateCost();
    }

    private void UpdateCost()
    {
        if (costText == null)
            return;
        if (PlayerStats.Money < currentCost)
        {
            costText.color = Color.red;
        }
        else
        {
            costText.color = Color.black;
        }
    }
    private void BuyUpgrade()
    {
        if (manager.PurchaseUpgrade(upgradeIndex))
        {
            Debug.Log("Upgrade purchased:" + titleText.text);
            gameObject.SetActive(false);
        }
    }
}


