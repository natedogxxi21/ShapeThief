using UnityEngine;
using UnityEngine.Events;

public class GuardHearing : MonoBehaviour
{
	public readonly UnityEvent<Vector3> onPlayerHeard = new();
	public readonly UnityEvent<Vector3> onPlayerNoticed = new();

	// Cues quieter than this are ignored.
	public const float AudibleCutoff = 0.05f;

	float soundHeard;
	Timer stopListeningTimer;
	[SerializeField] float investigateSoundThreshold;
	[SerializeField] float hearPlayerThreshold;
	[SerializeField] float suspicionDrain;
	public float Suspicion => soundHeard / hearPlayerThreshold;

#if UNITY_EDITOR
	// Editor-only: recent cues, used by the inspector graph to flash a dot on the curve.
	public struct CueRecord
	{
		public float distance;
		public float relativeLoudness;
		public float time; // Time.realtimeSinceStartup
	}

	public const float CueDebugLifetime = 1f;
	readonly System.Collections.Generic.List<CueRecord> recentCues = new();
	public System.Collections.Generic.IReadOnlyList<CueRecord> RecentCues => recentCues;

	void RecordCue(Vector3 pos, float relative)
	{
		float now = Time.realtimeSinceStartup;
		recentCues.RemoveAll(c => now - c.time > CueDebugLifetime);
		if (recentCues.Count >= 16) { recentCues.RemoveAt(0); }
		recentCues.Add(new CueRecord
		{
			distance = Vector3.Distance(pos, transform.position),
			relativeLoudness = relative,
			time = now
		});
	}
#endif

	[Header("Hearing Curve Parameters")]
	[SerializeField] float amplitude = 3;
	[SerializeField] float maxDistance = 10;
	[SerializeField][Range(0f, 1f)] float sustain = .3f;

	public void HearCue(float loudness, Vector3 pos)
	{
		float relative = RelativeLoudness(loudness, pos);
		if (relative < AudibleCutoff) { return; }

#if UNITY_EDITOR
		RecordCue(pos, relative);
#endif

		soundHeard += relative;
		stopListeningTimer = .5f;
		if (soundHeard > hearPlayerThreshold)
		{
			onPlayerHeard.Invoke(pos);
		}

		if (soundHeard > investigateSoundThreshold)
		{
			onPlayerNoticed.Invoke(pos);
		}
	}

	void FixedUpdate()
	{
		if (soundHeard > 0 && stopListeningTimer.Completed)
		{
			soundHeard = Mathf.Max(0, soundHeard - (suspicionDrain * Time.fixedDeltaTime));
		}
	}

	float RelativeLoudness(float sourceLoudness, Vector3 sourcePos)
	{
		float dist = Vector3.Distance(sourcePos, transform.position);
		return sourceLoudness * EvaluateCurve(dist);
	}

	// Falloff multiplier for a given distance
	public float EvaluateCurve(float distance)
	{
		return EvaluateCurve(amplitude, maxDistance, sustain, distance);
	}

	public static float EvaluateCurve(float amplitude, float maxDistance, float sustain, float distance)
	{
		float b = Mathf.Max(maxDistance, 0.0001f);
		float x = Mathf.Clamp(distance, 0f, b); // clamping to maxDistance keeps x/b <= 1, so no NaN
		float a = amplitude, p = sustain;
		// y = (a * (1 - (x/b)^p)) ^ (1 - p)
		return Mathf.Pow(a * (1 - Mathf.Pow(x / b, p)), 1 - p);
	}
}