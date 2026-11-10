using UnityEngine;

public class Pulse : MonoBehaviour
{
	[SerializeField] float pulseSpeed = 3f;
	[SerializeField] float pulseStrength = 0.25f;
	RectTransform rect;
	Vector3 originalScale;

	void Start()
	{
		rect = GetComponent<RectTransform>();
		originalScale = rect.lossyScale;
		Update();
	}

	void Update()
	{
		float s = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed) * pulseStrength) + 1;
		rect.localScale = originalScale * s;
	}
}