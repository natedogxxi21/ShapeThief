using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GuardVision : MonoBehaviour
{
	public readonly UnityEvent<GameObject> onPlayerSeen = new();
	public readonly UnityEvent<GameObject> onPlayerLost = new();
	public readonly UnityEvent<Vector3> onPlayerNoticed = new();
	public float Suspicion => playerSeen ? 1f : movementNoticed / noticeDistance;

	[Header("General")]
	[SerializeField] Transform eyes;
	[SerializeField] float viewDistance = 30f;
	[SerializeField] LayerMask visionBlockers = ~0;

	[Header("Movement detection (while shifted)")]
	[Tooltip("Meters of noticeable movement needed before the guard spots a shifted player.")]
	[SerializeField] float noticeDistance = 1.5f;
	[Tooltip("Movement slower than this is never noticed. Also how fast suspicion drains.")]
	[SerializeField] float safeSpeed = 0.5f;

	GameObject player;
	public List<Collider> touchedColliders = new();
	PlayerShift shift;
	Vector3 lastPlayerPos;
	float movementNoticed;
	bool playerSeen;

	float blindedUntil;
	bool IsBlinded => Time.time < blindedUntil;

	public void Blind(float duration)
	{
		blindedUntil = Mathf.Max(blindedUntil, Time.time + duration);

		if (player)
		{
			SetPlayerSeen(player, false);
			movementNoticed = 0f;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (player) return;
		touchedColliders.Add(other);
		if (!TryGetPlayer(out GameObject p, out PlayerShift s)) return;

		player = p;
		shift = s;
		lastPlayerPos = p.transform.position;
		movementNoticed = 0f;
	}

	void OnTriggerExit(Collider other)
	{
		if (!player) return;
		if (!TryGetPlayer(out GameObject p, out _) || p != player) return;
		if (touchedColliders.Contains(other)) { touchedColliders.Remove(other); }

		SetPlayerSeen(player, false);
		player = null;
		shift = null;
		movementNoticed = 0f;
	}

	void FixedUpdate()
	{
		if (!player) return;

		if (IsBlinded)
		{
			movementNoticed = 0f;
			SetPlayerSeen(player, false);
			lastPlayerPos = player.transform.position;
			return;
		}

		Vector3 pos = player.transform.position;
		float moved = Vector3.Distance(pos, lastPlayerPos);
		lastPlayerPos = pos;

		if (!HasLineOfSight(player))
		{
			movementNoticed = 0f;
			SetPlayerSeen(player, false);
			return;
		}

		if (!shift.Shifted)
		{
			movementNoticed = 0f;
			SetPlayerSeen(player, true);
			return;
		}

		// Shifted: only noticeable if moving faster than safeSpeed.
		float prevNotice = movementNoticed;
		movementNoticed += moved - safeSpeed * Time.fixedDeltaTime;
		movementNoticed = Mathf.Clamp(movementNoticed, 0f, noticeDistance);
		if (movementNoticed > prevNotice) { onPlayerNoticed.Invoke(player.transform.position); }

		// Hysteresis: spotted at full meter, lost only once it fully drains.
		if (movementNoticed >= noticeDistance) SetPlayerSeen(player, true);
		else if (movementNoticed <= 0f) SetPlayerSeen(player, false);
	}

	bool HasLineOfSight(GameObject target)
	{
		Vector3 toTarget = target.transform.position - eyes.position;

		if (!Physics.Raycast(eyes.position, toTarget.normalized, out RaycastHit hit,
				viewDistance, visionBlockers, QueryTriggerInteraction.Ignore))
			return false;

		return hit.rigidbody && hit.rigidbody.gameObject == target;
	}

	void SetPlayerSeen(GameObject target, bool seen)
	{
		if (seen == playerSeen) return;

		playerSeen = seen;
		(seen ? onPlayerSeen : onPlayerLost).Invoke(target);
	}

	bool TryGetPlayer(out GameObject player, out PlayerShift shift)
	{
		player = null;
		shift = null;

		if (touchedColliders.Count == 0) { return false; }

		foreach (Collider collider in touchedColliders)
		{
			if (collider)
			{
				Rigidbody rb = collider.attachedRigidbody;
				if (!rb || !rb.CompareTag("Player")) continue;

				player = rb.gameObject;
				if (player.TryGetComponent(out shift))
				{ return true; }
				else { continue; }
			}
		}

		return false;
	}
}