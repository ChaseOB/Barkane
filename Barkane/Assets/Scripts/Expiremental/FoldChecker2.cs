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

public class SquareStack
{
    public Vector3Int currLocation;
    public Vector3Int targetLocation;
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



public class FoldData2
{
    public List<PaperJoint> axisJoints; //all joints along axis
    public List<SquareStack> foldObjects;
    public List<SquareStack> playerFoldObjects;
    public Vector3Int axisPosition;
    public Vector3Int axisVector;
    public int degrees;

    public FoldData2(List<PaperJoint> aj, List<SquareStack> fo, List<SquareStack> pfo, Vector3Int apos, Vector3Int avec, int deg) {
        axisJoints = aj;
        foldObjects = fo;
        playerFoldObjects = pfo; 
        axisPosition = apos;
        axisVector = avec;
        degrees = deg;
    }

}



public class FoldChecker2 : Singleton<FoldChecker2>
{
    private void Awake() {
        InitializeSingleton();
    }

    public List<SquareStack> GetFoldPosition(FoldData2 fd)
    {
        Quaternion rotation = Quaternion.Euler(fd.axisVector * fd.degrees);

        foreach(SquareStack s in fd.foldObjects)
        {
            Vector3Int target = Vector3Int.RoundToInt(rotation * (s.currLocation - fd.axisPosition) + fd.axisPosition);
            s.SetTarget(target);
            print("intial Location : " + s.currLocation  + " axis " + s.orientation + " Target Location: " + s.targetLocation + " axis " + s.targetorientation);
        }

        List<SquareStack> combined = new();
        combined.AddRange(fd.playerFoldObjects);
        combined.AddRange(fd.foldObjects);
        foreach(SquareStack s1 in combined)
        {
            
            foreach(SquareStack s2 in combined)
            {
                StackOverlapType overlap = s1.GetOverlap(s2);
                switch(overlap)
                {
                    case StackOverlapType.SAME:
                    case StackOverlapType.NONE:
                        break;
                    case StackOverlapType.BOTH:
                        break;
                    case StackOverlapType.START:
                        break;
                    case StackOverlapType.END:
                        break;
                }
            }
        }
        return combined;
    }   
}
