using UnityEngine;

public class DoorSwing : MonoBehaviour
{
    public float pushForce = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Vector3 hitDirection = collision.relativeVelocity;
        hitDirection.y = 0;

        if (hitDirection.sqrMagnitude > 0.01f)
        {
            rb.AddTorque(Vector3.up * pushForce, ForceMode.Impulse);
        }
    }
}