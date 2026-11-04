using UnityEngine;

public class SpawnCell
{
    public Vector3 WorldPosition { get; private set; }

    public bool IsOccupied { get; set; }

    public GameObject OccupyingObject { get; private set;}

    public bool CanSpawnEnemy { get; set; }
    public bool CanSpawnItem { get; set; }
    public bool CanSpawnObject { get; set; }

    public SpawnCell(Vector3 worldPosition)
    {
        WorldPosition = worldPosition;

        IsOccupied = false;

        CanSpawnEnemy = true;
        CanSpawnItem = true;
        CanSpawnObject = true;

    }

    public void Occupy(GameObject obj)
    {
        IsOccupied = true;
        OccupyingObject = obj;
    }

    public void Clear()
    {
        IsOccupied = false;
        OccupyingObject = null;
    }

   
}

