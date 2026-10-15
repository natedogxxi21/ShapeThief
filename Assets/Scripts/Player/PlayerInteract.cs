using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlayerInteract : MonoBehaviour
{
	Finger castingFinger = null;
	SceneKeypad possibleKeypad = null;

	void HandleFingerDown(Finger finger)
	{
		if (HUD.Instance.IsScreenPosReserved(finger.currentTouch.screenPosition))
		{ return; }

		if (castingFinger == null)
		{
			castingFinger = finger;
			Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 50))
			{
				if (hit.transform.TryGetComponent(out SceneKeypad keypad))
				{
					possibleKeypad = keypad;
					return;
				}
			}

			possibleKeypad = null;
		}
	}

	void HandleFingerUp(Finger finger)
	{
		if (finger == castingFinger)
		{
			castingFinger = null;
			Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 50))
			{
				if (hit.transform.TryGetComponent(out SceneKeypad keypad))
				{
					if (keypad == possibleKeypad)
					{
						keypad.Open();
						possibleKeypad = null;
					}
				}
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		Rigidbody rb = other.attachedRigidbody;
		if (rb && rb.TryGetComponent(out ShiftProp prop))
		{ prop.HighlightDashed(true); }
	}

	void OnTriggerExit(Collider other)
	{
		Rigidbody rb = other.attachedRigidbody;
		if (rb && rb.TryGetComponent(out ShiftProp prop))
		{ prop.HighlightDashed(false); }
	}

	void OnEnable()
	{
		EnhancedTouchSupport.Enable();
		ETouch.Touch.onFingerDown += HandleFingerDown;
		ETouch.Touch.onFingerUp += HandleFingerUp;
	}

	void OnDisable()
	{
		ETouch.Touch.onFingerDown -= HandleFingerDown;
		ETouch.Touch.onFingerUp -= HandleFingerUp;
		EnhancedTouchSupport.Disable();
	}
}