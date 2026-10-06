using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using Unity.Scripting.LifecycleManagement;

public partial class PlayerTouchMovement : MonoBehaviour
{
	[AutoStaticsCleanup] public static PlayerTouchMovement Instance;

	[SerializeField] Rigidbody rb;
	[SerializeField] Transform model;
	[SerializeField] Transform camYaw;
	[SerializeField] Transform camPitch;
	[SerializeField] float speed;
	[SerializeField] float runSpeed;
	[SerializeField] float lookSensitivity = 1;
	[SerializeField] float maxStamina;
	[SerializeField] float staminaRecover;
	[SerializeField] float runStaminaDrain;

	[Header("Debug Keyboard & Mouse Controls")]
	[SerializeField] InputAction kbmMove;
	[SerializeField] InputAction kbmTurn;
	bool kbmTurning = false;
	[SerializeField] InputAction kbmLook;
	[SerializeField] InputAction kbmTest;

	Finger movementFinger;
	Vector2 moveInput;

	Finger lookFinger;
	Vector2 lookInput;
	float yaw = 0;
	float pitch = 45;
	float targetAngle = 0;
	const float turnSpeed = 10;
	public bool Running { get; private set; }
	float stamina;
	public float StaminaPercent => stamina / maxStamina;

	void Awake()
	{
		Instance = this;
		PlayerStats.ResetMoney();
		Application.targetFrameRate = 120;
		stamina = maxStamina;
	}

