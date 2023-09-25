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
        DisableStackInsides();
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
                    //newStack.debug = true;
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
        // if(newStack != null)
        // {
        //     Debug.Log("made new stack with " + newStack.squarelist.Count + " squares at " + newStack.currentPosition.location + " target " + newStack.targetPosition.location);
        // }
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
    public void MergeIntoStack(SquareStack other, Vector3 foldAxis)
    {
        debug = debug || other.debug;
        if(debug)
            Debug.Log("merging stacks at " + targetPosition.location);

        Vector3 cross = Vector3.Cross(currentPosition.axis, foldAxis);
        Vector3 distance = other.currentPosition.location - currentPosition.location;
        float dot = Vector3.Dot(cross, distance);
        bool sameAxis = other.targetPosition.axis == targetPosition.axis;
        Debug.Log("dot " + dot + " same axis" + sameAxis);
        bool dotLessThanZero = dot < 0;
        while(other.squarelist.Count > 0)
        {
                    SquareData s = dotLessThanZero == sameAxis ? other.squarelist.Last(): other.squarelist.First();
                    other.squarelist.Remove(s);
                    if(dot < 0)
                        squarelist.AddFirst(s);
                    else
                        squarelist.AddLast(s);
        }

        // if(dot < 0) //merge onto the top of this stack
        // {
        //     if(other.targetPosition.axis == targetPosition.axis) //other stack in same dir, don't flip
        //     {
        //         while(other.squarelist.Count > 0)
        //         {
        //             SquareData s = other.squarelist.Last();
        //             other.squarelist.Remove(s);
        //             squarelist.AddFirst(s);
        //         }
        //     }
        //     else
        //     {
        //         while(other.squarelist.Count > 0)
        //         {
        //             SquareData s = other.squarelist.First();
        //             other.squarelist.Remove(s);
        //             squarelist.AddFirst(s);
        //         }
        //     }
        // }
        // else
        // {
        //     if(other.targetPosition.axis == targetPosition.axis) //other stack in same dir, don't flip
        //     {
        //         while(other.squarelist.Count > 0)
        //         {
        //             SquareData s = other.squarelist.First();
        //             other.squarelist.Remove(s);
        //             squarelist.AddLast(s);
        //         }
        //     }
        //     else
        //     {
        //         while(other.squarelist.Count > 0)
        //         {
        //             SquareData s = other.squarelist.Last();
        //             other.squarelist.Remove(s);
        //             squarelist.AddLast(s);
        //         }
        //     }
        // }
        // if(other.targetPosition.axis == targetPosition.axis)
        // {
        //     while(other.squarelist.Count > 0)
        //     {
        //         SquareData s = other.squarelist.First();
        //         other.squarelist.Remove(s);
        //         squarelist.AddLast(s);
        //     }
        // }
        // else if (other.targetPosition.axis == -1 * targetPosition.axis)
        // {
        //     while(other.squarelist.Count > 0)
        //     {
        //      SquareData s = other.squarelist.Last();
        //         other.squarelist.Remove(s);
        //         squarelist.AddLast(s);
        //     }
        // }
        // else {
        //     Debug.LogWarning("Axis did not match, Other:" + other.targetPosition.axis + " This:" + targetPosition.axis);
        // }
        Debug.Log("stack with " + squarelist.Count + " squares merged at " + targetPosition.location);
    }

    public void DisableStackInsides()
    {
        if(squarelist.Count == 0) return;

        if(squarelist.Count == 1) 
        {
            PaperSquare s = squarelist.First().paperSquare;
            s.ToggleTop(true);
            s.ToggleBottom(true);
            return;
        }

        SquareData top = squarelist.First();
        bool topSame = top.targetPosition.axis == this.targetPosition.axis;
        top.paperSquare.ToggleTop(!topSame);
        top.paperSquare.ToggleBottom(topSame);

        SquareData bottom = squarelist.Last();
        bool botSame = bottom.targetPosition.axis == this.targetPosition.axis;
        bottom.paperSquare.ToggleBottom(!botSame);
        bottom.paperSquare.ToggleTop(botSame);

        foreach(SquareData s in squarelist)
        {
            if (s != top && s != bottom)
            {
                s.paperSquare.ToggleTop(false);
                s.paperSquare.ToggleBottom(false);
            }
        }
    }

    public void UpdateYOffsets()
    {
        if(debug)
            Debug.Log("updating y offsets for " + squarelist.Count + " squares at " + targetPosition.location);

        if(squarelist.Count < 1) return;
        if(squarelist.Count == 1)
        {
            squarelist.First().SetTargetYOffset(0);
            return;
        }
        float MAX_DISPLACEMENT = 0.1f;

        //If this is +y, we want the top of the stack to have a y-offset of 0
        //If this is a -y, we want the bottom of the stack to have a y-offset of 0
        //so marm can walk on them
        //else, use the full range
        float maxOffset = targetPosition.axis == Vector3.up ? 0 : MAX_DISPLACEMENT;
        float minOffset = targetPosition.axis == Vector3.down ? 0 : -1 * MAX_DISPLACEMENT;
        float diff = maxOffset - minOffset;

        Debug.Log("axis " + targetPosition.axis + " max " + maxOffset + " min  " + minOffset);
        
        for(int i = squarelist.Count - 1; i >= 0 ; i--)
        {
            SquareData s = squarelist.ElementAt(i);
            bool sameAxis = s.targetPosition.axis == targetPosition.axis;
            float target = (diff * i / (squarelist.Count - 1.0f) + minOffset) * (sameAxis ? 1 : -1);
            s.SetTargetYOffset(target);
        }
    }
}
