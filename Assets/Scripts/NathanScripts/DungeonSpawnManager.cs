using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DungeonSpawnManager : MonoBehaviour
{
    //Spawn Prefabs
    public GameObject enemyPrefab;
    public GameObject itemPrefab;

 [Header("Prop Placement")]
    public PropSpawnLocation objectSpawnLocation = PropSpawnLocation.Anywhere;

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
    [Header("Room Props")]
    public List<PropSpawnSettings> props = new List<PropSpawnSettings>();

    public Collider[] collider;

    private List<RoomGrid> roomGrids;
    private float radius;

    public enum PropSpawnLocation
    {
        Anywhere,
        EdgeOnly,
        CornerOnly,
        CenterOnly
    }


    [System.Serializable]
    public class PropSpawnSettings
    {
        public string propName;
        public GameObject prefab;

        [Range(0f, 1f)]
        public float spawnChance = 0.7f;

        public int minPerRoom = 1;
        public int maxPerRoom = 3;

        public PropSpawnLocation spawnLocation =  PropSpawnLocation.Anywhere;

        public float verticalOffset = 0.5f;   

    }

    public void Initialize(List<RoomGrid> grids)
    {
        roomGrids = grids;
        
        SpawnDungeonContents();
    }

    
    private void SpawnDungeonContents()
    {
        if (roomGrids == null || roomGrids.Count == 0 )
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

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemySpawnBlocker"))
        {
           print("Enemy Spawn Is Blocked!");

           return; 
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
       foreach (PropSpawnSettings prop in props)
        {
            if (prop == null || prop.prefab == null)
            continue;

            if (Random.value > prop.spawnChance)
            continue;

            int minCount = Mathf.Max(0, prop.minPerRoom);
            int maxCount = Mathf.Max(minCount, prop.maxPerRoom);

            int objectCount = Random.Range(minCount, maxCount + 1);

             for (int i = 0; i < objectCount; i++)
             {
                 SpawnCell cell = GetAvailablePropCell(roomGrid, prop.spawnLocation);


                 if(cell == null)
                 {
                     Debug.LogWarning($"No available shit");
                     break;
                 }

                Vector3 spawnPosition = cell.WorldPosition + Vector3.up * prop.verticalOffset;

                Instantiate(
                 prop.prefab,
                spawnPosition,
                 Quaternion.identity,
                 transform
                 );

                 cell.IsOccupied = true;
             }
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
   
   //Alright this one is going to be a little complicated compared to the other ones

    private SpawnCell GetAvailablePropCell(RoomGrid roomGrid, PropSpawnLocation location)
   
    {

        if (roomGrid.Cells == null || roomGrid.Cells == null || roomGrid.Cells.Count == 0)
        {   
            return null;
        }

        List<SpawnCell> availableCells = new List<SpawnCell>();


        //Finding the boundaries of the room's spawn grid
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (SpawnCell cell in roomGrid.Cells)
        {
            Vector3 pos = cell.WorldPosition;

            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minZ = Mathf.Min(minZ, pos.z);
            maxZ = Mathf.Max(maxZ, pos.z);
        }

        float centerX = (minX + maxX) /2f;
        float centerZ = (minZ + maxZ) /2f;

        foreach (SpawnCell cell in roomGrid.Cells)
        {

            if (cell.IsOccupied && !cell.CanSpawnObject)
                continue;
           

                Vector3 pos = cell.WorldPosition;

                bool left = Mathf.Approximately(pos.x, minX);
                bool right = Mathf.Approximately(pos.x, maxX);
                bool bottom = Mathf.Approximately(pos.z, minZ);
                bool top =  Mathf.Approximately(pos.z, maxZ);
                
                bool edge = left || right || bottom || top;
                bool corner = (left || right) &&  (bottom || top);

                bool center = 
                !edge &&
                 Mathf.Abs(pos.x - (minX + maxX) / 2f)
                    <= (maxX - minX) * 0.25f &&
                 Mathf.Abs(pos.z - (minZ + maxZ) / 2f)
                    <= (maxZ - minZ) * 0.25f;

                bool matches;

            switch (location)
            {
            case PropSpawnLocation.EdgeOnly:
                matches = edge;
                break;

            case PropSpawnLocation.CornerOnly:
                matches = corner;
                break;

            case PropSpawnLocation.CenterOnly:
                matches = center;
                break;

            default:
                matches = true;
                break;
            } 

            if (matches)
                availableCells.Add(cell);
        }
    
     if (availableCells.Count == 0)
        return null;

     return availableCells[Random.Range(0, availableCells.Count)];

    }


}
