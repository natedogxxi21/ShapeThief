using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.UI;

public partial class HUD : MonoBehaviour
{
	[AutoStaticsCleanup] public static HUD Instance;
	[SerializeField] PlayerTouchMovement playerMove;
	[SerializeField] PlayerShift playerShift;
	[SerializeField] ScreenButton shiftButton;
	[SerializeField] ScreenButton runButton;
	[SerializeField] TMP_Text moneyCounterText;

	[SerializeField] ScreenButton inventoryButton;
	[SerializeField] Button inventoryCloseButton;
	[SerializeField] GameObject inventoryPanel;
	[SerializeField] RectTransform inventoryPanelContents;
	[SerializeField] GameObject inventoryPropButtonPrefab;
	readonly List<Prop> inventory = new();

	static readonly Color activeColor = new(1, .94f, 0);
	static readonly Color normalColor = Color.white;

	[SerializeField] RectTransform[] reservedRects;
	bool inventoryOpen = false;
	Coroutine shiftButtonFlashCoroutine;

	void Start()
	{
		Instance = this;
		shiftButton.button.onClick.AddListener(ShiftButtonPressed);
		inventoryButton.button.onClick.AddListener(InventoryButtonPressed);
		inventoryCloseButton.onClick.AddListener(() => { SetInventoryOpen(false); });
		runButton.button.onClick.AddListener(RunButtonPressed);
		playerShift.ShiftEvent.AddListener(OnShiftEvent);
		PlayerStats.OnMoneyChanged.AddListener(OnMoneyChanged);
	}

	void Update()
	{
		if (playerShift.ShiftTimer > 0)
		{
			shiftButton.label.text = playerShift.ShiftTimer.ToString("F1");
			shiftButton.image.fillAmount = 1;
		}
		else if (playerShift.ShiftCooldown > 0)
		{
			shiftButton.label.text = "Shift";
			shiftButton.image.fillAmount = 1 - playerShift.ShiftCooldownPercent;
		}
		else
		{
			shiftButton.label.text = "Shift";
			shiftButton.image.fillAmount = 1;
		}
	}

	public void FlashShiftButton()
	{
		if (shiftButtonFlashCoroutine != null) { StopCoroutine(shiftButtonFlashCoroutine); }
		shiftButtonFlashCoroutine = StartCoroutine(FlashShiftButtonCoroutine());

		IEnumerator FlashShiftButtonCoroutine(float duration = 1)
		{
			shiftButton.image.color = activeColor;
			float timer = duration;
			while (timer > 0)
			{
				timer -= Time.deltaTime;
				Color color = Color.Lerp(normalColor, activeColor, Mathf.Max(0, timer) / duration);
				shiftButton.image.color = color;
				yield return null;
			}
			shiftButton.image.color = normalColor;
		}
	}

	public bool IsScreenPosReserved(Vector2 screenPos)
	{
		string printString = "";
		printString += screenPos;
		foreach (RectTransform rect in reservedRects)
		{
			if (!rect.gameObject.activeInHierarchy)
				continue;

			Vector3[] corners = new Vector3[4];
			rect.GetWorldCorners(corners);
			Vector2 screenBL = corners[0];
			Vector2 screenTR = corners[2];
			printString += "\n" + screenBL + " " + screenTR;

			if (screenPos.x >= screenBL.x && screenPos.x <= screenTR.x &&
				screenPos.y >= screenBL.y && screenPos.y <= screenTR.y)
			{
				return true;
			}
		}
		
		return false;
	}


	void OnMoneyChanged(int prev, int money)
	{
		moneyCounterText.text = $"${money}";
	}

	void OnShiftEvent() => SetShiftAvailable(playerShift.ShiftAvailable);

	public void SetShiftAvailable(bool ready)
	{
		shiftButton.button.interactable = ready;
		if (ready) { FlashShiftButton(); }
	}

	public void ShiftButtonPressed() => playerShift.Shift();

	public void InventoryButtonPressed() => SetInventoryOpen(!inventoryOpen);
	void SetInventoryOpen(bool open)
	{
		inventoryOpen = open;
		inventoryPanel.SetActive(open);
	}

	public void RunButtonPressed()
	{
		bool running = playerMove.ToggleRun();
		runButton.label.text = running ? "Walk" : "Run";
	}

	public void AddToInventory(Prop prop)
	{
		if (!inventory.Contains(prop))
		{
			GameObject newButton = Instantiate(inventoryPropButtonPrefab, inventoryPanelContents);
			newButton.name = prop.propName + "Button";
			newButton.GetComponent<Button>().onClick.AddListener(() => { ShiftToInventoryProp(prop); });
			newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = prop.propName;
			inventory.Add(prop);
		}
	}

	void ShiftToInventoryProp(Prop prop)
	{
		if (playerShift.ShiftInventory(prop))
		{
			SetInventoryOpen(false);
		}
	}
}