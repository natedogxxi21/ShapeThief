using UnityEngine;

public static class ExtensionMethods
{	
	public static Vector3 AddX(this Vector3 vector, float x) => new(vector.x + x, vector.y, vector.z);
	public static Vector3 AddY(this Vector3 vector, float y) => new(vector.x, vector.y + y, vector.z);
	public static Vector3 AddZ(this Vector3 vector, float z) => new(vector.x, vector.y, vector.z + z);

	public static Vector3 WithX(this Vector3 vector, float x) => new(x, vector.y, vector.z);
	public static Vector3 WithY(this Vector3 vector, float y) => new(vector.x, y, vector.z);
	public static Vector3 WithZ(this Vector3 vector, float z) => new(vector.x, vector.y, z);
}