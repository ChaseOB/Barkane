using System.Runtime.Remoting.Messaging;
using UnityEngine;

public enum Orientation
{
    YZ,
    XZ,
    XY
}

public static class OrientationExtension
{
    public static readonly Vector3 YZ = new Vector3(0, 0, -90);
    public static readonly Vector3 XZ = new Vector3(0, 0, 0);
    public static readonly Vector3 XY = new Vector3(90, 0, 0);

    public static Vector3 GetEulerAngle(Orientation orientation)
    {
        switch (orientation)
        {
            case Orientation.YZ:
                return YZ;
            case Orientation.XZ:
                return XZ;
            case Orientation.XY:
                return XY;
        }
        return Vector3.zero;
    }

    public static Vector3 GetRotationAxis(Orientation orientation)
    {
        switch (orientation)
        {
            case Orientation.YZ:
                return new Vector3(1, 0, 0);
            case Orientation.XZ:
                return new Vector3(0, 1, 0);
            case Orientation.XY:
                return new Vector3(0, 0, 1);
        }

        return Vector3.zero;
    }

    public static Orientation GetOrientation(Vector3 eulerAngle)
    {
        if (eulerAngle == YZ)
        {
            return Orientation.YZ;
        }
        else if (eulerAngle == XY)
        {
            return Orientation.XY;
        }
        return Orientation.XZ;
    }

    public static Vector3Int[] GetTangentDirs(Orientation orient)
    {
        Vector3Int[] dirs = new Vector3Int[4];

        //4 sides based on orientation
        switch (orient)
        {
            case Orientation.XY:
                dirs = new Vector3Int[4] { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };
                break;
            case Orientation.YZ:
                dirs = new Vector3Int[4] { Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back };
                break;
            case Orientation.XZ:
                dirs = new Vector3Int[4] { Vector3Int.left, Vector3Int.forward, Vector3Int.right, Vector3Int.back };
                break;
        }

        return dirs;
    }

    public static Vector3Int GetNormalDir(Orientation orient)
    {
        Vector3Int[] dirs = new Vector3Int[2];

        switch (orient)
        {
            case Orientation.XY:
                return Vector3Int.forward;
            case Orientation.YZ:
                return Vector3Int.right;
            case Orientation.XZ:
            default:
                return Vector3Int.up;
        }
    }
}
