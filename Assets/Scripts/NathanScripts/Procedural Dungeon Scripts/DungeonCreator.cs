using System;
using System.Collections;
using System.Collections.Generic;
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
    [Range(0, 2)]

//New DungeonSpawnManager

    public int roomOffset;
    public GameObject wallVertical, wallHorizontal;
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

    private void CreateDungeon()
    {
        DungeonGenerator generator = new DungeonGenerator(dungeonwidth,  dungeonLength);
        var listOfRooms = generator.CalculateDungeon(maxIterations, 
        roomWidthMin, 
        roomLengthMin, 
        roomBottomCornerModifier, 
        roomTopCornerModifier, 
        roomOffset,
        corridorWidth
        );

    //For Generating the Walls and Doors
    GameObject wallParent = new GameObject("WallParent");
    wallParent.transform.parent = transform;
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
                
          //Only creats a spawn grid for actual rooms
            if (listOfRooms[i] is RoomNode room)
            {
                RoomGrid grid = new RoomGrid(room);
                roomGrids.Add(grid);

            }
        }
        CreateWalls(wallParent);
        
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
    
    //After GameObject wallParent is created and identified within void Create Mesh, now we can write the logic for creating the walls and doors.
        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, bottomLeftV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for(int row = (int)topLeftV.x; row < (int)topRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
    }

    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
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
}
