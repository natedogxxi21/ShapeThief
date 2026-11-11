using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GuardState
{
	Idle, // Nothing
	Post, // Standing and turning head
	Wander, // Randomly navigate around
	Patrol, // Walk back and forth between two points
	Investigate, // Walk towards a point
	Chase, // Run towards a point
	Return // Walking back to spawn point
}

[RequireComponent(typeof(GuardController))]
public class GuardBehaviour : MonoBehaviour
{
	static readonly float[] stateTickDurations = { 10f, 4f, .2f, .5f, .2f, .2f, 1f };

	GuardController controller;
	[SerializeField] GuardVision vision;
	public GuardHearing hearing;
	float TotalSuspicion => vision.Suspicion + hearing.Suspicion;
	public GuardState State { get; private set; } = GuardState.Idle;
	[SerializeField] GuardState defaultState;
	Timer stateTickTimer;
	Vector3 spawnPoint;

	Vector3 postPoint;
	float postDirection;
	bool postFacingLeft;

	[Space]
	[SerializeField] Timer wanderTimer;

	[SerializeField] Timer patrolTimer;
	[SerializeField] Vector3 patrolPoint;
	bool patrolToStart = false;

	GameObject seenPlayer = null;
	Vector3 investigatePoint;

	bool chaseInvestigating = false;
	public Timer chaseInvestigateTimer;

	void Start() => StartCoroutine(WaitToLoadCoroutine());

	IEnumerator WaitToLoadCoroutine()
	{
		while (GuardNavMesh.Instance == null || !GuardNavMesh.Instance.Initialized)
		{
			yield return null;
		}

		GuardNavMesh.Instance.RegisterGuard(this);

		controller = GetComponent<GuardController>();
		controller.enabled = true;
		switch (defaultState)
		{
			case GuardState.Idle: Idle(); break;
			case GuardState.Wander: Wander(); break;
			case GuardState.Post: Post(); break;
			case GuardState.Patrol: Patrol(); break;
			default:
				Debug.LogWarning("Can only use Idle, Wander, Post, and Patrol as default guard states");
				Idle(); break;
		}
		spawnPoint = transform.position;
		vision.onPlayerSeen.AddListener(OnPlayerSeen);
		vision.onPlayerLost.AddListener(OnPlayerLost);
		vision.onPlayerNoticed.AddListener(OnPlayerNoticed);
		hearing.onPlayerHeard.AddListener(OnPlayerHeard);
		hearing.onPlayerNoticed.AddListener(OnPlayerNoticed);
	}

	void Update()
	{
		if (TotalSuspicion >= 1)
		{ Chase(investigatePoint); }

		if (stateTickTimer.Completed)
		{ TickState(); }
	}

	void TickState()
	{
		stateTickTimer = stateTickDurations[(int)State];
		switch (State)
		{
			case GuardState.Post:
				controller.WalkTo(postPoint);
				postFacingLeft = !postFacingLeft;
				if (!controller.Traveling)
				{ controller.FaceToward(postDirection + (postFacingLeft ? -30 : 30)); }
				break;

			case GuardState.Wander:
				if (controller.Traveling) { wanderTimer.Pause(); }
				else { wanderTimer.Resume(); }

				if (wanderTimer.Completed)
				{
					wanderTimer.Reset();
					controller.WalkTo(GetWanderPoint());
				}
				break;

			case GuardState.Patrol:
				if (controller.Traveling) { patrolTimer.Pause(); }
				else { patrolTimer.Resume(); }

				if (patrolTimer.Completed)
				{
					patrolTimer.Reset();
					patrolToStart = !patrolToStart;
				}

				controller.WalkTo(patrolToStart ? spawnPoint : patrolPoint);
				break;

			case GuardState.Investigate:
				controller.WalkTo(investigatePoint);
				if (TotalSuspicion <= 0)
				{
					Return();
				}
				break;

			case GuardState.Chase:
				if (controller.Traveling) { chaseInvestigateTimer.Pause(); }

				if (seenPlayer != null)
				{
					investigatePoint = seenPlayer.transform.position;
				}

				controller.RunTo(investigatePoint);

				if (!controller.Traveling) // reached the player or last seen location
				{
					if (chaseInvestigating)
					{
						chaseInvestigateTimer.Resume();
						if (chaseInvestigateTimer.Completed)
						{
							Return();
						}
					}
					else // not yet investigating
					{
						chaseInvestigateTimer = 3;
						chaseInvestigating = true;
					}
				}
				break;

			case GuardState.Return:
				controller.WalkTo(spawnPoint);
				if (controller.RemainingDistance < 0.3f)
				{
					switch (defaultState)
					{
						case GuardState.Idle:
							Idle();
							break;

						case GuardState.Post:
							Post(postPoint, postDirection);
							break;

						case GuardState.Wander:
							Wander();
							break;

						case GuardState.Patrol:
							Patrol();
							break;

						default:
							throw new Exception("Cannot accept Guard State " + defaultState + " as default state");
					}
				}
				break;
		}
	}

