using UnityEngine;

public class ShiftProp : MonoBehaviour
{
	[SerializeField] Mesh mesh;
	public Mesh Mesh { get => mesh; }
	[SerializeField] Material material;
	public Material Material { get => material; }

	[SerializeField] GameObject highlight;

	public void Highlight(bool on)
	{ if (highlight != null) { highlight.SetActive(on); } }
}