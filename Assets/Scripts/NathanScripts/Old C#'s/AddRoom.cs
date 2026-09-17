using UnityEngine;

public class AddRoom : MonoBehaviour
{
    private RoomTemplates templates;

    void Start()
    {
        templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomTemplates>();
       //sets template equal to the tag of the gameobject with the "rooms" tag and the room templates attached to the object.
        templates.rooms.Add(this.gameObject);
        //Now we can add the rooms to the template
        


    }
}
