using UnityEngine;
using UnityEngine.Events;

public class GuardVision : MonoBehaviour
{
	public readonly UnityEvent<GameObject> onPlayerSeen = new();
	public readonly UnityEvent<GameObject> onPlayerLost = new();

	void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody && other.attachedRigidbody.gameObject.CompareTag("Player"))
		{
			onPlayerSeen.Invoke(other.attachedRigidbody.gameObject);
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.attachedRigidbody && other.attachedRigidbody.gameObject.CompareTag("Player"))
		{
			onPlayerLost.Invoke(other.attachedRigidbody.gameObject);
		}
	}
}