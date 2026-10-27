using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private PlayerTouchMovement playerMovement;

    [Header("Time Upgrade")]
    [SerializeField] public int timeUpgradeCost = 100;
    [SerializeField] private float extraTime = 30f;

    [Header("Smoke Bomb")]
    [SerializeField] private ParticleSystem SmokeBomb;
    [SerializeField] private Transform Player;
    [SerializeField] private GuardVision[] guards;
    [SerializeField] private float blindDuration = 4f;
    [SerializeField] public int SmokeBombCost = 50;

    [Header("Stamina Upgrade")]
    [SerializeField] public int staminaUpgradeCost = 20;
    [SerializeField] private float extraStamina = 25f;

    private bool smokeBombObtained = false;
    private bool smokeBombUsed = false;


    public void Start()
    {
        if (SmokeBomb != null)
        {
            SmokeBomb.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        }

        if (countdownTimer != null && UpgradeData.BonusTime > 0f)
            countdownTimer.AddTime(UpgradeData.BonusTime);

        if (playerMovement != null && UpgradeData.BonusStamina > 0f)
            playerMovement.UpgradeStamina(UpgradeData.BonusStamina);

        if (UpgradeData.HasSmokeBomb && HUD.Instance != null)
            HUD.Instance.AddSmokeBombToInventory();
    }


    public bool PurchaseUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 0:
                return BuyExtraTime();

            case 1:
                if (smokeBombObtained)
                    return false;

                ObtainSmokeBomb();
                return true;


                case 2:
                return BuyStaminaUpgrade();

            default:
                Debug.LogWarning("Upgrade is broken");
                    return false;
        }
    }
    /*private void OnCollisionEnter(Collision collision)
    {
        BuyExtraTime();
        ObtainSmokeBomb();
        BuyStaminaUpgrade();

    }*/
    public bool BuyExtraTime()
    {
        if (PlayerStats.Money < timeUpgradeCost)
        {
            Debug.Log("Not enough money!");
            return false;
        }

        PlayerStats.ChangeMoney(-timeUpgradeCost);
        UpgradeData.BonusTime += extraTime;
        countdownTimer.AddTime(extraTime);

        Debug.Log("Time upgrade purchased!");
        return true;
    }

    public void ObtainSmokeBomb()
    {
        if (UpgradeData.HasSmokeBomb)
            return;

        UpgradeData.HasSmokeBomb = true;

        if(HUD.Instance != null)
        HUD.Instance.AddSmokeBombToInventory();
    }

    public void UseSmokeBomb()
    {
        if (!smokeBombObtained)
            return;

        if (SmokeBomb == null || Player == null)
        {
            return;
        }
        smokeBombObtained = false;

        SmokeBomb.transform.position = Player.position;
        SmokeBomb.Stop();
        SmokeBomb.Play();

        foreach (GuardVision guard in guards)
        {
            if (guard != null)
                guard.Blind(blindDuration);

        }

        HUD.Instance.RemoveSmokeBombFromInventory();

    }

    public bool BuyStaminaUpgrade()
    {
        if (PlayerStats.Money < staminaUpgradeCost)
        {
            Debug.Log("Something wronger");
            return false;
        }
        if (playerMovement == null)
        {
            Debug.Log("Something wrong");
            return false;
        }
        PlayerStats.ChangeMoney(-staminaUpgradeCost);
        UpgradeData.BonusStamina += extraStamina;
        playerMovement.UpgradeStamina(extraStamina);
        Debug.Log("Stamina upgraded");
        return true;
    }
}
