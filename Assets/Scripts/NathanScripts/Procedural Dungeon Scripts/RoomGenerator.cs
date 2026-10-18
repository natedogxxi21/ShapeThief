using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator
{
    //Room Generator
    private int maxIterations;
    private int roomLengthMin;
    private int roomWidthMin;

    public RoomGenerator(int maxIterations, int roomLengthMin, int roomWidthMin)
    {
        this.maxIterations = maxIterations;
        this.roomLengthMin = roomLengthMin;
        this.roomWidthMin = roomWidthMin;
    }

public List<RoomNode> GenerateRoomsInGivenSpaces(List<Node> roomSpaces, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset)
    {
        List<RoomNode> listToReturn = new List<RoomNode>();
        foreach (var space in roomSpaces)
        {
// (From BinarySpace Partitioning line 75.) Adding starting room structure before calling structure helper

            RoomNode room = (RoomNode)space;

            if (room.isStartingRoom)
            {
                room.BottomRightAreaCorner = new Vector2Int(room.TopRightAreaCorner.x, room.BottomLeftAreaCorner.y);
                room.TopLeftAreaCorner = new Vector2Int(room.BottomLeftAreaCorner.x, room.TopRightAreaCorner.y);
                listToReturn.Add(room);
                continue;
            }
// And then we continue with random room generation code

            Vector2Int newBottomLeftPoint = StructureHelper.GenerateBottomLeftCornerBetween(
                space.BottomLeftAreaCorner, space.TopRightAreaCorner, roomBottomCornerModifier, roomOffset
            );
            Vector2Int newTopRightPoint = StructureHelper.GenerateTopRightCornerBetween(
                space.BottomLeftAreaCorner, space.TopRightAreaCorner, roomTopCornerModifier, roomOffset
            );
            space.BottomLeftAreaCorner = newBottomLeftPoint;
            space.TopRightAreaCorner = newTopRightPoint;
            space.BottomRightAreaCorner = new Vector2Int(newTopRightPoint.x, newBottomLeftPoint.y);
            space.TopLeftAreaCorner = new Vector2Int(newBottomLeftPoint.x, newTopRightPoint.y);

            listToReturn.Add((RoomNode)space);
        }
        //Now we go Back to Structure Helper #
        return listToReturn;
    }




}
