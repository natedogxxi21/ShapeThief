using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonCreator : MonoBehaviour
{
    public DungeonSpawnManager spawnManager;
    // public NavigationBaker dungeonNavMesh;
    public GuardNavMesh guardNavMesh;
    public int dungeonwidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    public Material material;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    public float roomTopCornerModifier;


    [Header("Starting Room")]
    public GameObject startingRoomPrefab;
    public Transform playerTransform;

    public int startingRoomWidth = 15;
    public int startingRoomLength = 15;


    [Range(0,2)]
    public int roomOffset;

    //Keeping this down here so that I can include more room prefabs later
    //public DungeonRoomPrefabs startRoom;
    //public List<DungeonRoomPrefabs> roomPrefabs;
    //private List<DungeonRoomPrefabs> spawnedRooms = new List<DungeonRoomPrefabs>();

//New DungeonSpawnManager
    

    public GameObject wallVertical, wallHorizontal;
    public GameObject doorVertical, doorHorizontal;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;

//Adding new Room Grid identifications
private List<RoomGrid> roomGrids = new List<RoomGrid>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDungeon();
    }

    public void CreateDungeon()
    {
        DestroyAllChildren();
        //DungeonRoomPrefabs first = Instantiate(startRoom, Vector3.zero, Quaternion.identity); 
        //spawnedRooms.Add(first);
        
        DungeonGenerator generator = new DungeonGenerator(dungeonwidth,  dungeonLength);
        var listOfRooms = generator.CalculateDungeon(maxIterations, 
        roomWidthMin, 
        roomLengthMin, 
        roomBottomCornerModifier, 
        roomTopCornerModifier, 
        roomOffset,
        corridorWidth,
        startingRoomWidth,
        startingRoomLength
        );

        //Corridor list
        List<CorridorNode> corridors = listOfRooms
        .OfType<CorridorNode>()
        .ToList();

        //for creating starting room
        RoomNode startingRoom = listOfRooms
            .OfType<RoomNode>()
            .FirstOrDefault(room => room.isStartingRoom);

    //For Generating the Walls and Doors
    GameObject wallParent = new GameObject("WallParent");
    wallParent.transform.parent = transform;
    GameObject doorParent = new GameObject("DoorParent");
    doorParent.transform.parent = transform;
    possibleDoorVerticalPosition = new List<Vector3Int>();
    possibleDoorHorizontalPosition = new List<Vector3Int>();
    possibleWallHorizontalPosition = new List<Vector3Int>();
    possibleWallVerticalPosition = new List<Vector3Int>();

//Adding changes to DungeonCreator 10/07/2026
//Changes so we can create a grid system. Changing create mesh line and adding RoomGrid code
       roomGrids.Clear();
       
        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(
            listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);

        //idk
        //if (startingRoom != null && startingRoomPrefab != null)
          //  {
                

           // }    

          //Only creats a spawn grid for actual rooms
            if (listOfRooms[i] is RoomNode room)
            {
                RoomGrid grid = new RoomGrid(room);
                roomGrids.Add(grid);

            }
            //This will allow the first node to be our starting room before branching to more rooms
        }  
         
        if (startingRoom != null && startingRoomPrefab != null)
        {
            Vector3 roomPosition = new Vector3(
                (startingRoom.BottomLeftAreaCorner.x + startingRoom.TopRightAreaCorner.x) / 2f,
                0f,
                (startingRoom.BottomLeftAreaCorner.y + startingRoom.TopRightAreaCorner.y) /2f
            );

            GameObject decorations = Instantiate(startingRoomPrefab,
            roomPosition,
            Quaternion.identity, 
            transform);

            Transform spawnPoint = decorations.transform.Find("PlayerSpawnPoint");

            if (playerTransform != null && spawnPoint != null)
            {
               
                playerTransform.SetPositionAndRotation(
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();

                if (rb != null)
                {
                     rb.linearVelocity = Vector3.zero;
                     rb.angularVelocity = Vector3.zero;
                }

            }
            else
            {
               return;
            }
        }

        else if (startingRoom == null)
        {
            Debug.LogError("No starting room generated");
        }

        Debug.Log($"Horizontal door candidates: {possibleDoorHorizontalPosition.Count}");
Debug.Log($"Vertical door candidates: {possibleDoorVerticalPosition.Count}");

foreach (var p in possibleDoorHorizontalPosition)
    Debug.Log($"Horizontal door candidate at: {p}");

foreach (var p in possibleDoorVerticalPosition)
    Debug.Log($"Vertical door candidate at: {p}");

        CreateWalls(wallParent);

        CreateDoors(doorParent);
        
//Here we Add spawn Manager with the Create Dungeon

    // if (dungeonNavMesh != null)
    //     {
    //         dungeonNavMesh.BuildNavMesh();
    //     }
    if (guardNavMesh != null)
        {
            guardNavMesh.Build();
        }


    if (spawnManager != null)
        {
            spawnManager.Initialize(roomGrids);
        }
    

    }    

    private void OnDrawGizmos()
    {
        if (roomGrids == null)
        return;

        Gizmos.color = Color.green;

        
        foreach (RoomGrid grid in roomGrids)
        {
            foreach (SpawnCell cell in grid.Cells)
            {
                Vector3 center = cell.WorldPosition;


                Gizmos.DrawWireCube(
                    center,
                    new Vector3(1f, 0.05f, 1f)
                );
            }
        }
    }

    private void CreateDoors(GameObject doorParent)
    {
        SpawnCenteredDoors(
        possibleDoorHorizontalPosition,
        doorHorizontal,
        doorParent,
        true
    );

    SpawnCenteredDoors(
        possibleDoorVerticalPosition,
        doorVertical,
        doorParent,
        false
    );
    }

