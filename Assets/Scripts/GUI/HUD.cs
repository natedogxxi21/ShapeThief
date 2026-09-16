using TMPro;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public partial class HUD : MonoBehaviour
{
	[AutoStaticsCleanup] public static HUD Instance;
	[SerializeField] PlayerTouchMovement playerMove;
	[SerializeField] PlayerShift playerShift;
	[SerializeField] ScreenButton shiftButton;
	[SerializeField] ScreenButton runButton;
	[SerializeField] TMP_Text moneyCounterText;

	[SerializeField] RectTransform[] reservedRects;

	void Start()
	{
		Instance = this;
		shiftButton.button.onClick.AddListener(ShiftButtonPressed);
		runButton.button.onClick.AddListener(RunButtonPressed);
		playerShift.ShiftEvent.AddListener(OnShiftEvent);
		PlayerStats.OnMoneyChanged.AddListener(OnMoneyChanged);
	}

	void Update()
	{
		if (playerShift.ShiftTimer > 0)
		{
			shiftButton.label.text = playerShift.ShiftTimer.ToString("F1");
			shiftButton.background.fillAmount = 1;
		}
		else if (playerShift.ShiftCooldown > 0)
		{
			shiftButton.label.text = "Shift";
			shiftButton.background.fillAmount = 1 - playerShift.ShiftCooldownPercent;
		}
		else
		{
			shiftButton.label.text = "Shift";
			shiftButton.background.fillAmount = 1;
		}
	}

	public bool IsScreenPosReserved(Vector2 screenPos)
	{
		Vector2 anchorPos = new(screenPos.x / Screen.width, screenPos.y / Screen.height);

		foreach (RectTransform rect in reservedRects)
		{
			if (anchorPos.x >= rect.anchorMin.x
				&& anchorPos.x <= rect.anchorMax.x
				&& anchorPos.y >= rect.anchorMin.y
				&& anchorPos.y <= rect.anchorMax.y)
			{ return true; }
		}
		return false;
	}

	void OnMoneyChanged(int prev, int money)
	{
		moneyCounterText.text = $"${money}";
	}

	void OnShiftEvent() => SetShiftAvailable(playerShift.ShiftAvailable);

	public void SetShiftAvailable(bool ready) => shiftButton.button.interactable = ready;

	public void ShiftButtonPressed() => playerShift.Shift();
	public void RunButtonPressed()
	{
		bool running = playerMove.ToggleRun();
		runButton.label.text = running ? "Walk" : "Run";
	}
}