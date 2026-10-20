
using UnityEngine;

public class SpawnBlocker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy (gameObject, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision otherObject)
    {
        Debug.Log("Message: Collision");
        if(otherObject.gameObject.tag == "EnemySpawnBlocker" )
        Destroy(otherObject.gameObject);
    }
}
