using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

public enum FoldObjectType 
{
    NONE,
    OBJECT, 
    SQUARE,
    JOINT,
    SQUARESTACK,
    JOINTSTACK,
}

public class FoldObjectData
{
    FoldObjectType type;
    Vector3Int coordinates;
}

public enum StackOverlapType
{
    SAME,
    NONE,
    START,
    END,
    BOTH,
}

// public enum Axis 
// {
//     X,
//     NEGX,
//     Y,
//     NEGY,
//     Z,
//     NEGZ,
// }

public abstract class FoldableObject
{
    public Vector3Int currLocation;
    public Vector3Int targetLocation;
    public Vector3 orientation;
    public Vector3 targetorientation;

    public abstract void SendToTarget(Vector3 axis);
    

}

public class JointData: FoldableObject
{
    public PaperJoint paperJoint;

    public JointData(PaperJoint pj)
    {
        paperJoint = pj;
        currLocation = Vector3Int.RoundToInt(pj.transform.position);
        targetLocation = Vector3Int.RoundToInt(pj.transform.position);
    }

    public override void SendToTarget(Vector3 axis)
    {
        currLocation = targetLocation;
        orientation = targetorientation;
        paperJoint.transform.position = targetLocation;
        //paperJoint.transform.Rotate(axis, 90);
    }

    public void SetCurrent(Vector3Int v)
    {
        currLocation = v;
        orientation = GetAxisFromCoordinates(v);
    }

    public Vector3 GetAxisFromCoordinates(Vector3Int coordinates)
    {
        if(coordinates.x % 2 == 0) return Vector3.right;
        if(coordinates.y % 2 == 0) return Vector3.up;
        return Vector3.forward;

    }
}


public class SquareStack : FoldableObject
{
    
    public LinkedList<PaperSquare> squares = new();

    public SquareStack(Vector3Int location)
    {
        SetCurrent(location);
        targetLocation = location;
        targetorientation = orientation;
        //SetTarget(location);
    }

    // public Vector3 AxisVector {
    //     get 
    //     {
    //         if(orientation == Axis.X) return Vector3.right;
    //         if(orientation == Axis.Y) return Vector3.up;
    //         return Vector3.forward;
    //     }
    // }

    public Quaternion IndicatorRotation(Vector3 a) {
            if(a == Vector3.right || a == Vector3.left) return Quaternion.Euler(0, 0, 90);
            if(a == Vector3.up || a == Vector3.down) return quaternion.identity; 
            return Quaternion.Euler(90, 0, 0);
    }

    public void SetCurrent(Vector3Int v)
    {
        currLocation = v;
        orientation = GetAxisFromCoordinates(v);
    }


    // public void SetTargetAsCurrent()
    // {
    //     SetTarget(currLocation);
    // }
    
    public void SetTarget(Vector3Int target, Vector3 axis)
    {
        targetLocation = target;
        targetorientation = GetTargetOrientation(axis);
    }

    public Vector3 GetTargetOrientation(Vector3 axis)
    {
        Vector3 result = Vector3.Cross(axis, orientation);
        return result.magnitude == 0 ? orientation : result;
    }
    
    public Vector3 GetAxisFromCoordinates(Vector3Int coordinates)
    {
        if(coordinates.x % 2 != 0) return Vector3.right;
        if(coordinates.y % 2 != 0) return Vector3.up;
        return Vector3.forward;

    }

    public StackOverlapType GetOverlap(SquareStack other)
    {
        if(other == this) return StackOverlapType.SAME;
        bool sameStart = currLocation == other.currLocation;
        bool sameEnd = targetLocation == other.targetLocation;
        if(sameStart && sameEnd) return StackOverlapType.BOTH;
        if(sameEnd) return StackOverlapType.END; //Merge stacks at end of fold
        if(sameStart) return StackOverlapType.START; //Split stacks at start of fold
        return StackOverlapType.NONE;
    }

    public override void SendToTarget(Vector3 axis)
    {
        
        foreach(PaperSquare s in squares)
        {
            if(targetLocation != currLocation)
            {
            s.transform.position = targetLocation;
            s.transform.Rotate(axis, 90);
            }
        }   
        currLocation = targetLocation;
        orientation = targetorientation;

    }

    private Vector3 GetRotationFromAxis(Vector3 axis)
    {
        if(axis == Vector3.right) return new Vector3(0, 0, 90);
        if(axis == Vector3.left) return new Vector3(0, 0, -90);
        if(axis == Vector3.down) return new Vector3(0, 0, -90);
        if(axis == Vector3.forward) return new Vector3(-90, 0, 0);
        if(axis == Vector3.back) return new Vector3(90, 0, 0);
        return Vector3.zero;
    }
}
