using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button upgradeButton;

    private UpgradeManager manager;
    private int upgradeIndex;

    public void Setup(UpgradeManager upgradeManager, int Index, string title, string description)
    {
        manager = upgradeManager;
        upgradeIndex = Index;
        titleText.text = title;
        descriptionText.text = description;
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(BuyUpgrade);
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


