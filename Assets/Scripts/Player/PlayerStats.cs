using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Events;

public partial class PlayerStats : MonoBehaviour
{
	[AutoStaticsCleanup]
	public static int Money { get; private set; }

	public class MoneyChangedEvent : UnityEvent<int, int> { };

	[AutoStaticsCleanup]
	public static MoneyChangedEvent OnMoneyChanged = new();

	/// <summary>
	/// How far the player can see guards through walls (guard indicators are always visible if active)
	/// </summary>
	[AutoStaticsCleanup]
	public static float GuardDetectRadius { get; private set; } = 10;

	

	public static void ChangeMoney(int deltaMoney)
	{
		int prev = Money;
		Money += deltaMoney;
		OnMoneyChanged?.Invoke(prev, Money);
	}

	public static void ResetMoney()
	{
		int previousMoney = Money;
		Money = 0;
		OnMoneyChanged?.Invoke(previousMoney, Money);
	}
}