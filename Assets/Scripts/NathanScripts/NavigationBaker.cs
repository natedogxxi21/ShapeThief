
using Unity.AI.Navigation;
using UnityEngine;


public class NavigationBaker : MonoBehaviour 
{
    
    public NavMeshSurface navMeshSurface;
    public void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

     public void BuildNavMesh()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("DungeonNavMesh: No NavMeshSurface found.");
            return;
        }

        navMeshSurface.BuildNavMesh();

        Debug.Log("Dungeon NavMesh built successfully.");
    }


}
