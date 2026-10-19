using UnityEngine;
using UnityEngine.UI;
public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CountdownTimer countdownTimer;

    [Header("Time Upgrade")]
    [SerializeField] private int timeUpgradeCost = 100;
    [SerializeField] private float extraTime = 30f;

    [Header("Smoke Bomb")]
    [SerializeField] private ParticleSystem SmokeBomb;
    [SerializeField] private Transform Player;

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

        HUD.Instance.RemoveSmokeBombFromInventory();

    }


}