	public void Idle() => SetState(GuardState.Idle);
	public void Post()
	{
		postPoint = transform.position;
		postDirection = DirectionToAngle(transform.forward);
		SetState(GuardState.Post);
	}
	public void Post(Vector3 position, float angle)
	{
		postPoint = position;
		postDirection = angle;
		SetState(GuardState.Post);
	}
	public void Wander()
	{
		SetState(GuardState.Wander);
		wanderTimer = 3f;
	}
	public void Patrol()
	{
		SetState(GuardState.Patrol);
		patrolTimer = 2f;
	}
	public void Investigate(Vector3 point)
	{
		investigatePoint = point;
		SetState(GuardState.Investigate);
	}
	public void Chase(GameObject player)
	{
		seenPlayer = player;
		chaseInvestigating = false;
		chaseInvestigateTimer = 3;
		investigatePoint = seenPlayer.transform.position;
		SetState(GuardState.Chase);
	}
	public void Chase(Vector3 position)
	{
		chaseInvestigating = false;
		chaseInvestigateTimer = 3;
		investigatePoint = position;
		SetState(GuardState.Chase);
	}
	public void Return()
	{
		seenPlayer = null;
		SetState(GuardState.Return);
	}

	void SetState(GuardState newState)
	{
		State = newState;
		stateTickTimer = stateTickDurations[(int)newState];
		controller.StoppingDistance = newState == GuardState.Investigate ? 5f : 0f;
		TickState();
	}

	void OnPlayerSeen(GameObject player) => Chase(player);
	void OnPlayerLost(GameObject player) => seenPlayer = null;
	void OnPlayerNoticed(Vector3 position)
	{
		if (State != GuardState.Chase)
		{
			Investigate(position);
		}
	}

	void OnPlayerHeard(Vector3 pos) => Chase(pos);

	Vector3 GetWanderPoint()
	{
		float angle = Random.Range(0, 2 * Mathf.PI);
		float distance = Random.Range(2f, 6f);
		return transform.position + (new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * distance);
	}

	public void ResetGuard()
	{
		controller.Warp(spawnPoint);
		vision.LosePlayer();
		hearing.LosePlayer();
		switch (defaultState)
		{
			case GuardState.Idle: Idle(); break;
			case GuardState.Wander: Wander(); break;
			case GuardState.Post: Post(); break;
			case GuardState.Patrol: Patrol(); break;
		}
	}

	float DirectionToAngle(Vector3 direction) => Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (defaultState != GuardState.Patrol) return;

		// In edit mode spawnPoint isn't set yet, so use the current position
		Vector3 start = Application.isPlaying ? spawnPoint : transform.position;

		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(patrolPoint, 0.3f);
		Gizmos.DrawLine(start, patrolPoint);
	}

	public Vector3 PatrolPoint { get => patrolPoint; set => patrolPoint = value; }
	public GuardState DefaultState { get => defaultState; set => defaultState = value; }
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GuardBehaviour))]
class GuardBehaviourEditor : UnityEditor.Editor
{
	void OnSceneGUI()
	{
		var guard = (GuardBehaviour)target;
		if (guard.DefaultState != GuardState.Patrol) return;
		UnityEditor.EditorGUI.BeginChangeCheck();
		Vector3 newPoint = UnityEditor.Handles.PositionHandle(guard.PatrolPoint, Quaternion.identity);
		if (UnityEditor.EditorGUI.EndChangeCheck())
		{
			UnityEditor.Undo.RecordObject(guard, "Move Patrol Point");
			guard.PatrolPoint = newPoint;
		}
	}
}
#endif