	void Update()
	{
		float angle = Mathf.LerpAngle(model.localEulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
		model.localEulerAngles = new Vector3(0, angle, 0);
	}

	void FixedUpdate()
	{
		rb.linearVelocity = (Running ? runSpeed : speed) * ((moveInput.x * camYaw.right) + (moveInput.y * camYaw.forward));

		if (Running && stamina > 0)
		{
			if ((stamina -= runStaminaDrain * Time.fixedDeltaTime) <= 0)
			{
				ToggleRun();
				stamina = 0;
			}
		}
		else if (!Running)
		{
			stamina = Mathf.Min(stamina + (staminaRecover * Time.fixedDeltaTime), maxStamina);
		}
	}

	private void HandleFingerDown(Finger touchedFinger)
	{
		if (HUD.Instance.IsScreenPosReserved(touchedFinger.currentTouch.screenPosition))
		{ return; }

		// Left half of screen
		if (touchedFinger.screenPosition.x <= Screen.width / 2f)
		{
			if (movementFinger == null)
			{
				movementFinger = touchedFinger;
				moveInput = Vector2.zero;
				HUD.Instance.moveJoystick.gameObject.SetActive(true);
				HUD.Instance.moveJoystick.rectTransform.anchoredPosition = ClampStartPosition(touchedFinger.screenPosition);
			}
		}
		// Right half
		else
		{
			// if lookedFinger is null, assign touchedFinger
			lookFinger ??= touchedFinger;
		}
	}

	private void HandleFingerMove(Finger movedFinger)
	{
		if (movedFinger == movementFinger)
		{
			Vector2 knobPosition;
			float maxMovement = HUD.Instance.moveJoystick.size.x / 2f;
			ETouch.Touch currentTouch = movedFinger.currentTouch;
			float distance = Vector2.Distance(currentTouch.screenPosition, HUD.Instance.moveJoystick.rectTransform.anchoredPosition);

			if (distance > maxMovement)
			{
				knobPosition = (currentTouch.screenPosition - HUD.Instance.moveJoystick.rectTransform.anchoredPosition).normalized * maxMovement;
			}
			else
			{
				knobPosition = currentTouch.screenPosition - HUD.Instance.moveJoystick.rectTransform.anchoredPosition;
			}

			HUD.Instance.moveJoystick.knob.anchoredPosition = knobPosition;
			moveInput = knobPosition / maxMovement;

			// Make model face moving direction
			targetAngle = (Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg) + yaw;
		}

		if (movedFinger == lookFinger)
		{
			lookInput = Touchscreen.current.touches[movedFinger.index].delta.value;

			yaw += lookInput.x * lookSensitivity;
			pitch = Mathf.Clamp(pitch - (lookInput.y * lookSensitivity), 10, 80);

			camYaw.localEulerAngles = new Vector3(0, yaw, 0);
			camPitch.localEulerAngles = new Vector3(pitch, 0, 0);
		}
	}

	private void HandleFingerUp(Finger raisedFinger)
	{
		if (raisedFinger == movementFinger)
		{
			movementFinger = null;
			HUD.Instance.moveJoystick.knob.anchoredPosition = Vector2.zero;
			HUD.Instance.moveJoystick.gameObject.SetActive(false);
			moveInput = Vector2.zero;
		}

		if (raisedFinger == lookFinger)
		{
			lookFinger = null;
			lookInput = Vector2.zero;
		}
	}

	Vector2 ClampStartPosition(Vector2 startPosition)
	{
		if (startPosition.x < HUD.Instance.moveJoystick.size.x / 2)
		{
			startPosition.x = HUD.Instance.moveJoystick.size.x / 2;
		}

		if (startPosition.y < HUD.Instance.moveJoystick.size.y / 2)
		{
			startPosition.y = HUD.Instance.moveJoystick.size.y / 2;
		}
		else if (startPosition.y > Screen.height - HUD.Instance.moveJoystick.size.y / 2)
		{
			startPosition.y = Screen.height - HUD.Instance.moveJoystick.size.y / 2;
		}

		return startPosition;
	}

	public bool ToggleRun()
	{
		Running = !Running;
		return Running;
	}

	void OnKBMMove(InputAction.CallbackContext ctx)
	{
		moveInput = ctx.ReadValue<Vector2>();
		if (moveInput.sqrMagnitude > 0.05f)
		{ targetAngle = (Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg) + yaw; }
	}
	void OnKBMTurn(InputAction.CallbackContext ctx) => kbmTurning = ctx.ReadValue<float>() > 0.5f;
	void OnKBMLook(InputAction.CallbackContext ctx)
	{
		if (kbmTurning)
		{
			lookInput = ctx.ReadValue<Vector2>();

			const float addKBMSensitivity = 5;
			yaw += lookInput.x * lookSensitivity * addKBMSensitivity;
			pitch = Mathf.Clamp(pitch - (lookInput.y * lookSensitivity * addKBMSensitivity), 10, 80);

			camYaw.localEulerAngles = new Vector3(0, yaw, 0);
			camPitch.localEulerAngles = new Vector3(pitch, 0, 0);
			if (moveInput.sqrMagnitude > 0.05f)
			{ targetAngle = (Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg) + yaw; }
		}
	}
	void OnKBMTest(InputAction.CallbackContext ctx) => PlayerStats.ChangeMoney(100);

	void OnEnable()
	{
		EnhancedTouchSupport.Enable();
		ETouch.Touch.onFingerDown += HandleFingerDown;
		ETouch.Touch.onFingerUp += HandleFingerUp;
		ETouch.Touch.onFingerMove += HandleFingerMove;
		#if UNITY_EDITOR
		kbmMove.performed += OnKBMMove;
		kbmTurn.performed += OnKBMTurn;
		kbmLook.performed += OnKBMLook;
		kbmTest.performed += OnKBMTest;
		kbmMove.Enable();
		kbmTurn.Enable();
		kbmLook.Enable();
		kbmTest.Enable();
#endif
	}

	void OnDisable()
	{
		ETouch.Touch.onFingerDown -= HandleFingerDown;
		ETouch.Touch.onFingerUp -= HandleFingerUp;
		ETouch.Touch.onFingerMove -= HandleFingerMove;
		#if UNITY_EDITOR
		kbmMove.performed -= OnKBMMove;
		kbmTurn.performed -= OnKBMTurn;
		kbmLook.performed -= OnKBMLook;
		kbmTest.performed -= OnKBMTest;
		kbmMove.Disable();
		kbmTurn.Disable();
		kbmLook.Disable();
		kbmTest.Disable();
#endif
		EnhancedTouchSupport.Disable();
	}
}