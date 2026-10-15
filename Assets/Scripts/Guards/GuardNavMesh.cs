using System.Collections.Generic;
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

	public List<GuardBehaviour> RegisteredGuards { get; private set; } = new();

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

	public bool RegisterGuard(GuardBehaviour guard)
	{
		if (!RegisteredGuards.Contains(guard))
		{
			RegisteredGuards.Add(guard);
			return true;
		}
		return false;
	}
	
	public bool DeregisterGuard(GuardBehaviour guard)
	{
		if (RegisteredGuards.Contains(guard))
		{
			RegisteredGuards.Remove(guard);
			return true;
		}
		return false;
	}
}
