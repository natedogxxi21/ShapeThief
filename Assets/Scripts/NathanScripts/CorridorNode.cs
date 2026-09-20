using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using UnityEngine;

public class CorridorNode : Node
{
    private Node structure1;
    //changes "node1" to => "structure1"
    private Node structure2;
    //changes "node2" to => "structure2"
    private int corridorWidth;
    private int modifierDistanceFromWall;

    public CorridorNode (Node node1, Node node2, int corridorWidth) : base(null)
    {
        this.structure1 = node1;
        this.structure2 = node2;
        this.corridorWidth = corridorWidth;
        GenerateCorridor();

    }

    private void GenerateCorridor()
    {
        //Where is this node placed, var... is going to check whihc position the corridor will be. Above? Below? Left? Right?
        //We'll check structure 2 releavant to structure 1
        //Position in relation to, we are going to use a coordinate system. We are going to get the first object as if it was in the center of the coordinate system
        //We will use math to calculate the angle in between the 2 structures.
        var relativePositionOfStructure2 = CheckPositionStructure2AgainstStructure1();
       //switch position relevant of structure then case create position of corridor
        switch (relativePositionOfStructure2)
        {
            case RelativePosition.Up:
                ProcessRoomInRelationUpOrDown(this.structure1, this.structure2);
            break;
            case RelativePosition.Down:
                ProcessRoomInRelationUpOrDown(this.structure2, this.structure1);
            break;
            case RelativePosition.Right:
            ProcessRoomInRelationRightOrLeft(this.structure1, this.structure2);
            break;
            case RelativePosition.Left:
            ProcessRoomInRelationRightOrLeft(this.structure1, this.structure2);
            break;

            default:
            break;
        }
    }

