using UnityEngine;
using Unity.Scripting.LifecycleManagement;

public partial class AudioCueManager : MonoBehaviour
{
	[AutoStaticsCleanup] static AudioCueManager Instance;
	[SerializeField] GameObject particleEffectPrefab;
	[SerializeField] AudioClip footstepClip;
	[SerializeField] AudioClip testClip;

	void Awake() => Instance = this;

	public static void PlayFootstep(Vector3 position, bool running)
	{
		float volume = running ? 1.5f : 1f;
		float deltaPitch = Random.Range(.5f, 1f);
		SFXManager.PlayClip(Instance.footstepClip, volume, position, deltaPitch);
		CueGuards(volume, position);
		MakeParticles(position, volume);
	}

	public static void PlayCollisionSound(Prop prop, Vector3 position, float velocity)
	{
		float volume = (2 * velocity) - 1f;
		float deltaPitch = Random.Range(-.1f, .1f);
		SFXManager.PlayRandomClip(ref prop.collisionSounds, volume, position, deltaPitch);
		CueGuards(volume, position);
		MakeParticles(position, volume);
	}

	public static void PlayTestSound(Vector3 position)
	{
		float deltaPitch = Random.Range(-.1f, .1f);
		SFXManager.PlayClip(Instance.testClip, 1, position, deltaPitch);
		CueGuards(loudness: 3, position);
		MakeParticles(position, 3);
	}

	static void MakeParticles(Vector3 position, float volume)
	{
		float speed = 3.25f + (volume / 3);
		var particleMain = Instantiate(Instance.particleEffectPrefab, position, Quaternion.identity).GetComponent<ParticleSystem>().main;
		particleMain.startSpeed = speed;
	}

	static void CueGuards(float loudness, Vector3 position)
	{
		foreach (GuardBehaviour guard in GuardNavMesh.Instance.RegisteredGuards)
		{
			guard.hearing.HearCue(loudness, position);	
		}
	}
}