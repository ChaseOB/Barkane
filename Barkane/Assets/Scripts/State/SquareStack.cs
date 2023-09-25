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


// public class JointData: FoldableObject
// {
//     public PaperJoint paperJoint;

//     public JointData(PaperJoint pj)
//     {
//         paperJoint = pj;
//         currLocation = Vector3Int.RoundToInt(pj.transform.position);
//         targetLocation = Vector3Int.RoundToInt(pj.transform.position);
//     }

//     public override void SendToTarget(Vector3 axis)
//     {
//         if(currLocation != targetLocation)
//         {
//             paperJoint.transform.position = targetLocation;
//             paperJoint.transform.Rotate(axis, 90, Space.World);
// //            Debug.Log("joint " + paperJoint.gameObject.name + " moved " + axis);
//         }
//         currLocation = targetLocation;
//         orientation = targetorientation;
//     }

//     public void SetCurrent(Vector3Int v)
//     {
//         currLocation = v;
//         orientation = GetAxisFromCoordinates(v);
//     }

//     public static Vector3 GetAxisFromCoordinates(Vector3Int coordinates)
//     {
//         if(coordinates.x % 2 == 0) return Vector3.right;
//         if(coordinates.y % 2 == 0) return Vector3.up;
//         return Vector3.forward;

//     }
// }


// public class SquareStack : FoldableObject
// {
    
//     public LinkedList<PaperSquare> squares = new();

//     public SquareStack(Vector3Int location)
//     {
//         SetCurrent(location);
//         targetLocation = location;
//         targetorientation = orientation;
//         //SetTarget(location);
//     }

//     // public Vector3 AxisVector {
//     //     get 
//     //     {
//     //         if(orientation == Axis.X) return Vector3.right;
//     //         if(orientation == Axis.Y) return Vector3.up;
//     //         return Vector3.forward;
//     //     }
//     // }

//     public Quaternion IndicatorRotation(Vector3 a) {
//             if(a == Vector3.right || a == Vector3.left) return Quaternion.Euler(0, 0, 90);
//             if(a == Vector3.up || a == Vector3.down) return quaternion.identity; 
//             return Quaternion.Euler(90, 0, 0);
//     }

//     public void SetCurrent(Vector3Int v)
//     {
//         currLocation = v;
//         orientation = GetAxisFromCoordinates(v);
//     }


//     // public void SetTargetAsCurrent()
//     // {
//     //     SetTarget(currLocation);
//     // }
    
//     public void SetTarget(Vector3Int target, Vector3 axis)
//     {
//         targetLocation = target;
//         targetorientation = GetTargetOrientation(axis);
//     }

//     public Vector3 GetTargetOrientation(Vector3 axis)
//     {
//         Vector3 result = Vector3.Cross(axis, orientation);
//         return result.magnitude == 0 ? orientation : result;
//     }
    
//     public Vector3 GetAxisFromCoordinates(Vector3Int coordinates)
//     {
//         if(coordinates.x % 2 != 0) return Vector3.right;
//         if(coordinates.y % 2 != 0) return Vector3.up;
//         return Vector3.forward;

//     }

//     public StackOverlapType GetOverlap(SquareStack other)
//     {
//         if(other == this) return StackOverlapType.SAME;
//         bool sameStart = currLocation == other.currLocation;
//         bool sameEnd = targetLocation == other.targetLocation;
//         if(sameStart && sameEnd) return StackOverlapType.BOTH;
//         if(sameEnd) return StackOverlapType.END; //Merge stacks at end of fold
//         if(sameStart) return StackOverlapType.START; //Split stacks at start of fold
//         return StackOverlapType.NONE;
//     }

//     public override void SendToTarget(Vector3 axis)
//     {
        
//         foreach(PaperSquare s in squares)
//         {
//             if(targetLocation != currLocation)
//             {
//             s.transform.position = targetLocation;
//             s.transform.Rotate(axis, 90);
//             }
//         }   
//         currLocation = targetLocation;
//         orientation = targetorientation;

//     }

//     private Vector3 GetRotationFromAxis(Vector3 axis)
//     {
//         if(axis == Vector3.right) return new Vector3(0, 0, 90);
//         if(axis == Vector3.left) return new Vector3(0, 0, -90);
//         if(axis == Vector3.down) return new Vector3(0, 0, -90);
//         if(axis == Vector3.forward) return new Vector3(-90, 0, 0);
//         if(axis == Vector3.back) return new Vector3(90, 0, 0);
//         return Vector3.zero;
//     }
// }