    //Create void for ProccessRoomInRelation Up or Down & Right or Left
    private void ProcessRoomInRelationRightOrLeft(Node structure1, Node structure2)
    {
       Node leftStructure = null;
       List<Node> leftStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure1);
        Node rightStructure = null;
        List<Node> rightStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure2);    
    
        var sortedLeftStructure = leftStructureChildren.OrderByDescending(child => child.TopRightAreaCorner.x).ToList();
        if(sortedLeftStructure.Count == 1)
        {
            leftStructure = sortedLeftStructure[0];
        }
        else
        {
            int maxX = sortedLeftStructure[0].TopRightAreaCorner.x;
            sortedLeftStructure = sortedLeftStructure.Where(children => Math.Abs(maxX - children.TopRightAreaCorner.x) < 10).ToList();
            int index = UnityEngine.Random.Range(0, sortedLeftStructure.Count);
            leftStructure = sortedLeftStructure[index];
        }
        //so tired
        //What this slide goes over basically is connecting valid rooms
        //Basically it takes the x values of the rooms and determines which x values of the rooms are most closesst together
        //Determining that the x values are the closest of the rooms, it will create a corridor there.
   
        var possibleNeighborsInRightStructureList = rightStructureChildren.Where(
        child => GetValidForNeighbourLeftRight(
            leftStructure.TopRightAreaCorner,
            leftStructure.BottomRightAreaCorner,
            child.TopLeftAreaCorner,
            child.BottomLeftAreaCorner
        )   != -1
        ).OrderBy(child => child.BottomRightAreaCorner.x).ToList();
        if(possibleNeighborsInRightStructureList.Count <= 0)
        {
            rightStructure = structure2;

        }
        else
        {
            rightStructure = possibleNeighborsInRightStructureList[0];
        }
        int y = GetValidForNeighbourLeftRight(leftStructure.TopLeftAreaCorner, leftStructure.BottomRightAreaCorner,
        rightStructure.TopLeftAreaCorner,
        rightStructure.BottomLeftAreaCorner);
        while(y== -1 && sortedLeftStructure.Count > 0)
        {
            sortedLeftStructure = sortedLeftStructure.Where(
                child => child.TopLeftAreaCorner.y != leftStructure.TopLeftAreaCorner.y).ToList();
            leftStructure = sortedLeftStructure[0];

            y = GetValidForNeighbourLeftRight(leftStructure.TopLeftAreaCorner, leftStructure.BottomRightAreaCorner,
        rightStructure.TopLeftAreaCorner,
        rightStructure.BottomLeftAreaCorner);
        }
        BottomLeftAreaCorner = new Vector2Int(leftStructure.BottomRightAreaCorner.x, y);
        TopRightAreaCorner = new Vector2Int(rightStructure.TopLeftAreaCorner.x, y + this.corridorWidth);

    }


    private int GetValidForNeighbourLeftRight(Vector2Int leftNodeUp, Vector2Int leftNodeDown, Vector2Int rightNodeUp, Vector2Int rightNodeDown)
    {
        //detects location of two rooms if one room is higher or lower than the second room
        //detecting y position on grid, so it knows where to place the corridor between rooms
        if(rightNodeUp.y >= leftNodeUp.y && leftNodeDown.y >= rightNodeDown.y)
        {
            return StructureHelper.CalculateMiddlePoint(
                leftNodeDown+ new Vector2Int(0, modifierDistanceFromWall),
                leftNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
            ).y;
        }
        if(rightNodeUp.y <= leftNodeUp.y && leftNodeDown.y <=rightNodeDown.y)
        {
             return StructureHelper.CalculateMiddlePoint(
                    rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                    rightNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
            ).y;
        }
        if(leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y)
        {
            return StructureHelper.CalculateMiddlePoint(
                rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                leftNodeUp - new Vector2Int(0, modifierDistanceFromWall)
            ).y;
        }
        if(leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y)
        {
            return StructureHelper.CalculateMiddlePoint(
             leftNodeDown + new Vector2Int(0, modifierDistanceFromWall), 
             rightNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)  
            ).y;
        }
        return -1;
    }

    private void ProcessRoomInRelationUpOrDown(Node structure1, Node structure2)
    {
        Node bottomStructure = null;
        List<Node> structureBottomChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure1);
        Node topStructure = null;
        List<Node> structureAboveChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure2);

        var sortedBottomStructure = structureBottomChildren.OrderByDescending(child => child.TopRightAreaCorner.y).ToList();

        if (sortedBottomStructure.Count == 1)
        {
            bottomStructure = structureBottomChildren[0];
        }
        else
        {
            int maxY = sortedBottomStructure[0].TopLeftAreaCorner.y;
            sortedBottomStructure = sortedBottomStructure.Where(child => MathF.Abs(maxY-child.TopLeftAreaCorner.y) < 10).ToList();
            int index = UnityEngine.Random.Range(0, sortedBottomStructure.Count);
            bottomStructure = sortedBottomStructure[index];

        }

        var possibleNeighborsInTopStructure = structureAboveChildren.Where(

            child => GetValidXForNeighborUpDown(
              bottomStructure.TopLeftAreaCorner,
              bottomStructure.TopRightAreaCorner,
              child.BottomLeftAreaCorner,
              child.BottomRightAreaCorner)
              != -1).OrderBy(child => child.BottomRightAreaCorner.y).ToList();
            if (possibleNeighborsInTopStructure.Count == 0)
        {
            topStructure = structure2;
        }
        else
        {
            topStructure = possibleNeighborsInTopStructure[0];
        }
        //??
        int x = GetValidXForNeighborUpDown(
              bottomStructure.TopLeftAreaCorner,
              bottomStructure.TopRightAreaCorner,
              topStructure.BottomLeftAreaCorner,
              topStructure.BottomRightAreaCorner);
      while(x== -1 && sortedBottomStructure.Count > 1)
        {
            sortedBottomStructure = sortedBottomStructure.Where(child => child.TopLeftAreaCorner.x!=topStructure.TopLeftAreaCorner.x).ToList();
            bottomStructure = sortedBottomStructure[0];
             x = GetValidXForNeighborUpDown(
              bottomStructure.TopLeftAreaCorner,
              bottomStructure.TopRightAreaCorner,
              topStructure.BottomLeftAreaCorner,
              topStructure.BottomRightAreaCorner);
        }
        BottomLeftAreaCorner = new Vector2Int(x, bottomStructure.TopLeftAreaCorner.y);
        TopRightAreaCorner = new Vector2Int(x + this.corridorWidth, topStructure.BottomLeftAreaCorner.y);   
        
        }

    private int GetValidXForNeighborUpDown (Vector2Int bottomNodeLeft, Vector2Int bottomNodeRight, Vector2Int topNodeLeft, Vector2Int topNodeRight)
    {
        if(topNodeLeft.x < bottomNodeLeft.x && bottomNodeRight.x < topNodeRight.x)
        {
            return StructureHelper.CalculateMiddlePoint(
                bottomNodeLeft + new Vector2Int(modifierDistanceFromWall,0),
                bottomNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0) 
            ).x;
        }
        if(topNodeLeft.x >= bottomNodeLeft.x && bottomNodeRight.x >= topNodeRight.x)
        {
            return StructureHelper.CalculateMiddlePoint(
                topNodeLeft + new Vector2Int(modifierDistanceFromWall,0),
                topNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall,0)
            ).x;
        }
        if(bottomNodeLeft.x >= topNodeLeft.x && bottomNodeLeft.x <= topNodeRight.x)
        {
             return StructureHelper.CalculateMiddlePoint(
                bottomNodeLeft + new Vector2Int(modifierDistanceFromWall,0 ),
                topNodeRight - new Vector2Int(this.corridorWidth,0)
            ).x;
        }
        if(bottomNodeRight.x <= topNodeRight.x && bottomNodeRight.x >= topNodeLeft.x)
        {
             return StructureHelper.CalculateMiddlePoint(
               topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0 ),
               bottomNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)  
            ).x;
        }
        return -1;
    }

    private RelativePosition CheckPositionStructure2AgainstStructure1()
    {
        //All the children nodes are in the space that those corners are describing
        Vector2 middlePointStructure1Temp = ((Vector2)structure1.TopRightAreaCorner + structure1.BottomLeftAreaCorner) / 2;
        Vector2 middlePointStructure2Temp = ((Vector2)structure2.TopRightAreaCorner + structure2.BottomLeftAreaCorner)  / 2;
        float angle = CalculateAngle(middlePointStructure1Temp, middlePointStructure2Temp);
        if((angle < 45 && angle >=0) || (angle>-45 && angle <0))
        {
            return RelativePosition.Right;
        }
        else if(angle > 45 && angle < 135)
        {
            return RelativePosition.Up;
        }
        else if(angle > -135 && angle < -45)
        {
            return RelativePosition.Down;
        }
        else
        {
            return RelativePosition.Left;
        }
    }

    //fucking withcraft what
    private float CalculateAngle(Vector2 middlePointStructure1Temp, Vector2 middlePointStructure2Temp)
    {
        //Yeah math
        return Mathf.Atan2(middlePointStructure2Temp.y - middlePointStructure1Temp.y,
        middlePointStructure2Temp.x - middlePointStructure1Temp.x)*Mathf.Rad2Deg;
    }



}
