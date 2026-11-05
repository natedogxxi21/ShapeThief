using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CorridorsGenerator
{
    public List<Node> CreateCorridor(List<RoomNode> allNodesCollection, int corridorWidth)
    {
        
        List<Node> corridorList = new List<Node>();
        Queue<RoomNode> structuresToCheck = new Queue<RoomNode>(
             allNodesCollection.OfType<RoomNode>().OrderByDescending(node => node.TreeLayerIndex).ToList());
        //lowest indexes are gradually moved up
        while (structuresToCheck.Count > 0)
        {

          var node = structuresToCheck.Dequeue();

            if (node.ChildrenNodeList.Count == 0)
            {
                continue;

            }

    


            CorridorNode corridor = new CorridorNode(node.ChildrenNodeList[0], node.ChildrenNodeList[1], corridorWidth);
            corridorList.Add(corridor);
        }
        //Create few more additional connections between rooms
        AddExtraCorridors(corridorList, allNodesCollection, corridorWidth);

        return corridorList;
    }

    private bool OverlapsExistingCorridor(CorridorNode newCorridor, List<Node> corridorList)
    {
        //if this shit doesn't fix this damn generation... why did I pick this major...
        foreach (Node node in corridorList)
        {
            if (!(node is CorridorNode existingCorridor))
            continue;

            //W overlaps
            bool overlaps = 
                newCorridor.BottomLeftAreaCorner.x < existingCorridor.TopRightAreaCorner.x &&
                newCorridor.TopRightAreaCorner.x > existingCorridor.BottomLeftAreaCorner.x &&
                newCorridor.BottomLeftAreaCorner.y < existingCorridor.TopRightAreaCorner.y &&
                newCorridor.TopRightAreaCorner.y > existingCorridor.BottomLeftAreaCorner.y;

            if (overlaps)
                return true;
                //return true if an overlap is detected
        }

        return false;
    }

    private void AddExtraCorridors(List<Node> corridorList, List<RoomNode> allNodesCollection, int corridorWidth)
    {
        int extraCorridorsToCreate = 4;
        int maxCorridorDistance = 12;
        int corridorsCreated = 0;

        //Okay corridor seems to be glitching by the starting room let's ignore the startingroom node
        List<RoomNode> rooms = allNodesCollection.Where(node => node.ChildrenNodeList.Count == 0 && !node.isStartingRoom).ToList();

        //Need to randomize the order by which corridors are made cause they keep getting spammed in the beginning
        rooms = rooms.OrderBy(room => UnityEngine.Random.value).ToList();

        //Checking possible pairs of room to connect a corridor
        //Time to add in weird math bs from Corridor code
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                //Turning rooms into Room A and Room B to identify our potential rooms and draw connections between them.
                //RoomNodes of A and B are going to equal into the variable of i and j so that the rooms are counted into room.Count in our nodes collection
                RoomNode roomA = rooms[i];
                RoomNode roomB = rooms[j];


                //uh flip the x and y values for horizontal and vertical
                //Check if the rooms are seperated horizontally
                bool horizontal = roomA.TopRightAreaCorner.x <= roomB.BottomLeftAreaCorner.x || 
                                    roomB.TopRightAreaCorner.x <= roomA.BottomLeftAreaCorner.x;

                bool vertical = roomA.TopRightAreaCorner.y <= roomB.BottomLeftAreaCorner.y || 
                                    roomB.TopRightAreaCorner.y <= roomA.BottomLeftAreaCorner.y;

                if (!horizontal && !vertical)
                continue;

                //yeah science- i mean math

                //Connect rooms that have enough overlap with each other.
                int overlapX = Math.Min(roomA.TopRightAreaCorner.x,
                                        roomB.TopRightAreaCorner.x) 
                                - Mathf.Max(roomA.BottomLeftAreaCorner.x,
                                            roomB.BottomLeftAreaCorner.x);
                //Repeat with y;
                int overlapY = Math.Min(roomA.TopRightAreaCorner.x,
                                        roomB.TopRightAreaCorner.x) 
                                - Mathf.Max(roomA.BottomLeftAreaCorner.x,
                                            roomB.BottomLeftAreaCorner.x);


                int gap;
                //?

                if (horizontal)
                {
                    gap = Mathf.Max(roomA.BottomLeftAreaCorner.x - roomB.TopRightAreaCorner.x,
                                    roomB.BottomLeftAreaCorner.x - roomA.TopRightAreaCorner.x);
                    //add a new!! CORRIDOR YAyYYY
                    //corridorList.Add(new CorridorNode(roomA, roomB, corridorWidth));
                    //tally up and count the int Corridorscreated so that we can keep track of the int and not create too many extra corridors
                    float centerX = Mathf.Abs((roomA.BottomLeftAreaCorner.x + roomA.TopRightAreaCorner.x) / 2f -
                                                (roomB.BottomLeftAreaCorner.x + roomB.TopRightAreaCorner.x) / 2f);
                    float centerY = Mathf.Abs((roomA.BottomLeftAreaCorner.y + roomA.TopRightAreaCorner.y) / 2f -
                                                (roomB.BottomLeftAreaCorner.y + roomB.TopRightAreaCorner.y) / 2f);

                    //Prevent overlapps
                    if (overlapY < corridorWidth || gap <= 0 || gap >maxCorridorDistance || centerX <= centerY)
                        continue;
                }
                else
                {
                    //repeat the same thing with y value (vertical)
                    gap = Mathf.Max(roomA.BottomLeftAreaCorner.y - roomB.TopRightAreaCorner.y,
                                    roomB.BottomLeftAreaCorner.y - roomA.TopRightAreaCorner.y);
                    //add a new!! CORRIDOR YAyYYY
                    //corridorList.Add(new CorridorNode(roomA, roomB, corridorWidth));
                    //tally up and count the int Corridorscreated so that we can keep track of the int and not create too many extra corridors
                    float centerX = Mathf.Abs((roomA.BottomLeftAreaCorner.x + roomA.TopRightAreaCorner.x) / 2f -
                                                (roomB.BottomLeftAreaCorner.x + roomB.TopRightAreaCorner.x) / 2f);
                    float centerY = Mathf.Abs((roomA.BottomLeftAreaCorner.y + roomA.TopRightAreaCorner.y) / 2f -
                                                (roomB.BottomLeftAreaCorner.y + roomB.TopRightAreaCorner.y) / 2f);

                    //Prevent overlapps
                    if (overlapX < corridorWidth || gap <= 0 || gap >maxCorridorDistance || centerY <= centerX)
                        continue;
                }
                
                CorridorNode corridor = new CorridorNode(roomA, roomB, corridorWidth);

                //This next part will detect invalid dimensions that shouldn't exist (Ex. if TopRight x is less than BottomLeft x, then continue.)
                if (corridor.TopRightAreaCorner.x <= corridor.BottomLeftAreaCorner.x ||
                    corridor.TopRightAreaCorner.y <= corridor.BottomLeftAreaCorner.y)
                    continue;

                    //Skip corridor if it overlaps and move to the next.
                    if (OverlapsExistingCorridor(corridor, corridorList))
                    continue; 


                    corridorList.Add(corridor);
                    corridorsCreated++;
                    //corridor created yay!!



                if (corridorsCreated >= extraCorridorsToCreate)
                return;
                //Prevents extra corridors from being made
            }    
        
        }
    }
}
