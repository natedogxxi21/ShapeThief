using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(GuardController))]
public class GuardBehaviour : MonoBehaviour
{
	public enum GuardState
	{
		Idle, // Nothing
		Post, // Standing and turning head
		Wander, // Randomly navigate around
		Investigate, // Walk towards a point
		Chase // Run towards a point
	}

	static readonly float[] stateTickDurations = { 10f, 4f, .2f, .2f, .2f };

	GuardController controller;
	GuardState state = GuardState.Idle;
	Timer stateTickTimer;

	Vector3 postPoint;
	float postDirection;
	bool postFacingLeft;

	[SerializeField] Timer wanderTimer;

	Vector3 investigatePoint;

	void Awake()
	{
		controller = GetComponent<GuardController>();
		Wander();
	}

	void Update()
	{
		if (stateTickTimer.Completed)
		{
			TickState();
		}
	}

	void TickState()
	{
		stateTickTimer = stateTickDurations[(int)state];
		switch (state)
		{

			case GuardState.Post:
				postFacingLeft = !postFacingLeft;
				break;

			case GuardState.Wander:
				if (controller.traveling) { wanderTimer.Pause(); }
				else { wanderTimer.Resume(); }

				if (wanderTimer.Completed)
				{
					wanderTimer.Reset();
					controller.WalkTo(GetWanderPoint());
				}
				break;

			case GuardState.Investigate:
				controller.WalkTo(investigatePoint);
				break;

			case GuardState.Chase:
				controller.RunTo(investigatePoint);
				break;
		}
	}

	public void Idle() => SetState(GuardState.Idle);
	public void Post(Vector3 position, Vector3 direction)
	{
		postPoint = position;
		postDirection = DirectionToAngle(direction);
		SetState(GuardState.Post);
	}
	public void Wander()
	{
		SetState(GuardState.Wander);
		wanderTimer = 3f;
	}

	void SetState(GuardState newState)
	{
		state = newState;
		stateTickTimer = stateTickDurations[(int)newState];
	}

	Vector3 GetWanderPoint()
	{
		float angle = Random.Range(0, 2 * Mathf.PI);
		float distance = Random.Range(2f, 6f);
		return transform.position + (new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * distance);
	}

	float DirectionToAngle(Vector3 direction) => Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
}

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