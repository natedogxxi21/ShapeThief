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

    public void Setup(UpgradeManager upgradeManager, int Index, string title, string description,int cost)
    {
        manager = upgradeManager;
        upgradeIndex = Index;
        currentCost = cost;
        titleText.text = title;
        descriptionText.text = description;
        costText.text = $"S{cost}";
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
            costText.color = Color.white;
        }
    }
    private void BuyUpgrade()
    {
        if (manager.PurchaseUpgrade(upgradeIndex))
        {
            Debug.Log("Upgrade puechased:" + titleText.text);
            gameObject.SetActive(false);
        }
    }
}


