using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using System.Collections;
using UnityEngine.Events;

public class PlayerShift : MonoBehaviour
{
	[SerializeField] Rigidbody rb;
	ShiftObject possibleShiftTarget;
	ShiftObject shiftTarget;
	[SerializeField] Transform baseModel;
	[SerializeField] Transform shiftModelTransform;
	[SerializeField] MeshFilter shiftMeshFilter;
	[SerializeField] MeshRenderer shiftMeshRenderer;
	[SerializeField] MeshCollider shiftMeshCollider;
	[SerializeField] LayerMask shiftObjMask;

	public UnityEvent ShiftEvent { get; private set; } = new();
	public bool ShiftAvailable => !shiftTransitioning &&
						  (shiftTarget != null || shifted) &&
											ShiftCooldown <= 0;

	[SerializeField] float shiftTransitionLength;
	[SerializeField] float shiftDuration;
	[SerializeField] float shiftCooldownDuration;

	bool shifted;
	bool shiftTransitioning;
	public float ShiftTimer { get; private set; }
	public float ShiftCooldown { get; private set; }
	public float ShiftCooldownPercent => ShiftCooldown / shiftCooldownDuration;

	Finger castingFinger = null;

	void Update()
	{
		if (ShiftTimer > 0)
		{
			ShiftTimer -= Time.deltaTime;
			if (ShiftTimer <= 0)
			{
				Shift();
			}
		}
		if (ShiftCooldown > 0)
		{
			ShiftCooldown -= Time.deltaTime;
			ShiftEvent.Invoke();
		}
	}

	public void Shift()
	{
		if (ShiftAvailable)
		{
			shiftTransitioning = true;
			ShiftEvent.Invoke();
			if (!shifted)
			{
				shiftMeshCollider.sharedMesh = shiftTarget.Mesh;
				shiftMeshFilter.mesh = shiftTarget.Mesh;
				shiftMeshRenderer.material = shiftTarget.Material;
			}
			StartCoroutine(ShiftCoroutine(!shifted));
		}
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
			float shiftProgress = toObject ? pc : 1 - pc;
			float baseProgress = 1 - shiftProgress;
			shiftModelTransform.localScale = new(shiftProgress, shiftProgress, shiftProgress);
			baseModel.localScale = new(baseProgress, baseProgress, baseProgress);
			transform.position = new(transform.position.x, 0, transform.position.z);
			rb.position = transform.position;
			yield return null;
		}

		shiftModelTransform.gameObject.SetActive(toObject);
		baseModel.gameObject.SetActive(!toObject);
		transform.position = new(transform.position.x, 0, transform.position.z);
		rb.position = transform.position;

		if (toObject) { ShiftTimer = shiftDuration; }
		else { ShiftCooldown = shiftCooldownDuration; }

		shiftTransitioning = false;
		shifted = toObject;

		ShiftEvent.Invoke();
	}

	void HandleFingerDown(Finger finger)
	{
		if (HUD.Instance.IsPosReserved(finger.currentTouch.screenPosition))
		{ return; }

		if (castingFinger == null)
		{
			castingFinger = finger;
			Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
			Debug.DrawLine(ray.origin, ray.origin + (ray.direction * 100), Color.red, 4);
			if (Physics.Raycast(ray, out RaycastHit hit, 100, shiftObjMask))
			{
				if (hit.transform.TryGetComponent(out ShiftObject shiftObject))
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
			ShiftEvent.Invoke();
		}
	}

	void HandleFingerUp(Finger finger)
	{
		if (finger == castingFinger)
		{
			castingFinger = null;
			Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 100, shiftObjMask))
			{
				if (hit.transform.TryGetComponent(out ShiftObject shiftObject))
				{
					if (shiftObject == possibleShiftTarget)
					{
						if (shiftTarget != null) { shiftTarget.Highlight(false); }
						shiftTarget = possibleShiftTarget;
						ShiftEvent.Invoke();
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