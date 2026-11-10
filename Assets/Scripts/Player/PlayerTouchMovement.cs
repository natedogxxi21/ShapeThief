using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using Unity.Scripting.LifecycleManagement;

public partial class PlayerTouchMovement : MonoBehaviour
{
	[AutoStaticsCleanup] public static PlayerTouchMovement Instance;

	[SerializeField] Rigidbody rb;
	[SerializeField] CameraController cam;
	[SerializeField] Transform model;
	[SerializeField] Transform camYaw;
	[SerializeField] Transform camPitch;
	[SerializeField] float speed;
	[SerializeField] float runSpeed;
	[SerializeField] float maxStamina;
	[SerializeField] float staminaRecover;
	[SerializeField] float runStaminaDrain;
	[SerializeField] float footstepDistance = 1f;

	[Header("Debug Keyboard & Mouse Controls")]
	[SerializeField] InputAction kbmMove;
	[SerializeField] InputAction kbmTurn;
	bool kbmTurning = false;
	[SerializeField] InputAction kbmLook;
	[SerializeField] InputAction kbmTest;

	protected Finger movementFinger;
	Vector2 moveInput;
	protected Finger lookFinger;

	float charModelTargetAngle = 0;
	const float charModelTurnSpeed = 10;
	public bool Running { get; private set; }
	float stamina;
	public float StaminaPercent => stamina / maxStamina;
	float footstepTicker;

	void Awake()
	{
		Instance = this;
		Application.targetFrameRate = 120;
		stamina = maxStamina;
	}

	void Update()
	{
		float angle = Mathf.LerpAngle(model.localEulerAngles.y, charModelTargetAngle, charModelTurnSpeed * Time.deltaTime);
		model.localEulerAngles = new Vector3(0, angle, 0);

		if (footstepTicker >= footstepDistance)
		{
			AudioCueManager.PlayFootstep(transform.position, Running, rb.linearVelocity.magnitude / (Running ? runSpeed : speed));
			footstepTicker = 0;
		}
	}

	void FixedUpdate()
	{
		footstepTicker += rb.linearVelocity.magnitude * Time.fixedDeltaTime;
		rb.linearVelocity = (Running ? runSpeed : speed) * ((moveInput.x * camYaw.right) + (moveInput.y * camYaw.forward));

		if (Running && stamina > 0 && moveInput.sqrMagnitude > 0.01f)
		{
			if ((stamina -= runStaminaDrain * Time.fixedDeltaTime) <= 0)
			{
				ToggleRun();
				stamina = 0;
			}
		}
		else if (!Running || moveInput.sqrMagnitude < 0.01f)
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

	protected virtual void HandleFingerMove(Finger movedFinger)
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
			charModelTargetAngle = (Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg) + cam.yaw;
		}

		if (movedFinger == lookFinger)
		{
			cam.ProcessLookInput(Touchscreen.current.touches[movedFinger.index].delta.value);
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
		{ charModelTargetAngle = (Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg) + cam.yaw; }
	}
	void OnKBMTurn(InputAction.CallbackContext ctx) => kbmTurning = ctx.ReadValue<float>() > 0.5f;
	void OnKBMLook(InputAction.CallbackContext ctx)
	{
		if (kbmTurning)
		{
			const float addKBMSensitivity = 5;
			cam.ProcessLookInput(ctx.ReadValue<Vector2>() * addKBMSensitivity);

			if (moveInput.sqrMagnitude > 0.05f)
			{ charModelTargetAngle = (Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg) + cam.yaw; }
		}
	}
	void OnKBMTest(InputAction.CallbackContext ctx) => AudioCueManager.PlayTestSound(transform.position);

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

	public void UpgradeStamina(float amount)
	{
		Debug.Log($"BEFORE upgrade: Current={stamina}, Max={maxStamina}, Amount={amount}");
		maxStamina += amount;
		stamina += amount;
		Debug.Log($"AFTER upgrade: Current={stamina}, Max={maxStamina}");
	}
}