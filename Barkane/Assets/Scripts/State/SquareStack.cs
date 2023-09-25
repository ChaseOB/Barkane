using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

public class SquareStack : FoldableObject
{
    public LinkedList<SquareData> squarelist = new();
    public bool IsEmpty => squarelist.Count == 0;

    public bool debug;

    public SquareStack(PaperSquare paperSquare)
    {
        PositionData positionData = new(
            Vector3Int.RoundToInt(paperSquare.transform.position),
            paperSquare.transform.rotation,
            GetAxisFromCoordinates( Vector3Int.RoundToInt(paperSquare.transform.position))
        );

        SquareData squareData = new(positionData, paperSquare);
        squarelist.AddFirst(squareData);
        currentPosition = positionData;
        targetPosition = positionData;
    }

    public SquareStack(SquareData s)
    {
        squarelist.AddFirst(s);
        currentPosition = new(s.currentPosition);
        targetPosition = new(s.targetPosition);
    }

    public static Vector3 GetAxisFromCoordinates(Vector3Int coordinates)
    {
        if(coordinates.x % 2 != 0) return Vector3.right;
        if(coordinates.y % 2 != 0) return Vector3.up;
        return Vector3.forward;

    }

    public override void SendToTarget()
    {
        //targetPosition = currentPosition;
        //When sendToTarget is called, all squares should have the same position
        currentPosition = targetPosition;
        //targetPosition = new(squarelist.First().targetPosition);
        foreach(SquareData sd in squarelist)
        {
            sd.SendToTarget();
        }  
    }

    

    //if elements of this stack should be in another stack, split them out into a new stack
    public SquareStack SplitStack()
    {
        SquareStack newStack = null;
        List<SquareData> remove = new();
        foreach(SquareData squareData in squarelist)
        {
            if(!squareData.IsInTargetPos())
            {
                if(newStack == null)
                {
                    newStack = new(squareData);
                    newStack.debug = true;
                }
                else
                {
                    newStack.squarelist.AddLast(squareData);
                }
                remove.Add(squareData);
                //Debug.Log(squareData.paperSquare.gameObject.name + " at " + squareData.currentPosition.location + " target " + squareData.targetPosition.location);
            }
        }
        foreach(SquareData s in remove)
        {
            squarelist.Remove(s);
        }
        if(newStack != null)
        {
            Debug.Log("made new stack with " + newStack.squarelist.Count + " squares at " + newStack.currentPosition.location + " target " + newStack.targetPosition.location);
        }
        return newStack;
    }

    public override void SetParent(Transform parent)
    {
        throw new System.NotImplementedException();
    }

    public StackOverlapType GetOverlap(SquareStack other)
    {
        if(other == this) return StackOverlapType.SAME;
        bool sameStart = currentPosition.Equals(other.currentPosition);
        bool sameEnd = targetPosition.Equals(other.targetPosition);
        if(sameStart && sameEnd) return StackOverlapType.BOTH;
        if(sameEnd) return StackOverlapType.END; //Merge stacks at end of fold
        if(sameStart) return StackOverlapType.START; //Split stacks at start of fold
        return StackOverlapType.NONE;
    }

    //merge other into this stack
    public void MergeIntoStack(SquareStack other)
    {
        debug = debug || other.debug;
        if(debug)
            Debug.Log("merging stacks at " + targetPosition.location);
        //TODO: how tf do you merge :sob:
        if(other.targetPosition.axis == targetPosition.axis)
        {
            while(other.squarelist.Count > 0)
            {
                SquareData s = other.squarelist.First();
                other.squarelist.Remove(s);
                squarelist.AddLast(s);
            }
        }
        else if (other.targetPosition.axis == -1 * targetPosition.axis)
        {
            while(other.squarelist.Count > 0)
            {
             SquareData s = other.squarelist.Last();
                other.squarelist.Remove(s);
                squarelist.AddLast(s);
            }
        }
        else {
            Debug.LogWarning("Axis did not match, Other:" + other.targetPosition.axis + " This:" + targetPosition.axis);
        }
        Debug.Log("stack with " + squarelist.Count + " squares merged at " + targetPosition.location);
    }

    public void UpdateYOffsets()
    {
        if(debug)
            Debug.Log("updating y offsets for " + squarelist.Count + " squares at " + targetPosition.location);
        float maxOffset = 0.1f;
        
        float inc = (squarelist.Count - 1) > 0 ? maxOffset / (squarelist.Count - 1) : 0;
        for(int i = 0; i < squarelist.Count; i++)
        {
            SquareData s = squarelist.ElementAt(i);
            s.SetTargetYOffset(i * inc);
        }
    }
}
