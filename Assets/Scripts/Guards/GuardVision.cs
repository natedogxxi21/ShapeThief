using UnityEngine;
using UnityEngine.Events;

public class GuardVision : MonoBehaviour
{
	public readonly UnityEvent<GameObject> onPlayerSeen = new();
	public readonly UnityEvent<GameObject> onPlayerLost = new();

	[SerializeField] Transform eyes;
	[SerializeField] float viewDistance = 30f;
	[SerializeField] LayerMask visionBlockers = ~0;

	bool playerSeen;

	void OnTriggerStay(Collider other)
	{
		if (!TryGetPlayer(other, out GameObject player, out PlayerShift shift)) return;

		SetPlayerSeen(player, CanSee(player, shift));
	}

	void OnTriggerExit(Collider other)
	{
		if (!TryGetPlayer(other, out GameObject player, out _)) return;

		SetPlayerSeen(player, false);
	}

	bool CanSee(GameObject player, PlayerShift shift)
	{
		if (shift.shifted) return false;

		Vector3 toPlayer = player.transform.position - eyes.position;

		if (!Physics.Raycast(eyes.position, toPlayer.normalized, out RaycastHit hit,
				viewDistance, visionBlockers, QueryTriggerInteraction.Ignore))
			return false;

		return hit.rigidbody && hit.rigidbody.gameObject == player;
	}

	void SetPlayerSeen(GameObject player, bool seen)
	{
		if (seen == playerSeen) return;

		playerSeen = seen;
		(seen ? onPlayerSeen : onPlayerLost).Invoke(player);
	}

	static bool TryGetPlayer(Collider other, out GameObject player, out PlayerShift shift)
	{
		player = null;
		shift = null;

		Rigidbody rb = other.attachedRigidbody;
		if (!rb || !rb.CompareTag("Player")) return false;

		player = rb.gameObject;
		return player.TryGetComponent(out shift);
	}
}