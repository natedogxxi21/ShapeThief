using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CountdownTimer countdownTimer;
    PlayerTouchMovement PlayerMovement => PlayerTouchMovement.Instance;

    [Header("Time Upgrade")]
    [SerializeField] public int timeUpgradeCost = 100;
    [SerializeField] private float extraTime = 30f;

    [Header("Smoke Bomb")]
    [SerializeField] private ParticleSystem SmokeBomb;
    Transform Player => PlayerMovement.transform;
    private GuardVision[] guards;
    [SerializeField] private float blindDuration = 4f;
    [SerializeField] public int SmokeBombCost = 50;

    [Header("Stamina Upgrade")]
    [SerializeField] public int staminaUpgradeCost = 20;
    [SerializeField] private float extraStamina = 25f;

    private bool smokeBombObtained = false;
    //private bool smokeBombUsed = false;

    public void Awake()
    {
        guards = Object.FindObjectsByType<GuardVision>(FindObjectsInactive.Exclude);
    }
    public void Start()
    {
        if (SmokeBomb != null)
        {
            SmokeBomb.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        }

        if (countdownTimer != null && UpgradeData.BonusTime > 0f)
            countdownTimer.AddTime(UpgradeData.BonusTime);

        if (PlayerMovement != null && UpgradeData.BonusStamina > 0f)
            PlayerMovement.UpgradeStamina(UpgradeData.BonusStamina);
        RestoreSmokeBomb();

        Debug.Log("Smoke Bomb saved: " + UpgradeData.HasSmokeBomb);

    }

    private void RestoreSmokeBomb()
    {
        smokeBombObtained = UpgradeData.HasSmokeBomb;

        if (smokeBombObtained && HUD.Instance != null)
        {
            HUD.Instance.AddSmokeBombToInventory();
            Debug.Log("Smoke Bomb restored in the new level.");
        }
    }


    public bool PurchaseUpgrade(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 0:
                return BuyExtraTime();

            case 1:
                if (UpgradeData.HasSmokeBomb)
                    return false;

                if (PlayerStats.Money < SmokeBombCost)
                {
                    Debug.Log("You are Broke");
                    return false;
                }

                PlayerStats.ChangeMoney(-SmokeBombCost);
                ObtainSmokeBomb();
                return true;


            case 2:
                return BuyStaminaUpgrade();

            default:
                Debug.LogWarning("Upgrade is broken");
                return false;
        }
    }
    /* private void OnCollisionEnter(Collision collision)
     {
         BuyExtraTime();
         ObtainSmokeBomb();
         BuyStaminaUpgrade();

         Debug.Log("Smoke Bomb saved: " + UpgradeData.HasSmokeBomb);


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
        smokeBombObtained = true;

        if (HUD.Instance != null)
        {
            HUD.Instance.AddSmokeBombToInventory();
        }
        else
        {
            Debug.LogWarning("WHAT THE F");
        }
    }

    public void UseSmokeBomb()
    {
        guards = Object.FindObjectsByType<GuardVision>(FindObjectsInactive.Exclude);
        if (!UpgradeData.HasSmokeBomb)
            return;

        if (SmokeBomb == null || Player == null)
        {
            return;
        }
        UpgradeData.HasSmokeBomb = false;
        smokeBombObtained = false;

        SmokeBomb.transform.position = Player.position;
        SmokeBomb.Stop();
        SmokeBomb.Play();

        foreach (GuardVision guard in guards)
        {
            if (guard != null)
                guard.Blind(blindDuration);

        }

        if (HUD.Instance != null)
            HUD.Instance.RemoveSmokeBombFromInventory();

    }

    public bool BuyStaminaUpgrade()
    {
        if (PlayerStats.Money < staminaUpgradeCost)
        {
            Debug.Log("Something wronger");
            return false;
        }
        if (PlayerMovement == null)
        {
            Debug.Log("Something wrong");
            return false;
        }
        PlayerStats.ChangeMoney(-staminaUpgradeCost);
        UpgradeData.BonusStamina += extraStamina;
        PlayerMovement.UpgradeStamina(extraStamina);
        Debug.Log("Stamina upgraded");
        return true;
    }

    public int GetUpgradeCost(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 0:
                return timeUpgradeCost;
            case 1:
                return SmokeBombCost;
            case 2:
                return staminaUpgradeCost;
            default:
                return 0;
        }
    }
}
