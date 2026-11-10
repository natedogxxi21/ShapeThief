using UnityEngine;

public class TutorialGuardInteract : GuardInteract
{
	bool caughtPlayer = false;

	protected override void OnCollisionEnter(Collision collision)
	{
		if (endGame && !caughtPlayer)
		{
			if (collision.gameObject.CompareTag("Player"))
			{
				caughtPlayer = true;
				RestartTransition.Restart(ReturnToCheckpoint);
			}
		}
	}

	void ReturnToCheckpoint()
	{
		GetComponent<GuardBehaviour>().ResetGuard();
		PlayerTouchMovement.Instance.transform.position = new(0, 0, 2);
		caughtPlayer = false;
	}
}