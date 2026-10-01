using UnityEngine;

public class Billboard : MonoBehaviour
{
	void LateUpdate()
	{
		if (transform.gameObject.activeSelf)
		{
			transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
		}
	}
}