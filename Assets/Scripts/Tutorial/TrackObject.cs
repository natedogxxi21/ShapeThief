using UnityEngine;

public class TrackObject : MonoBehaviour
{
	public Transform target;
	RectTransform rect;

	void Start()
	{
		rect = GetComponent<RectTransform>();
		Update();
	}

	void Update()
	{
		if (target != null)
		{
			Vector2 p = Camera.main.WorldToScreenPoint(target.position);
			rect.anchoredPosition = p;
		}
	}
}