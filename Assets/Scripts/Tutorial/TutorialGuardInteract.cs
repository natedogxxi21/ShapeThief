using UnityEngine;

public class TutorialGuardInteract : GuardInteract
{
	bool caughtPlayer = false;
	[SerializeField] Transform movePlayerTo;

	protected override void OnCollisionEnter(Collision collision)
	{
		if (endGame && !caughtPlayer)
		{
			if (collision.gameObject.CompareTag("Player"))
			{
				caughtPlayer = true;
				RestartTransition.Restart();
			}
		}
	}

	public void ReturnToCheckpoint()
	{
		GetComponent<GuardBehaviour>().ResetGuard();
		PlayerTouchMovement.Instance.transform.position = movePlayerTo.position;
		caughtPlayer = false;
	}
}