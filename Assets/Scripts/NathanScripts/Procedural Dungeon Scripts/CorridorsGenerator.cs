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
             allNodesCollection.OrderByDescending(node => node.TreeLayerIndex).ToList());
        //lowest indexes are gradually moved up
        while (structuresToCheck.Count > 0)
        {

          var node = structuresToCheck.Dequeue();
            if (node.ChildrenNodeList.Count == 0)
            {
                continue;

            }

            Debug.Log(
                $"Creating corridor for parent layer {node.TreeLayerIndex}: " +
                $"{node.ChildrenNodeList[0].BottomLeftAreaCorner} to " +
                $"{node.ChildrenNodeList[0].TopRightAreaCorner} | " +
                $"{node.ChildrenNodeList[1].BottomLeftAreaCorner} to " +
                $"{node.ChildrenNodeList[1].TopRightAreaCorner}"
            );

            CorridorNode corridor = new CorridorNode(node.ChildrenNodeList[0], node.ChildrenNodeList[1], corridorWidth);
            corridorList.Add(corridor);
        }
        return corridorList;
    }
}
