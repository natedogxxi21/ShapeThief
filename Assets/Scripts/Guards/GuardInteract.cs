using UnityEngine;
using UnityEngine.SceneManagement;

public class GuardInteract : MonoBehaviour
{
	[SerializeField] protected bool endGame = true;
	[SerializeField] GameObject seeThroughBody;
	[SerializeField] float footstepDistance = 1.8f;

	float footstepTicker;
	Vector3 lastFramePos;

	void Update()
	{
		float playerDistance = Vector3.Distance(transform.position, PlayerTouchMovement.Instance.transform.position);
		seeThroughBody.SetActive(playerDistance <= PlayerStats.GuardDetectRadius);

		footstepTicker = Vector3.SqrMagnitude(transform.position - lastFramePos);
		lastFramePos = transform.position;

		if (footstepTicker * footstepTicker >= footstepDistance)
		{
			AudioCueManager.PlayFootstep(transform.position, false, 0.75f, stimulus: false);
			footstepTicker = 0;
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (endGame)
		{
			if (collision.gameObject.CompareTag("Player"))
			{
				SceneManager.LoadScene("GameOver");
			}
		}
	}
}