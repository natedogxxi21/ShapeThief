using UnityEngine;

[CreateAssetMenu(fileName = "Prop", menuName = "Scriptable Objects/Prop")]
public class Prop : ScriptableObject
{
	public string propName;
	public Mesh mesh;
	public Material material;
	public Vector3 scale;
}