using System;
using UnityEngine;

[Serializable]
public struct Timer
{
	[field: SerializeField] public float EndTime { get; private set; }
	[field: SerializeField] public float Duration { get; private set; }
	[field: SerializeField] public bool Paused { get; private set; }
	[SerializeField] float pausedTime;
	readonly float EvalTime => Paused ? pausedTime : Time.time;
	public readonly float Remaining => EndTime - EvalTime;
	public readonly float Elapsed => Duration - Remaining;
	public readonly float PercentLeft => Duration > 0f ? Mathf.Clamp01(Remaining / Duration) : 1f;
	public readonly bool Completed => EvalTime >= EndTime;

	public Timer(float length)
	{
		Paused = false;
		EndTime = Time.time + length;
		pausedTime = EndTime;
		Duration = length;
	}

	public void Set(float length)
	{
		EndTime = EvalTime + length;
		Duration = length;
	}

	public void Reset() => EndTime = EvalTime + Duration;
	public void Cycle() => EndTime += Duration;

	public void Pause()
	{
		if (Paused) return;
		Paused = true;
		pausedTime = Time.time;
	}

	public void Resume()
	{
		if (!Paused) return;
		Paused = false;
		EndTime = Time.time + (EndTime - pausedTime);
	}

	public static implicit operator Timer(float duration) => new(duration);
}