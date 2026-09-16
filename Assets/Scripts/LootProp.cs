using ITISKIRUHERE;
using UnityEditor;
using UnityEngine;

public class LootProp : MonoBehaviour
{
	public Mesh mesh;
	[SerializeField] int lootValue;
	bool collected;

	void OnCollisionEnter(Collision collision)
	{
		if (collected) { return; }
		if (collision.gameObject.CompareTag("Player"))
		{
			PlayerStats.ChangeMoney(lootValue);
			collected = true;
			Destroy(gameObject);
		}
	}
}