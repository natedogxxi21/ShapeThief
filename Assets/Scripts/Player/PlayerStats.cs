using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Events;

public partial class PlayerStats : MonoBehaviour
{
	public static int Money { get; private set; }

	public class MoneyChangedEvent : UnityEvent<int, int> {};

	public static MoneyChangedEvent OnMoneyChanged = new();


    public void Awake()
    {
        ResetMoney();
    }
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