using UnityEngine;
using System.Collections;
using Unity.Scripting.LifecycleManagement;

public partial class SFXManager : MonoBehaviour
{
	[AutoStaticsCleanup] static SFXManager Instance;
	[SerializeField] GameObject SFXObject;
	
	void Awake()
	{
		Instance = this;
	}
	
	#region PlayClip()
	public static AudioSource PlayClip(AudioClip audioClip, float volume)
	{
		if (Instance == null) { return null; }

		AudioSource audioSource = Instantiate(Instance.SFXObject).GetComponent<AudioSource>();
		
		audioSource.clip = audioClip;
		audioSource.volume = volume;
		audioSource.spatialBlend = 0;
		audioSource.Play();
		
		Destroy(audioSource.gameObject, audioClip.length);
		
		return audioSource;
	}
	public static AudioSource PlayClip(AudioClip audioClip, float volume, float deltaPitch)
	{
		if (Instance == null) { return null; }

		AudioSource audioSource = Instantiate(Instance.SFXObject).GetComponent<AudioSource>();
		
		audioSource.clip = audioClip;
		audioSource.volume = volume;
		audioSource.spatialBlend = 0;
		audioSource.pitch += deltaPitch;
		audioSource.Play();
		
		Destroy(audioSource.gameObject, audioClip.length);
		
		return audioSource;
	}
	public static AudioSource PlayClip(AudioClip audioClip, float volume, Vector3 location)
	{
		if (Instance == null) { return null; }

		AudioSource audioSource = Instantiate(Instance.SFXObject).GetComponent<AudioSource>();

		audioSource.clip = audioClip;
		audioSource.volume = volume;
		audioSource.spatialBlend = 1;
		audioSource.transform.position = location;
		audioSource.Play();

		Destroy(audioSource.gameObject, audioClip.length);

		return audioSource;
	}
	public static AudioSource PlayClip(AudioClip audioClip, float volume, Vector3 location, float deltaPitch)
	{
		if (Instance == null) { return null; }

		AudioSource audioSource = Instantiate(Instance.SFXObject).GetComponent<AudioSource>();

		audioSource.clip = audioClip;
		audioSource.volume = volume;
		audioSource.spatialBlend = 1;
		audioSource.transform.position = location;
		audioSource.pitch += deltaPitch;
		audioSource.Play();

		Destroy(audioSource.gameObject, audioClip.length);

		return audioSource;
	}
	#endregion PlayClip()

	public static AudioSource PlayRandomClip(ref AudioClip[] audioClips, float volume, Vector3 location, float deltaPitch)
	{
		if (Instance == null) { return null; }
		
		AudioSource audioSource = Instantiate(Instance.SFXObject).GetComponent<AudioSource>();
		
		int clipIndex = Random.Range(0, audioClips.Length);
		
		audioSource.clip = audioClips[clipIndex];
		audioSource.volume = volume;
		audioSource.spatialBlend = 1;
		audioSource.transform.position = location;
		audioSource.pitch += deltaPitch;
		audioSource.Play();
		
		Destroy(audioSource.gameObject, audioClips[clipIndex].length);
		
		return audioSource;
	}

	public static AudioSource CreateAudioSource(AudioClip audioClip)
	{
		if (Instance == null) { return null; }

		AudioSource audioSource = Instantiate(Instance.SFXObject).GetComponent<AudioSource>();

		audioSource.clip = audioClip;

		return audioSource;
	}
	
	public static void FadeIn(AudioSource audioSource, float fadeDuration)
	{
		Instance.StartCoroutine(FadeInCoroutine());
		
		IEnumerator FadeInCoroutine()
		{
			float startVolume = audioSource.volume;
			float elapsedTime = 0f;
			
			while (elapsedTime < fadeDuration)
			{
				elapsedTime += Time.deltaTime;
				audioSource.volume = Mathf.Lerp(startVolume, 1f, elapsedTime / fadeDuration);
				yield return null;
			}

			audioSource.volume = 1f;
		}
	}

	public static void FadeOut(AudioSource audioSource, float fadeDuration, bool destroy)
	{
		Instance.StartCoroutine(FadeOutCoroutine());

		IEnumerator FadeOutCoroutine()
		{
			float startVolume = audioSource.volume;
			float elapsedTime = 0f;

			while (elapsedTime < fadeDuration)
			{
				if (audioSource == null) { print("exiting early"); yield break; }
				
				elapsedTime += Time.deltaTime;
				audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
				yield return null;
			}

			audioSource.volume = 0f; // Ensure volume is set to 0 at the end
			audioSource.Stop();      // Stop the audio source
			if (destroy) { Destroy(audioSource.gameObject); } // Optional: Destroy the object if needed
		}
	}
}