using UnityEngine;
public class UpgradeManager : MonoBehaviour
{
        [Header("References")]
        [SerializeField] private CountdownTimer countdownTimer;

        [Header("Time Upgrade")]
        [SerializeField] private int timeUpgradeCost = 100;
        [SerializeField] private float extraTime = 30f;


    private void OnCollisionEnter(Collision collision)
    {
        BuyExtraTime();
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
    }
