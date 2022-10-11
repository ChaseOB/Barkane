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
}
