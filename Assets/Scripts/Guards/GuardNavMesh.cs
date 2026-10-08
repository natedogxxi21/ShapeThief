using Unity.AI.Navigation;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public partial class GuardNavMesh : MonoBehaviour
{
	[AutoStaticsCleanup]
	public static GuardNavMesh Instance;
	public bool Initialized { get; private set; }
	NavMeshSurface navMesh;

	void Awake()
	{
		Instance = this;
		navMesh = GetComponent<NavMeshSurface>();
	}

	public void Build()
	{
		navMesh.BuildNavMesh();
		Initialized = true;
	}
}
