using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    [SerializeField]
    private Door door;


    private readonly Dictionary<Collider, Transform> occupants = new();

    private readonly Dictionary<Transform, int> actorCounts = new();

    private void OnTriggerEnter (Collider other)
    {

    Transform actor = GetActor(other);

    if (actor == null )
        return;
    Debug.Log($"Door detected: {actor.name} through collider: {other.name}");

    door.Open(actor.position);
    
    occupants.Add(other, actor);

    if (actorCounts.TryGetValue(actor, out int count))
        actorCounts[actor] = count + 1;
//??
    else
        actorCounts.Add(actor, 1);

    if (door != null && !door.IsOpen)
        door.Open(actor.position);
Debug.Log("Trigger entered by: " + other.name +
          " | Tag: " + other.tag);
    }
       

private void OnTriggerExit(Collider other)
    {
        if (!occupants.TryGetValue(other, out Transform actor))
            return;
        
        occupants.Remove(other);

        if (actorCounts.ContainsKey(actor))
        {
            actorCounts[actor]--;

            if (actorCounts[actor] <= 0)
                actorCounts.Remove(actor);

        }
        
        // Only closes and checks when both the player and guards have left the area.
        if (actorCounts.Count == 0 && door != null && door.IsOpen)
            door.Close();
        //if (other.TryGetComponent<CharacterController>(out CharacterController controller))
        //{
            //if (Door.IsOpen)
            //{
                //Door.Close();
           // }
        //}
    }
private Transform GetActor(Collider other)
{
    // First check this collider belongs to a rigidbody
    Rigidbody rb = other.attachedRigidbody;

    if (rb != null && rb.CompareTag("Player"))
        return rb.transform;

    // Check the parent hierarchy for the player tag
    Transform current = other.transform;

    while (current != null)
    {
        if (current.CompareTag("Player"))
            return current;

        current = current.parent;
    }

    // Check  guards
    GuardController guard =
        other.GetComponentInParent<GuardController>();

    if (guard != null)
        return guard.transform;

    return null;
}

}