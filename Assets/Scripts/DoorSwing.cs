using UnityEngine;

public class DoorSwing : MonoBehaviour
{
    public float pushForce = 2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Vector3 direction = collision.relativeVelocity;
       direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            Vector3 push = direction.normalized * pushForce;

            rb.AddForceAtPosition
            (direction.normalized * pushForce, collision.GetContact(0).point, ForceMode.Impulse);
        }
    }
}