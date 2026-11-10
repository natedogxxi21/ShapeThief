using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public partial class PlayerStats : MonoBehaviour
{
	[AutoStaticsCleanup]
	public static int Money { get; private set; }

	[AutoStaticsCleanup]
	public static int TotalMoneyCollected { get; private set; }

	[AutoStaticsCleanup]
	public static int HighestMoneyCollected { get; private set; }



	public class MoneyChangedEvent : UnityEvent<int, int> { };

	[AutoStaticsCleanup]
	public static MoneyChangedEvent OnMoneyChanged = new();

	[AutoStaticsCleanup]
	public static float GuardDetectRadius { get; private set; } = 10;

	public void Update()
	{
		HighestMoneyCollected = PlayerPrefs.GetInt("High Score", HighestMoneyCollected);
	}

	public static void ChangeMoney(int deltaMoney)
	{
		int prev = Money;
		Money += deltaMoney;


		if (deltaMoney > 0)
			TotalMoneyCollected += deltaMoney;

		if (TotalMoneyCollected > HighestMoneyCollected)
		{
			HighestMoneyCollected += deltaMoney;
			
		}

        PlayerPrefs.SetInt("HighScore", HighestMoneyCollected);
        PlayerPrefs.Save();
        OnMoneyChanged?.Invoke(prev, Money);

	}

	public static void ResetMoney()
	{
		int previousMoney = Money;
		Money = 0;
		TotalMoneyCollected = 0;
		OnMoneyChanged?.Invoke(previousMoney, Money);
	}
}
