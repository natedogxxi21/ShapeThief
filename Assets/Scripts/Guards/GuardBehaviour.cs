using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
		Chase, // Run towards a point
		Return // Walking back to spawn point
	}

	static readonly float[] stateTickDurations = { 10f, 4f, .2f, .2f, .2f, 1f };

	GuardController controller;
	[SerializeField] GuardVision vision;
	[SerializeField] GuardState state = GuardState.Idle;
	[SerializeField] GuardState defaultState;
	Timer stateTickTimer;
	Vector3 spawnPoint;

	[SerializeField] GameObject investigateSightIcon;
	[SerializeField] GameObject chaseIcon;

	Vector3 postPoint;
	float postDirection;
	bool postFacingLeft;

	[Space]
	[SerializeField] Timer wanderTimer;

	GameObject seenPlayer = null;
	Vector3 investigatePoint;

	bool chaseInvestigating = false;
	[SerializeField] Timer chaseInvestigateTimer;

	void Start() => StartCoroutine(WaitToLoadCoroutine());

	IEnumerator WaitToLoadCoroutine()
	{
		while (GuardNavMesh.Instance == null || !GuardNavMesh.Instance.Initialized)
		{
			yield return null;
		}

		controller = GetComponent<GuardController>();
		controller.enabled = true;
		Wander();
		spawnPoint = transform.position;
		vision.onPlayerSeen.AddListener(OnPlayerSeen);
		vision.onPlayerLost.AddListener(OnPlayerLost);
		vision.onPlayerNoticed.AddListener(OnPlayerNoticed);
	}

	void Update()
	{
		if (stateTickTimer.Completed)
		{
			TickState();
		}

		// visibility of icons set by State Functions
		if (state == GuardState.Investigate)
		{
			Image icon = investigateSightIcon.GetComponent<Image>();
			icon.fillAmount = vision.Suspicion;
			icon.color = Color.Lerp(Color.white, Color.yellow, vision.Suspicion);
			icon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, vision.Suspicion);
		}
		else if (state == GuardState.Chase)
		{
			Image icon = chaseIcon.GetComponent<Image>();
			float p = chaseInvestigateTimer.Remaining / chaseInvestigateTimer.Duration;
			icon.fillAmount = p;
			icon.color = Color.Lerp(Color.white, Color.yellow, p);
			icon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, p);
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
				if (controller.Traveling) { wanderTimer.Pause(); }
				else { wanderTimer.Resume(); }

				if (wanderTimer.Completed)
				{
					wanderTimer.Reset();
					controller.WalkTo(GetWanderPoint());
				}
				break;

			case GuardState.Investigate:
				controller.WalkTo(investigatePoint);
				if (vision.Suspicion <= 0)
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

						default:
							throw new Exception("Cannot accept Guard State " + defaultState + " as default state");
					}
				}
				break;
		}
	}

	public void Idle() => SetState(GuardState.Idle);
	public void Post(Vector3 position, float direction)
	{
		postPoint = position;
		postDirection = direction;
		SetState(GuardState.Post);
	}
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
	public void Investigate(Vector3 point)
	{
		investigateSightIcon.SetActive(true);
		chaseIcon.SetActive(false);

		investigatePoint = point;
		SetState(GuardState.Investigate);
	}
	public void Chase(GameObject player)
	{
		investigateSightIcon.SetActive(false);
		chaseIcon.SetActive(true);

		seenPlayer = player;
		chaseInvestigating = false;
		chaseInvestigateTimer = 3;
		investigatePoint = seenPlayer.transform.position;
		SetState(GuardState.Chase);
	}
	public void Return()
	{
		investigateSightIcon.SetActive(false);
		chaseIcon.SetActive(false);
		SetState(GuardState.Return);
	}

	void SetState(GuardState newState)
	{
		state = newState;
		stateTickTimer = stateTickDurations[(int)newState];
		controller.StoppingDistance = newState == GuardState.Investigate ? 5f : 0f;
		TickState();
	}

	void OnPlayerSeen(GameObject player) => Chase(player);
	void OnPlayerLost(GameObject player) => seenPlayer = null;
	void OnPlayerNoticed(GameObject player)
	{
		if (state != GuardState.Chase)
		{
			Investigate(player.transform.position);
		}
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