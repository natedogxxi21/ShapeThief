using UnityEngine;
using UnityEngine.SceneManagement;

public class GuardInteract : MonoBehaviour
{
	[SerializeField] bool endGame = true;
	[SerializeField] GameObject seeThroughBody;

	void Update()
	{
		float playerDistance = Vector3.Distance(transform.position, PlayerTouchMovement.Instance.transform.position);
		seeThroughBody.SetActive(playerDistance <= PlayerStats.GuardDetectRadius);
	}

	void OnCollisionEnter(Collision collision)
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