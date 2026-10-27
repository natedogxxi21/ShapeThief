using System.Collections.Generic;

using UnityEngine;
using System.Linq;

public class DungeonGenerator 
{
    //RoomNode rootNode;
    List<RoomNode> allNodesCollection = new List<RoomNode>();
    private int dungeonWidth;
    private int dungeonLength;

  

public DungeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;

    }

        //Represents our tree like structure in the Binary Space Partitioner
        //Line 23, "CalculateRooms" changed to => "CalculateDungeon" remember for changes to all refrences
        //Updating to add startingRoomWidth and startingRoomLength
    public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset, int corridorWidth, int startingRoomWidth, int startingRoomLength)
    {
        
            BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength); 
            allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin, startingRoomWidth, startingRoomLength);
            //Now that its updated, we need to update DungeonCreator
            List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeafes(bsp.RootNode);

            foreach (Node node in roomSpaces)
            {
                if (node is RoomNode room)
                {
                    Debug.Log(
                        $"Room bounds: {room.BottomLeftAreaCorner} to " + $"{room.TopRightAreaCorner}, Starting: {room.isStartingRoom}"
                    );
                }
            }
            //Now we need to create rooms in those room spaces
            //We are going to create a new class called Room generator
            RoomGenerator roomGenerator = new RoomGenerator (maxIterations, roomLengthMin, roomWidthMin);         
            List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBottomCornerModifier,  roomTopCornerModifier,  roomOffset);

            //Creating code for creating the connecting Corridors that will be connecting our rooms together.
            CorridorsGenerator corridorGenerator = new CorridorsGenerator();
            var corridorList = corridorGenerator.CreateCorridor(allNodesCollection, corridorWidth);
            
            return new List<Node>(roomList).Concat(corridorList).ToList();
            //We can pass our room list through line 32 change "allSpaceNodes" to => "roomList"

    }       
    
}
