using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    RoomNode rootNode;
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
    public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset, int corridorWidth)
    {
        
            BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength); 
            allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
            List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeafes(bsp.RootNode); 
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
