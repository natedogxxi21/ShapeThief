
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
            return;
        }

        navMeshSurface.BuildNavMesh();

    }


}
