using UnityEngine;

public class RandomRoomGenerator : MonoBehaviour
{
    

public int openingDirection;
// 1 --> need bottom door
// 2 --> need top door
// 3 --> need left door
// 4 --> need right door


private RoomTemplates templates;
private int rand;
private bool spawned = false;

    void Start()
    {
        templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomTemplates>();
        Invoke("Spawn", 1f);

    }

    // Update is called once per frame
    void Spawn()
    {
        //we need to create a seperate "void Spawn()" for rooms to spawn hallways instead, and then call void Spawn() for the spawnpoints of those hallways
        //In order to get more advanced room generation, I am going to need to create a lot of empty game objects within my room prefabs.
        //I could go more advanced with the tutorial that allows you to change the x and z values of individual rooms but I believe having rooms premade will alieve me of all that stress.
        
        if(spawned == false)
        {
            if(openingDirection == 1){
            // Need to spawn a room with a BOTTOM door.
            rand = Random.Range(0, templates.bottomRooms.Length);
            Instantiate(templates.bottomRooms[rand], transform.position, templates.bottomRooms[rand].transform.rotation);
            print ("Spawned Room 1");
        } 
        else if(openingDirection == 2){
            rand = Random.Range(0, templates.topRooms.Length);
            Instantiate(templates.topRooms[rand], transform.position, templates.topRooms[rand].transform.rotation);
            print ("Spawned Room 2");

            // Need to spawn a room with a TOP door.
        } 
        else if(openingDirection == 3){
            rand = Random.Range(0, templates.leftRooms.Length);
            Instantiate(templates.leftRooms[rand], transform.position, templates.leftRooms[rand].transform.rotation);
            print ("Spawned Room 3");

            // Need to spawn a room with a LEFT door.
        } 
        else if(openingDirection == 4){
             rand = Random.Range(0, templates.rightRooms.Length);
            Instantiate(templates.rightRooms[rand], transform.position, templates.rightRooms[rand].transform.rotation);
            print ("Spawned Room 4");

            // Need to spawn a room with a RIGHT door.
         } 
            spawned = true;
        }

       

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("SpawnPoint"))   
        {
            if(other.CompareTag("SpawnPoint") && other.GetComponent<RandomRoomGenerator>().spawned == true)
          // if(other.GetComponent<RandomRoomGenerator>().spawned == false && spawned == false)
            {
                // Instantiate(templates.closedRoom[rand], transform.position, Quaternion.identity);

                // spawn walls blocking off any openings
               
                Destroy(gameObject);
            }
            }
           
            //spawned = true;
    }
}
