using System;
using System.Collections;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public partial class TutorialFlow : MonoBehaviour
{
	// states
	[AutoStaticsCleanup] public static float looked = 0;
	const float lookReq = 1500f;
	[AutoStaticsCleanup] public static float moved = 0;
	const float moveReq = 7f;
	[AutoStaticsCleanup] public static bool looted;
	[AutoStaticsCleanup] public static bool selected;
	[AutoStaticsCleanup] public static bool shifted;
	[AutoStaticsCleanup] public static bool unshifted;
	[AutoStaticsCleanup] public static bool collected;
	[AutoStaticsCleanup] public static bool openedInventory;
	[AutoStaticsCleanup] public static bool usedInventory;
	[AutoStaticsCleanup] public static bool ran;

	[SerializeField] int delayFrames = 2;
	[SerializeField] bool enableTutorial = true;
	[SerializeField] bool skipDelays = false;
	[SerializeField] GameObject poofParticlesPrefab;
	[SerializeField] AudioClip poofClip;

	[Header("References")]
	[SerializeField] GameObject timerDisplay;
	// Tutorial prompts
	[SerializeField]
	GameObject
	lookPrompt, progress, bar, // point at right half, diagram of moving camera
	movePrompt, // point at left half of screen, diagram of moving plr
	lootArrow, moneyDisplay, lootReference, // have player collect gem then show money counter
	tapPropPrompt, shiftPropReference, // finger over the tap prop
	shiftPrompt, shiftButton, // finger over button, then tap again to unshift
	shiftTutDoor, shiftTutLoot, // room with conspicuous gards, reach loot
	collectPropReference, // tap the prop for the next step
	collectPrompt, collectButton, inventoryButton, // collect the prop and have it fly into inventory
	sneakPrompt, sneakTutDoor, sneakTutLoot,// sneak past guards facing away
	openInventoryPrompt, useInventoryPrompt, invTutDoor, invLoot, // open inventory and use what collected earlier
	runPrompt, runText, runButton; // run past conspicuous guards to the exit pad

	[SerializeField] GameObject[] shiftTutGuards, sneakTutGuards, invTutGuards, runTutGuards;

	void SetBar(float p) => bar.GetComponent<RectTransform>().anchorMax = new(Mathf.Clamp01(p), 1);

	// tutorial sequence
	IEnumerator Tutorial()
	{
		yield return Wait(1f);
		yield return LookStep();
		yield return Wait(1.5f);
		yield return MoveStep();
		yield return Wait(0.5f);
		yield return MoneyStep();
		yield return Wait(0.5f);
		yield return ShiftStep();
		yield return Wait(1f);
		yield return ShiftRoomStep();
		yield return Wait(1.0f);
		yield return UnshiftStep();
		yield return Wait(1.0f);
		yield return CollectStep();
		yield return Wait(1.0f);
		yield return SneakStep();
		yield return Wait(1.0f);
		yield return InventoryStep();
		yield return RunStep();
	}

	// individual steps
	IEnumerator LookStep()
	{
		looked = 0;
		Show(lookPrompt); Show(progress);
		SetBar(0);
		yield return WaitUntil(() => looked >= lookReq, () => SetBar(looked / lookReq));
		Hide(progress);
		yield return Wait(0.5f);
		Hide(lookPrompt);
	}

	IEnumerator MoveStep()
	{
		moved = 0;
		Show(movePrompt);
		SetBar(0);
		Show(progress);
		yield return WaitUntil(() => moved >= moveReq, () => SetBar(moved / moveReq));
		Hide(progress);
		yield return Wait(0.5f);
		Hide(movePrompt);
	}

	IEnumerator MoneyStep()
	{
		looted = false;
		Show(moneyDisplay);
		Show(lootReference);
		Show(lootArrow);
		yield return WaitUntil(() => looted);
		Hide(lootArrow);
	}

	IEnumerator ShiftStep()
	{
		selected = false;
		Show(shiftPropReference);
		Show(shiftButton);

		yield return WaitUntil(() => shifted, () =>
		{
			tapPropPrompt.SetActive(!selected);
			shiftPrompt.SetActive(selected);
		});

		Hide(tapPropPrompt);
		Hide(shiftPrompt);
	}

	IEnumerator ShiftRoomStep()
	{
		Show(shiftTutGuards);
		yield return null;
		Hide(shiftTutDoor);
		lootArrow.GetComponent<TrackObject>().target = shiftTutLoot.transform;
		Show(lootArrow);
		Show(shiftTutLoot);
		looted = false;
		yield return WaitUntil(() => looted);
		Hide(lootArrow);
		yield return Poof(shiftTutGuards, false);
	}

	IEnumerator UnshiftStep()
	{
		Show(shiftPrompt);
		yield return WaitUntil(() => unshifted);
		Hide(shiftPrompt);
	}

	IEnumerator CollectStep()
	{
		selected = false;
		tapPropPrompt.GetComponent<TrackObject>().target = collectPropReference.transform;
		Show(collectPropReference);

		yield return WaitUntil(() => collected, () =>
		{
			tapPropPrompt.SetActive(!selected);
			collectButton.SetActive(selected);
			collectPrompt.SetActive(selected);
		});

		Hide(tapPropPrompt);
		Hide(collectPrompt);
		Show(inventoryButton);
		inventoryButton.GetComponent<ScreenButton>().button.interactable = false;
		yield return Wait(1f);
	}

	IEnumerator SneakStep()
	{
		looted = false;
		lootArrow.GetComponent<TrackObject>().target = sneakTutLoot.transform;
		Show(sneakPrompt);
		Show(sneakTutGuards);
		yield return null;
		Hide(sneakTutDoor);
		Show(sneakTutLoot);
		Show(lootArrow);
		float t = 0;
		while (t < 3)
		{
			float t2 = Ease(Mathf.Clamp01(t - 2));
			Vector2 min = Vector2.Lerp(new(.3f, .1f), new(.4f, .7f), t2);
			Vector2 max = Vector2.Lerp(new(.7f, .9f), new(.6f, 1.025f), t2);
			sneakPrompt.GetComponent<RectTransform>().anchorMin = min;
			sneakPrompt.GetComponent<RectTransform>().anchorMax = max;
			t = Mathf.Min(t + Time.deltaTime, 3f);
			yield return null;
		}
		yield return WaitUntil(() => looted);
		Hide(lootArrow);
		Hide(sneakPrompt);
		yield return Poof(sneakTutGuards, false);
	}

	IEnumerator InventoryStep()
	{
		looted = false;
		inventoryButton.GetComponent<ScreenButton>().button.interactable = true;
		Show(openInventoryPrompt);
		yield return WaitUntil(() => usedInventory, () =>
		{
			openInventoryPrompt.SetActive(!openedInventory);
			useInventoryPrompt.SetActive(openedInventory);
		});
		shiftButton.GetComponent<ScreenButton>().button.interactable = false;
		Hide(useInventoryPrompt);
		Show(invLoot);
		Show(invTutGuards);
		yield return null;
		Hide(invTutDoor);
		yield return WaitUntil(() => looted);
	}

	IEnumerator RunStep()
	{
		shiftButton.GetComponent<ScreenButton>().button.interactable = true;
		Time.timeScale = 0;
		yield return Wait(1.5f);
		yield return Poof(runTutGuards, true);
		Show(runButton);
		Show(runPrompt);
		Show(runText);
		yield return WaitUntil(() => ran);
		Time.timeScale = 1;
		Hide(runPrompt);
	}


	void Start() { if (enableTutorial) { StartCoroutine(DelayCoroutine()); } }

	// This is here to wait for everything to load
	IEnumerator DelayCoroutine()
	{
		for (int i = 0; i < delayFrames; i++)
		{
			yield return null;
		}

		HideUnlearnedInterface();
		StartCoroutine(Tutorial());
	}

	void HideUnlearnedInterface()
	{
		shiftButton.SetActive(false);
		collectButton.SetActive(false);
		runButton.SetActive(false);
		inventoryButton.SetActive(false);
		moneyDisplay.SetActive(false);
		timerDisplay.SetActive(false);
	}

	IEnumerator WaitUntil(Func<bool> done, Action each = null)
	{
		while (!done())
		{
			each?.Invoke();
			yield return null;
		}
	}

	void Poof(GameObject target, bool visible)
	{
		Instantiate(poofParticlesPrefab, target.transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
		SFXManager.PlayClip(poofClip, 1, target.transform.position, UnityEngine.Random.Range(-0.05f, .1f));
		if (!visible)
		{
			target.GetComponent<GuardBehaviour>().ResetGuard();
			target.GetComponent<GuardHUD>().Update();
			Hide(target);
		}
		else
		{
			Show(target);
		}
	}

	IEnumerator Poof(GameObject[] targets, bool visible)
	{
		foreach (GameObject target in targets)
		{
			Poof(target, visible);
			yield return Wait(0.1f);
		}
	}

	float Ease(float x) => x < 0.5 ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2;

	WaitForSecondsRealtime Wait(float t) => new(skipDelays ? 0 : t);
	void Show(GameObject g) => g.SetActive(true);
	void Show(GameObject[] gs) { foreach (GameObject g in gs) { Show(g); } }
	void Hide(GameObject g) => g.SetActive(false);
}