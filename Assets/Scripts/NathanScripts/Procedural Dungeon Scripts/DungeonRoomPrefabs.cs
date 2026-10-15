using UnityEngine;

public enum RoomType 
{
    Start,
    Vault,
}

public class DungeonRoomPrefabs : MonoBehaviour
{
    public RoomType roomType;
    //public Transform[] doorPoints;
    //I will go more over this once I start to implement doors more, 
    //Possibly refer to door setup already created from Unity Dungeon Generator Tutorial
}
