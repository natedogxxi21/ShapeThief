using UnityEngine;

public class TutorialLootProp : LootProp
{

	protected override void OnCollisionEnter(Collision collision)
	{
		if (collected) { return; }
		if (collision.gameObject.CompareTag("Player"))
		{
			PlayerStats.ChangeMoney(lootValue);
			SFXManager.PlayClip(collectClip, volume: 0.75f, transform.position, Random.Range(-.05f, .075f));
			collected = true;
			TutorialFlow.looted = true;
			Destroy(gameObject);
			return;
		}

		if (collision.impulse.sqrMagnitude >= .5f)
		{
			Vector3 pos = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
			AudioCueManager.PlayCollisionSound(collisionClip, pos, collision.impulse.magnitude);
		}
	}
}