using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

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

public enum Axis 
{
    X,
    Y,
    Z,
}

public class FoldableObject
{
    public Vector3Int currLocation;
    public Vector3Int targetLocation;
}

public class JointData: FoldableObject
{
    public PaperJoint paperJoint;

    public JointData(PaperJoint pj)
    {
        paperJoint = pj;
        currLocation = Vector3Int.RoundToInt(pj.transform.position);
    }
}


public class SquareStack : FoldableObject
{
    public Axis orientation = Axis.X;
    public Axis targetorientation = Axis.X;
    public LinkedList<PaperSquare> squares = new();

    public SquareStack(Vector3Int location)
    {
        SetCurrent(location);
        SetTarget(location);
    }

    public Vector3 AxisVector {
        get 
        {
            if(orientation == Axis.X) return Vector3.right;
            if(orientation == Axis.Y) return Vector3.up;
            return Vector3.forward;
        }
    }

    public Quaternion IndicatorRotation(Axis a) {
            if(a == Axis.X) return Quaternion.Euler(0, 0, 90);
            if(a == Axis.Y) return quaternion.identity;
            return Quaternion.Euler(90, 0, 0);
    }

    public void SetCurrent(Vector3Int v)
    {
        currLocation = v;
        orientation = GetAxisFromCoordinates(v);
    }

    public void SetTargetAsCurrent()
    {
        SetTarget(currLocation);
    }
    
    public void SetTarget(Vector3Int v)
    {
        targetLocation = v;
        targetorientation = GetAxisFromCoordinates(v);
    }

    public Axis GetAxisFromCoordinates(Vector3Int coordinates)
    {
        if(coordinates.x % 2 != 0) return Axis.X;
        if(coordinates.y % 2 != 0) return Axis.Y;
        return Axis.Z;

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
}
