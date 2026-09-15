using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Events;

public partial class PlayerStats : MonoBehaviour
{
	[AutoStaticsCleanup]
	public static int Money { get; private set; }

	public class MoneyChangedEvent : UnityEvent<int, int> {};
	[AutoStaticsCleanup]
	public static MoneyChangedEvent OnMoneyChanged = new();

	public static void ChangeMoney(int deltaMoney)
	{
		int prev = Money;
		Money += deltaMoney;
		OnMoneyChanged.Invoke(prev, Money);
	}
}