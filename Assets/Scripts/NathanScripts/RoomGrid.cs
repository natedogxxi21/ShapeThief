using System.Collections.Generic;
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
}
