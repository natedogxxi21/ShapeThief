using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
	NavMeshAgent navAgent;

	[SerializeField] float walkSpeed;
	[SerializeField] float runSpeed;

	public bool Traveling => navAgent.velocity.sqrMagnitude > 0.3f;
	public float RemainingDistance => navAgent.remainingDistance;
	public float StoppingDistance
	{ 
		get => navAgent.stoppingDistance;
		set => navAgent.stoppingDistance = value;
	}

	void OnEnable()
	{
		navAgent = GetComponent<NavMeshAgent>();
		navAgent.enabled = true;
	}

	public void WalkTo(Vector3 pos) { navAgent.speed = walkSpeed; navAgent.SetDestination(pos); }
	public void WalkTo(Transform target) { navAgent.speed = walkSpeed; navAgent.SetDestination(target.position); }
	public void RunTo(Vector3 pos) { navAgent.speed = runSpeed; navAgent.SetDestination(pos); }
	public void RunTo(Transform target) { navAgent.speed = runSpeed; navAgent.SetDestination(target.position); }

	public void Stop() => navAgent.SetDestination(transform.position);
	public void FaceToward(float angle) => transform.eulerAngles.WithY(angle);

	public void GlanceToward() => throw new System.NotImplementedException();
}