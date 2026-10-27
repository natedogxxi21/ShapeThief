using UnityEngine;

public class LootProp : MonoBehaviour
{
	public Mesh mesh;
	[SerializeField] int lootValue;
	[SerializeField] AudioClip collectClip;
	bool collected;

	void OnCollisionEnter(Collision collision)
	{
		if (collected) { return; }
		if (collision.gameObject.CompareTag("Player"))
		{
			PlayerStats.ChangeMoney(lootValue);
			SFXManager.PlayClip(collectClip, 1, transform.position, Random.Range(-.1f, .1f));
			collected = true;
			Destroy(gameObject);
		}
	}
}