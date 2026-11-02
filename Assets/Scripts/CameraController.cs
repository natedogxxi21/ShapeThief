using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] Transform camYaw;
	[SerializeField] Transform camPitch;
	[SerializeField] Transform camTransform;

	[SerializeField] float lerpCamOcclusionSpeed;
	[SerializeField] float camDistance;
	[SerializeField] float lookSensitivity;
	[SerializeField] LayerMask occlusionMask;

	public float yaw = 0;
	public float pitch = 45;
	float currentCamDist;

	void FixedUpdate()
	{
		bool camOccluded = Physics.SphereCast(
			origin: camYaw.position,
			radius: 0.5f,
			direction: -camPitch.forward,
			hitInfo: out RaycastHit hit,
			maxDistance: camDistance,
			layerMask: occlusionMask
		);

		float targetCamDistance = camOccluded ? hit.distance : camDistance;
		float attemptedCamDistance = Mathf.Lerp(currentCamDist, targetCamDistance, lerpCamOcclusionSpeed * Time.deltaTime);
		currentCamDist = Mathf.Min(attemptedCamDistance, targetCamDistance);
		camTransform.position = camYaw.position + (-camPitch.forward * currentCamDist);
	}

	public void ProcessLookInput(Vector2 input)
	{
		yaw += input.x * lookSensitivity;
		pitch = Mathf.Clamp(pitch - (input.y * lookSensitivity), 10, 80);

		camYaw.localEulerAngles = new Vector3(0, yaw, 0);
		camPitch.localEulerAngles = new Vector3(pitch, 0, 0);
	}
}