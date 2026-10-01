using UnityEngine;
using UnityEngine.Events;

public class GuardVision : MonoBehaviour
{
	public readonly UnityEvent<GameObject> onPlayerSeen = new();
	public readonly UnityEvent<GameObject> onPlayerLost = new();

	void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody 
		&& other.attachedRigidbody.gameObject.CompareTag("Player"))
		{
			GameObject playerGO = other.attachedRigidbody.gameObject;
			if (playerGO.TryGetComponent(out PlayerShift playerShift))
			{
				if (!playerShift.shifted)
				{
					onPlayerSeen.Invoke(playerGO);
				}
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.attachedRigidbody
		&& other.attachedRigidbody.gameObject.CompareTag("Player"))
		{
			GameObject playerGO = other.attachedRigidbody.gameObject;
			if (playerGO.TryGetComponent(out PlayerShift playerShift))
			{
				if (!playerShift.shifted)
				{
					onPlayerLost.Invoke(playerGO);
				}
			}
		}
	}
}