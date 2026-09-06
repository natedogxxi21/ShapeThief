using TMPro;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.UI;

public partial class HUD : MonoBehaviour
{
	[AutoStaticsCleanup] public static HUD Instance;
	[SerializeField] PlayerShift player;
	[SerializeField] Button shiftButton;
	[SerializeField] TMP_Text shiftButtonLabel;
	[SerializeField] Image shiftButtonBackground;

	[SerializeField] RectTransform[] reservedRects;


	void Start()
	{
		Instance = this;
		player.ShiftEvent.AddListener(OnShiftEvent);
	}

	void Update()
	{
		if (player.ShiftTimer > 0)
		{
			shiftButtonLabel.text = player.ShiftTimer.ToString("F1");
			shiftButtonBackground.fillAmount = 1;
		}
		else if (player.ShiftCooldown > 0)
		{
			shiftButtonLabel.text = "Shift";
			shiftButtonBackground.fillAmount = 1 - player.ShiftCooldownPercent;
		}
		else
		{
			shiftButtonLabel.text = "Shift";
			shiftButtonBackground.fillAmount = 1;
		}
	}

	public bool IsPosReserved(Vector2 screenPos)
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

	void OnShiftEvent() => SetShiftAvailable(player.ShiftAvailable);

	public void SetShiftAvailable(bool ready) => shiftButton.interactable = ready;

	public void TriggerShift() => player.Shift();
}