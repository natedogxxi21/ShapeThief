using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public partial class PlayerTouchMovement : MonoBehaviour
{
	[AutoStaticsCleanup] public static PlayerTouchMovement example;
	[SerializeField] FloatingJoystick joystick;

	[SerializeField] Rigidbody rb;

	Finger movementFinger;
	Vector2 movementAmount;

	void Awake()
	{
		example = this;
	}

	void Update()
	{
		
	}

	void OnEnable()
	{
		EnhancedTouchSupport.Enable();
		ETouch.Touch.onFingerDown += HandleFingerDown;
		ETouch.Touch.onFingerUp += HandleFingerUp;
		ETouch.Touch.onFingerMove += HandleFingerMove;
	}

	void OnDisable()
	{
		ETouch.Touch.onFingerDown -= HandleFingerDown;
		ETouch.Touch.onFingerUp -= HandleFingerUp;
		ETouch.Touch.onFingerMove -= HandleFingerMove;
		EnhancedTouchSupport.Disable();
	}

	private void HandleFingerDown(Finger touchedFinger)
	{
		if (movementFinger == null && touchedFinger.screenPosition.x <= Screen.width / 2f)
		{
			movementFinger = touchedFinger;
			movementAmount = Vector2.zero;
			joystick.gameObject.SetActive(true);
			joystick.rectTransform.anchoredPosition = ClampStartPosition(touchedFinger.screenPosition);
		}
	}

	private void HandleFingerMove(Finger movedFinger)
	{
		if (movedFinger == movementFinger)
		{
			Vector2 knobPosition;
			float maxMovement = joystick.size.x / 2f;
			ETouch.Touch currentTouch = movedFinger.currentTouch;
			float distance = Vector2.Distance(currentTouch.screenPosition, joystick.rectTransform.anchoredPosition);

			if (distance > maxMovement)
			{
				knobPosition = (currentTouch.screenPosition - joystick.rectTransform.anchoredPosition).normalized * maxMovement;
			}
			else
			{
				knobPosition = currentTouch.screenPosition - joystick.rectTransform.anchoredPosition;
			}

			joystick.knob.anchoredPosition = knobPosition;
			movementAmount = knobPosition / maxMovement;
		}
	}

	private void HandleFingerUp(Finger raisedFinger)
	{
		if (raisedFinger == movementFinger)
		{
			movementFinger = null;
			joystick.knob.anchoredPosition = Vector2.zero;
			joystick.gameObject.SetActive(false);
			movementAmount = Vector2.zero;
		}
	}

	Vector2 ClampStartPosition(Vector2 startPosition)
	{
		if (startPosition.x < joystick.size.x / 2)
		{
			startPosition.x = joystick.size.x / 2;
		}

		if (startPosition.y < joystick.size.y / 2)
		{
			startPosition.y = joystick.size.y / 2;
		}
		else if (startPosition.y > Screen.height - joystick.size.y / 2)
		{
			startPosition.y = Screen.height - joystick.size.y / 2;
		}

		return startPosition;
	}
}