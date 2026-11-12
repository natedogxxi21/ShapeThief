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


	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void LoadHighScore()
	{
		HighestMoneyCollected = PlayerPrefs.GetInt("HighScore", 0);
		Exit_Level.levelHighScore = PlayerPrefs.GetInt("LevelScore", 1);
		Debug.Log("Loaded High Score: " + HighestMoneyCollected);
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
			PlayerPrefs.SetInt("HighScore", HighestMoneyCollected);
			PlayerPrefs.Save();
			Debug.Log("Saved High Score: " + HighestMoneyCollected);
		}

		if (Exit_Level.levelHighScore > 0)
		{
            PlayerPrefs.SetInt("LevelScore", Exit_Level.levelHighScore);
			PlayerPrefs.Save();
        }
		OnMoneyChanged?.Invoke(prev, Money);

	}

	public static void ResetMoney()
	{
		int previousMoney = Money;
		Money = 0;
		TotalMoneyCollected = 0;
		OnMoneyChanged?.Invoke(previousMoney, Money);
	}

	public static void DeleteHighScore()
	{
		HighestMoneyCollected = 0;
		Exit_Level.levelHighScore = 1;
        PlayerPrefs.DeleteKey("HighScore");
		PlayerPrefs.DeleteKey("LevelScore");
		
	}
}