private void SpawnCenteredDoors(
    List<Vector3Int> positions,
    GameObject doorPrefab,
    GameObject doorParent,
    bool horizontal)
{
    if (doorPrefab == null || positions.Count == 0)
        return;

    // Group positions along the same wall.
    var groups = positions
        .GroupBy(p => horizontal ? p.z : p.x);

    foreach (var group in groups)
    {
        var sorted = group
            .OrderBy(p => horizontal ? p.x : p.z)
            .ToList();

        int i = 0;

        while (i < sorted.Count)
        {
            int j = i + 1;

            while (j < sorted.Count)
            {
                int previous = horizontal
                    ? sorted[j - 1].x
                    : sorted[j - 1].z;

                int current = horizontal
                    ? sorted[j].x
                    : sorted[j].z;

                if (current != previous + 1)
                    break;

                j++;
            }

            int runLength = j - i;

            if (runLength >= 3)
            {
                // Spawn one door at the center of this run.
                Vector3Int first = sorted[i];
                Vector3Int last = sorted[j - 1];

                Vector3 center = ((Vector3)first + (Vector3)last) / 2f;

                Instantiate(
                    doorPrefab,
                    center,
                    Quaternion.identity,
                    doorParent.transform
                );
            }

            i = j;
        }
    }
}
    private void CreateDoor(GameObject doorParent, Vector3Int doorPosition, GameObject doorPrefab)
    {
        Instantiate(doorPrefab, doorPosition, Quaternion.identity, doorParent.transform);
    }
    private void CreateWalls(GameObject wallParent)
    {
        foreach (var wallPosition in possibleWallHorizontalPosition)
        {
            CreateWall(wallParent, wallPosition, wallHorizontal);
        }

        foreach (var wallPosition in possibleWallVerticalPosition)
        {
            CreateWall(wallParent, wallPosition, wallVertical);
        }
    }

    //Create private void of creating individual walls
    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
    {
        Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);

    }

    //This will create our meshes for our "Dungeons"
    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i ++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);

        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
            
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        GameObject dungeonFloor = new GameObject("Mesh" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));

        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.localScale = Vector3.one;
        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = material;
        dungeonFloor.transform.parent = transform;

    //After GameObject wallParent is created and identified within void Create Mesh, now we can write the logic for creating the walls and doors.
        for (int row = (int)bottomLeftV.x; row <= (int)bottomRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, bottomLeftV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for(int row = (int)topLeftV.x; row <= (int)topRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int col = (int)bottomLeftV.z; col <= (int)topLeftV.z; col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        for (int col = (int)bottomRightV.z; col <= (int)topRightV.z; col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
    }


    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);

    
        
      
      // If a door already exists here, dont spawn another
       if (doorList.Contains(point))
       return;


       if (wallList.Contains(point))
        {
            doorList.Add(point);
            wallList.Remove(point);

        }
        else
        {
            wallList.Add(point);
        } 
    }

    private void ConsolidateDoorPositions()
{
    ConsolidateDoors(
        possibleDoorHorizontalPosition,
        possibleWallHorizontalPosition,
        true
    );

    ConsolidateDoors(
        possibleDoorVerticalPosition,
        possibleWallVerticalPosition,
        false
    );
}

private void ConsolidateDoors(
    List<Vector3Int> doorCandidates,
    List<Vector3Int> wallPositions,
    bool horizontal)
{
    HashSet<Vector3Int> remaining =
        new HashSet<Vector3Int>(doorCandidates);

    doorCandidates.Clear();

    while (remaining.Count > 0)
    {
        Vector3Int first = remaining.First();

        // Find every candidate in this continuous wall segment.
        List<Vector3Int> segment = remaining
            .Where(p => horizontal
                ? p.z == first.z
                : p.x == first.x)
            .OrderBy(p => horizontal ? p.x : p.z)
            .ToList();

        // Split the segment into continuous runs.
        List<List<Vector3Int>> runs = new List<List<Vector3Int>>();
        List<Vector3Int> run = new List<Vector3Int>();

        foreach (Vector3Int point in segment)
        {
            if (run.Count > 0)
            {
                // ????
                int previous = horizontal
                    ? run[run.Count - 1].x
                    : run[run.Count - 1].z;

                int current = horizontal ? point.x : point.z;

                if (current != previous + 1)
                {
                    runs.Add(run);
                    run = new List<Vector3Int>();
                }
            }

            run.Add(point);
        }

        if (run.Count > 0)
            runs.Add(run);

        foreach (List<Vector3Int> currentRun in runs)
        {
            foreach (Vector3Int point in currentRun)
                remaining.Remove(point);

            // A valid opening must fit three adjacent grid positions.
            if (currentRun.Count < 3)
            {
                foreach (Vector3Int point in currentRun)
                    wallPositions.Add(point);

                continue;
            }

            int start = (currentRun.Count - 3) / 2;

            for (int i = 0; i < currentRun.Count; i++)
            {
                Vector3Int point = currentRun[i];

                if (i >= start && i < start + 3)
                    doorCandidates.Add(point);
                else
                    wallPositions.Add(point);
            }
        }
    }
}
    private void DestroyAllChildren()
    {
        while(transform.childCount != 0)
        {
            foreach(Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}
