using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{

    public GameObject spawnObject;
    public Transform spawnLocation;
    void Start()
    {
        
        if  (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(spawnObject, spawnLocation.position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    
    void Update()
    {
        
    }

    
}
