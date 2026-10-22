using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private PlayerTouchMovement playerMovement;

    [Header("Time Upgrade")]
    [SerializeField] private int timeUpgradeCost = 100;
    [SerializeField] private float extraTime = 30f;

    [Header("Smoke Bomb")]
    [SerializeField] private ParticleSystem SmokeBomb;
    [SerializeField] private Transform Player;
    [SerializeField] private GuardVision[] guards;
    [SerializeField] private float blindDuration = 4f;

    [Header("Stamina Upgrade")]
    [SerializeField] private int staminaUpgradeCost = 20;
    [SerializeField] private float extraStamina = 25f;

    private bool smokeBombObtained = false;
    private bool smokeBombUsed = false;


    public void Start()
    {
        if (SmokeBomb != null)
            SmokeBomb.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    private void OnCollisionEnter(Collision collision)
    {
        BuyExtraTime();
        ObtainSmokeBomb();
        BuyStaminaUpgrade();

    }
    public bool BuyExtraTime()
    {
        if (PlayerStats.Money < timeUpgradeCost)
        {
            Debug.Log("Not enough money!");
            return false;
        }

        PlayerStats.ChangeMoney(-timeUpgradeCost);
        countdownTimer.AddTime(extraTime);

        Debug.Log("Time upgrade purchased!");
        return true;
    }

    public void ObtainSmokeBomb()
    {
        if (smokeBombObtained)
            return;

        smokeBombObtained = true;
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

        playerMovement.UpgradeStamina(extraStamina);
        Debug.Log("Stamina upgraded");
        return true;
    }
}
