using System.Collections;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

public partial class TutorialFlow : MonoBehaviour
{
	// states
	[AutoStaticsCleanup] public static bool looked;
	[AutoStaticsCleanup] public static bool moved;
	[AutoStaticsCleanup] public static bool looted;
	[AutoStaticsCleanup] public static bool selected;
	[AutoStaticsCleanup] public static bool shifted;
	[AutoStaticsCleanup] public static bool unshifted;
	[AutoStaticsCleanup] public static bool collected;
	[AutoStaticsCleanup] public static bool openedInventory;
	[AutoStaticsCleanup] public static bool closedInventory;

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
	lookPrompt, // point at right half, diagram of moving camera
	movePrompt, // point at left half of screen, diagram of moving plr
	lootArrow, moneyDisplay, lootReference, // have player collect gem then show money counter
	tapPropPrompt, shiftPropReference, // finger over the tap prop
	shiftPrompt, shiftButton, // finger over button, then tap again to unshift
	shiftTutDoor, shiftTutLoot, // room with conspicuous gards, reach loot
	shiftTutGuardA, shiftTutGuardB,
	collectPropReference, // tap the prop for the next step
	collectPrompt, collectButton, inventoryButton, // collect the prop and have it fly into inventory

	openInventoryPrompt, inventorySuggestion, // open inventory and show diagram on when to use
	closeInventoryPrompt, // close inventory
	sneakPrompt, // sneak past guards facing away
	runPrompt, runButton; // run past conspicuous guards to the exit pad

	IEnumerator Tutorial()
	{
		yield return Wait(1f);
		// look around
		Show(lookPrompt);
		while (!looked) { yield return null; }
		yield return Wait(0.5f);
		Hide(lookPrompt);
		yield return Wait(1.5f);
		// move around
		Show(movePrompt);
		while (!moved) { yield return null; }
		yield return Wait(0.5f);
		Hide(movePrompt);
		yield return Wait(0.5f);
		// collect money
		Show(moneyDisplay);
		Show(lootReference);
		Show(lootArrow);
		while (!looted) { yield return null; }
		Hide(lootArrow);
		yield return Wait(0.5f);
		// select prop
		Show(shiftPropReference);
		Show(tapPropPrompt);
		while (!selected) { yield return null; }
		Hide(tapPropPrompt);
		// shift into a prop
		Show(shiftButton);
		Show(shiftPrompt);
		while (!shifted) { yield return null; }
		yield return Wait(1f);
		Hide(shiftPrompt);
		// open shift tutorial room
		Show(shiftTutGuardA);
		Show(shiftTutGuardB);
		yield return null;
		Hide(shiftTutDoor);
		lootArrow.GetComponent<TrackObject>().target = shiftTutLoot.transform;
		Show(lootArrow);
		Show(shiftTutLoot);
		looted = false;
		while (!looted) { yield return null; }
		Hide(lootArrow);
		Poof(shiftTutGuardA);
		Poof(shiftTutGuardB);
		yield return Wait(1.0f);
		// unshift
		Show(shiftPrompt);
		while (!unshifted) { yield return null; }
		Hide(shiftPrompt);
		yield return Wait(0.5f);
		Hide(shiftPropReference);
		yield return Wait(1.0f);
		// collect a prop
		selected = false;
		tapPropPrompt.GetComponent<TrackObject>().target = collectPropReference.transform;
		Show(collectPropReference);
		Show(tapPropPrompt);
		while (!selected) { yield return null; }
		Hide(tapPropPrompt);
		Show(collectButton);
		Show(collectPrompt);
		while (!collected) { yield return null; }
		Hide(collectPrompt);
		Show(inventoryButton);
		yield return Wait(1.5f);
		// see inventory
		Show(openInventoryPrompt);
		while (!openedInventory) { yield return null; }
		Hide(openInventoryPrompt);
		Show(inventorySuggestion);
		yield return Wait(1.5f);
		Show(closeInventoryPrompt);
		while (!closedInventory) { yield return null; }
		Hide(inventorySuggestion);
		Hide(closeInventoryPrompt);

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

	void Poof(GameObject target)
	{
		Instantiate(poofParticlesPrefab, target.transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
		SFXManager.PlayClip(poofClip, 1, target.transform.position, Random.Range(-0.05f, .1f));
		target.GetComponent<GuardBehaviour>().ResetGuard();
		Hide(target);
	}

	WaitForSecondsRealtime Wait(float t) => new(skipDelays ? 0 : t);
	void Pause() => Time.timeScale = 0;
	void Resume() => Time.timeScale = 1;
	void Show(GameObject g) => g.SetActive(true);
	void Hide(GameObject g) => g.SetActive(false);
}