using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomTemplates : MonoBehaviour
{
    //Public GameObjects for Rooms
    public GameObject[] bottomRooms;
    public GameObject[] topRooms;
    public GameObject[] leftRooms;
    public GameObject[] rightRooms;

    //We need to create a public GameObject to call hallways that will connect to other rooms.
    //This will help us be able to connect rooms from one another without major issues of rooms spawning in awkward positions and overlapping.
    
    //Public GameObject Hallways
    public GameObject[] bottomhallways;
    public GameObject[] tophallways;
    public GameObject[] lefthallways;
    public GameObject[] righthallways; 

    
    public GameObject[] closedRoom;

    public List<GameObject> rooms;


}
