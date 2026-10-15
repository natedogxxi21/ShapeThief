using UnityEngine;

public class AudioCueManager : MonoBehaviour
{
	static AudioCueManager Instance;
	[SerializeField] AudioClip footstepClip;

	void Awake()
	{
		Instance = this;
	}

	public static void PlayFootstep(Vector3 position)
	{
		SFXManager.PlayClip(Instance.footstepClip, 1, 1);
		foreach (GuardBehaviour guard in GuardNavMesh.Instance.RegisteredGuards)
		{
			
		}
	}
}