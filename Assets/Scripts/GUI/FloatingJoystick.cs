using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class FloatingJoystick : MonoBehaviour
{
	[HideInInspector] public RectTransform rectTransform;
	public RectTransform knob;
	public Vector2 size = new(300, 300);

	void Awake() => rectTransform = GetComponent<RectTransform>();
}