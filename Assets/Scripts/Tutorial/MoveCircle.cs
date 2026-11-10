using UnityEngine;

public class MoveCircle : MonoBehaviour
{
	[SerializeField] float radius = 50;
	[SerializeField] float speed = 1;
	RectTransform rect;
	Vector2 startPos;

	void Start()
	{
		rect = GetComponent<RectTransform>();
		startPos = rect.anchoredPosition;
		Update();
	}

	void Update()
	{
		float t = Time.time * speed;
		Vector2 offset = new Vector2(-Mathf.Cos(t), Mathf.Sin(t)) * radius;
		rect.anchoredPosition = startPos + offset;
	}
}