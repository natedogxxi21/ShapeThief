using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using System.Collections;
using UnityEngine.Events;
using Unity.Scripting.LifecycleManagement;

public partial class PlayerShift : MonoBehaviour
{
	[AutoStaticsCleanup] public static PlayerShift Instance;

	[SerializeField] Rigidbody rb;
	ShiftProp possibleShiftTarget;
	ShiftProp shiftTarget;
	[SerializeField] Transform baseModel;
	[SerializeField] Transform shiftModelTransform;
	[SerializeField] MeshFilter shiftMeshFilter;
	[SerializeField] MeshRenderer shiftMeshRenderer;
	[SerializeField] MeshCollider shiftMeshCollider;
	[SerializeField] LayerMask shiftObjMask;

	public UnityEvent OnShift { get; private set; } = new();
	public bool ShiftAvailable => !shiftTransitioning &&
						  (shiftTarget != null || Shifted) &&
											ShiftCooldownTimer.Completed;
	public UnityEvent OnStore { get; private set; } = new();
	public bool StoreAvailable => shiftTarget != null && StoreCooldownTimer.Completed;

	[SerializeField] float shiftTransitionLength;
	[SerializeField] float shiftDuration;
	[SerializeField] float shiftCooldownDuration;
	[SerializeField] float storeCooldownDuration;

	public bool Shifted { get; private set; }
	bool shiftTransitioning;
	Vector3 targetShiftObjectScale;
	public Timer ShiftTimer { get; private set; } // duration player stays shifted
	public Timer ShiftCooldownTimer { get; private set; }
	public Timer StoreCooldownTimer { get; private set; }
	// these are used to detect when the timers above end
	bool shiftActive = false;
	bool shiftOnCooldown = false;
	bool storeOnCooldown = false;

	Finger castingFinger = null;

	void Awake()
	{
		Instance = this;
	}

	void Update()
	{
		if (shiftActive)
		{
			if (ShiftTimer.Completed)
			{
				Shift();
				shiftActive = false;
			}
		}
		if (shiftOnCooldown)
		{
			if (ShiftCooldownTimer.Completed)
			{
				OnShift.Invoke();
				shiftOnCooldown = false;
			}
		}
		
		if (storeOnCooldown)
		{
			if (StoreCooldownTimer.Completed)
			{
				OnStore.Invoke();
				storeOnCooldown = false;
			}
		}
	}

	public void Shift()
	{
		if (ShiftAvailable)
		{
			shiftTransitioning = true;
			OnShift.Invoke();
			if (!Shifted)
			{
				shiftMeshCollider.sharedMesh = shiftTarget.prop.mesh;
				shiftMeshFilter.mesh = shiftTarget.prop.mesh;
				shiftMeshRenderer.material = shiftTarget.prop.material;
				targetShiftObjectScale = shiftTarget.prop.scale;
			}
			StartCoroutine(ShiftCoroutine(!Shifted));
		}
	}

	public void Store()
	{
		if (StoreAvailable)
		{
			HUD.Instance.AddToInventory(shiftTarget.prop);
			StoreCooldownTimer = storeCooldownDuration;
			storeOnCooldown = true;
			OnStore.Invoke();
		}
	}

	public bool ShiftInventory(Prop prop)
	{
		if (shiftTransitioning || Shifted || !ShiftCooldownTimer.Completed)
		{ return false; }

		shiftTransitioning = true;
		OnShift.Invoke();
		shiftMeshCollider.sharedMesh = prop.mesh;
		shiftMeshFilter.mesh = prop.mesh;
		shiftMeshRenderer.material = prop.material;
		targetShiftObjectScale = prop.scale;
		HUD.Instance.AddToInventory(prop);
		StartCoroutine(ShiftCoroutine(!Shifted));

		return true;
	}

	IEnumerator ShiftCoroutine(bool toObject)
	{
		if (toObject) { shiftModelTransform.gameObject.SetActive(true); }
		else
		{
			ShiftTimer = -1;
			baseModel.gameObject.SetActive(true);
		}

		float p = 0;
		while (p <= 1)
		{
			p += Time.deltaTime / shiftTransitionLength;
			float pc = Mathf.Clamp01(p);
			float shiftProgress = Mathf.Clamp(toObject ? pc : 1 - pc, 0.05f, 1);
			float baseProgress = Mathf.Clamp(1 - shiftProgress, 0.05f, 1);
			shiftModelTransform.localScale = targetShiftObjectScale * shiftProgress;
			baseModel.localScale = new(baseProgress, baseProgress, baseProgress);
			transform.position = new(transform.position.x, 0, transform.position.z);
			rb.position = transform.position;
			yield return null;
		}

		shiftModelTransform.gameObject.SetActive(toObject);
		baseModel.gameObject.SetActive(!toObject);
		transform.position = new(transform.position.x, 0, transform.position.z);
		rb.position = transform.position;

		if (toObject)
		{
			ShiftTimer = shiftDuration;
			shiftActive = true;
		}
		else
		{
			ShiftCooldownTimer = shiftCooldownDuration;
			shiftOnCooldown = true;
		}

		shiftTransitioning = false;
		Shifted = toObject;

		OnShift.Invoke();
	}

	void HandleFingerDown(Finger finger)
	{
		if (HUD.Instance.IsScreenPosReserved(finger.currentTouch.screenPosition))
		{ return; }

		if (castingFinger == null)
		{
			castingFinger = finger;
			Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 50, shiftObjMask))
			{
				if (hit.transform.TryGetComponent(out ShiftProp shiftObject))
				{
					// Will become actual shift target if the same object is
					// found where the player lifts their finger off the screen
					possibleShiftTarget = shiftObject;
					return;
				}
			}
			// Will be reached unless there's a hit with a ShiftObject
			possibleShiftTarget = null;
			if (shiftTarget != null) { shiftTarget.Highlight(false); }
			shiftTarget = null;
			OnShift.Invoke();
			OnStore.Invoke();
		}
	}

	void HandleFingerUp(Finger finger)
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
						OnStore.Invoke();
						shiftTarget.Highlight(true);
					}
				}
			}
		}
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