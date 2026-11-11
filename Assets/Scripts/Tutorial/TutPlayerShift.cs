using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public partial class TutPlayerShift : PlayerShift
{
	public override void Shift()
	{
		if (ShiftAvailable)
		{
			if (!Shifted)
			{
				TutorialFlow.shifted = true;
			}
			else
			{
				TutorialFlow.unshifted = true;
			}
		}
		base.Shift();
	}

	public override void Collect()
	{
		if (CollectAvailable)
		{
			HUD.Instance.AddToInventory(shiftTarget.prop);
			Instantiate(collectPoofParticlesPrefab, shiftTarget.transform.position, Quaternion.identity);
			SFXManager.PlayClip(collectPoofClip, 1, shiftTarget.transform.position, Random.Range(-0.05f, .1f));
			Destroy(shiftTarget.gameObject);
			shiftTarget.Highlight(false);
			HUD.Instance.AnimateCollect(shiftTarget);
			shiftTarget = null;
			CollectCooldownTimer = collectCooldownDuration;
			collectOnCooldown = true;
			OnCollect.Invoke();
			TutorialFlow.collected = true;
		}
	}

	protected override void HandleFingerUp(Finger finger)
	{
		if (finger == castingFinger)
		{
			castingFinger = null;
			Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 50, shiftObjMask))
			{
				if (hit.transform.TryGetComponent(out ShiftProp shiftObject))
				{
					if (shiftObject == possibleShiftTarget)
					{
						if (shiftTarget != null) { shiftTarget.Highlight(false); }
						shiftTarget = possibleShiftTarget;
						OnShift.Invoke();
						OnCollect.Invoke();
						shiftTarget.Highlight(true);
						TutorialFlow.selected = true;
					}
				}
			}

			if (possibleShiftTarget == null && deselecting && Vector2.Distance(deselectFingerPos, finger.currentTouch.screenPosition) < 10f)
			{
				if (shiftTarget != null) { shiftTarget.Highlight(false); }
				shiftTarget = null;
				TutorialFlow.selected = false;
				OnShift.Invoke();
				OnCollect.Invoke();
			}
		}
	}
}