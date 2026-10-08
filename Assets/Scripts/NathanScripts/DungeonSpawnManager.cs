using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DungeonSpawnManager : MonoBehaviour
{
    //Spawn Prefabs
    public GameObject enemyPrefab;
    public GameObject itemPrefab;
    public GameObject objectPrefab;

    //Testing Spawns per room

    //EnemySpawning
    [Range(0f, 1f)]
    public float enemySpawnChance = 0.8f;

    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRooom = 2;

    //Item Spawning
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.5f;

    public int minItemsPerRoom = 1;
    public int maxItemsPerRoom = 2;

    //Object Spawning
    [Range(0f, 1f)]
    public float objectSpawnChance = 0.7f;

    public int minObjectsPerRoom = 3;
    public int maxObjectsPerRoom = 5;

    private List<RoomGrid> roomGrids;

    public void Initialize(List<RoomGrid> grids)
    {
        roomGrids = grids;

        SpawnDungeonContents();
    }

    private void SpawnDungeonContents()
    {
        if (roomGrids == null || roomGrids.Count == 0)
        {
            Debug.LogWarning("DungeonSpawnManager: No room grids were provided.");
            return;
        }

        foreach (RoomGrid roomGrid in roomGrids)
        {
            SpawnEnemies(roomGrid);
            SpawnItems(roomGrid);
            SpawnObjects(roomGrid);
        }
    }

    private void SpawnEnemies(RoomGrid roomGrid)
    {
        if (enemyPrefab == null)
        return;

        //Decides of the room will get enemies
        if (Random.value > enemySpawnChance)
        return;

        //Pick a random number between minimum and maximum
        int enemyCount = Random.Range(
            minEnemiesPerRoom,
            maxEnemiesPerRooom + 1);
        

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnCell cell = GetAvailableEnemyCell(roomGrid);

            if (cell == null)
            {
                Debug.LogWarning("Not enough valid cells to spawn all enemies in room.");
                return;
            }

            Instantiate(
                enemyPrefab,
                cell.WorldPosition,
                Quaternion.identity,
                transform
            );

            cell.IsOccupied = true;
        }
    }

    private void SpawnItems(RoomGrid roomGrid)
    {
        if (itemPrefab == null)
        return;


        //Decides of the room will get enemies
        if (Random.value > itemSpawnChance)
        return;

        int itemCount = Random.Range(
            minItemsPerRoom,
            maxItemsPerRoom + 1);
        
        for (int i = 0; i < itemCount; i++)
        {
            SpawnCell cell = GetAvailableItemCell(roomGrid);

            if(cell == null)
            {
                Debug.LogWarning("Not enough valid cells to spawn all items in room.");
                return;
            }

            Instantiate(
            itemPrefab,
            cell.WorldPosition + Vector3.up * 0.5f,
            Quaternion.identity,
            transform
            );

            cell.IsOccupied = true;
        }
    }

    private void SpawnObjects(RoomGrid roomGrid)
    {
        if (objectPrefab == null)
        return;

        //Decides whether an object will spawn
        if (Random.value > objectSpawnChance)
        return;

        int objectCount = Random.Range(
            minObjectsPerRoom,
            maxObjectsPerRoom + 1);
        

        for (int i = 0; i < objectCount; i++)
        {
            SpawnCell cell = GetAvailableObjectCell(roomGrid);

            if(cell == null)
            {
                Debug.LogWarning("Not enough valid cells to spawn all items in room.");
                return;
            }

            Instantiate(
            objectPrefab,
            cell.WorldPosition,
            Quaternion.identity,
            transform
            );

            cell.IsOccupied = true;
        }

    }

    //Now with those 3 categories made. We make void for the GetAvailable[]Cell
private SpawnCell GetAvailableEnemyCell(RoomGrid roomGrid)
    {
        List<SpawnCell> availableCells = new List<SpawnCell>();

        foreach (SpawnCell cell in roomGrid.Cells)
        {
            if (!cell.IsOccupied && cell.CanSpawnEnemy)
            {
                availableCells.Add(cell);
            }
        }
    
     if (availableCells.Count == 0)
        return null;

     int randomIndex = Random.Range(0, availableCells.Count);

     return availableCells[randomIndex];
    
    }

    private SpawnCell GetAvailableItemCell(RoomGrid roomGrid)
    {
        List<SpawnCell> availableCells = new List<SpawnCell>();

        foreach (SpawnCell cell in roomGrid.Cells)
        {
            if (!cell.IsOccupied && cell.CanSpawnItem)
            {
                availableCells.Add(cell);
            }
        }
    
     if (availableCells.Count == 0)
        return null;

     int randomIndex = Random.Range(0, availableCells.Count);

     return availableCells[randomIndex];
    
    }
   
    private SpawnCell GetAvailableObjectCell(RoomGrid roomGrid)
    {
         List<SpawnCell> availableCells = new List<SpawnCell>();

        foreach (SpawnCell cell in roomGrid.Cells)
        {
            if (!cell.IsOccupied && cell.CanSpawnObject)
            {
                availableCells.Add(cell);
            }
        }
    
     if (availableCells.Count == 0)
        return null;

     int randomIndex = Random.Range(0, availableCells.Count);

     return availableCells[randomIndex];

    }


}
