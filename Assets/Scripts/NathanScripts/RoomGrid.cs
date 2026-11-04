using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomGrid
{
    public RoomNode Room { get; private set; }

    public List<SpawnCell> Cells { get; private set; }

    public RoomGrid(RoomNode room)
    {
        Room = room;
        Cells = new List<SpawnCell>();

        GenerateCells();
    }

    private void GenerateCells()
    {
        for (int x = Room.BottomLeftAreaCorner.x +1;
        x < Room.TopRightAreaCorner.x - 1;
        x++)
        {
            for (int z = Room.BottomLeftAreaCorner.y + 1;
            z < Room.TopRightAreaCorner.y - 1;
            z++)
            {
                Vector3 worldPosition = new Vector3(
                x + 0.5f,
                0,
                z + 0.5f
                );

                

                Cells.Add(new SpawnCell(worldPosition));
            }
        }
    }
    //This will will detect objects occupying more than just 1 grid space
    private SpawnCell GetCellAt(int x, int z)
    {
        Vector3 position = new Vector3(x + 0.5f, 0, z + 0.5f);

        return Cells.Find(cell => cell.WorldPosition == position);

    }

    //Is going to check where a footprint fits in the grid
    public List<SpawnCell> GetCellsForFootprint(SpawnCell startingCell, Vector2Int footprint)
    {
        List<SpawnCell> footprintCells = new List<SpawnCell>();

        //After gets start int from List SpawnCell
        int startX = Mathf.FloorToInt(startingCell.WorldPosition.x);
        int startZ = Mathf.FloorToInt (startingCell.WorldPosition.z);

        //Then checks footprint cell from prefab x + z values
        for (int x = 0; x < footprint.x; x++)
        {
            for (int z = 0; z < footprint.y; z++)
            {
                SpawnCell cell = GetCellAt(startX + x, startZ + z);

                if (cell == null)
                    return null;

                footprintCells.Add(cell);
            }
        }

        return footprintCells;
    }

    //Is going to check if the required cells are avaialble in section
    public bool CanFitFootprint(SpawnCell startingCell, Vector2Int footprint)
    {
        List<SpawnCell> footprintCells = GetCellsForFootprint(startingCell,footprint);
            GetCellsForFootprint(startingCell, footprint);

        if (footprintCells == null)
            return false;
        
        foreach (SpawnCell cell in footprintCells)
        {
            if (cell.IsOccupied)
            return false;
        }

        return true;

    }
    //Spawns the object in the reserved cells.
    public void OccupyFootprint(SpawnCell startingCell, Vector2Int footprint, GameObject obj)
    {
        List<SpawnCell> footprintCells = 
            GetCellsForFootprint(startingCell, footprint);

        if (footprintCells == null)
        return; 

        foreach (SpawnCell cell in footprintCells)
        {
            cell.Occupy(obj);
        }
    }

    public Vector3 GetFootprintCenter(SpawnCell startingCell, Vector2Int footprint)
    {
        //This will allow us to get the Custom Cell Width + length and spawn the prop at its center
        List<SpawnCell> footprintCells = 
            GetCellsForFootprint(startingCell, footprint);

        if (footprintCells == null || footprintCells.Count == 0)
            return startingCell.WorldPosition;

        Vector3 center = Vector3.zero;

        foreach (SpawnCell cell in footprintCells)
        {
            center += cell.WorldPosition;
        }

        return center / footprintCells.Count;
    }

    // We are going to update Spawning instations 
}
