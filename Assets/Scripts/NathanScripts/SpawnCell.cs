using UnityEngine;

public class SpawnCell
{
    public Vector3 WorldPosition { get; private set; }

    public bool IsOccupied { get; set; }

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
}
