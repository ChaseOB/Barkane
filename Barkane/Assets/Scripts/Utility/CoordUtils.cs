using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordUtils
{
    public static Vector3 Clamp(Vector3 v)
    {
        return new Vector3(
            Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z)
            );
    }

    public static (int, int, int) AsCoord(Vector3 v) => (Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));

    public static int DiffAxisCount((int, int, int) a, (int, int, int) b)
        => (a.Item1 != b.Item1 ? 1 : 0) + (a.Item2 != b.Item2 ? 1 : 0) + (a.Item3 != b.Item3 ? 1 : 0);

    public static (int, int, int) Average((int, int, int) a, (int, int, int) b)
        => ((a.Item1 + b.Item1) / 2, (a.Item2 + b.Item2) / 2, (a.Item3 + b.Item3) / 2);

    public static (int, int, int) FromTo((int, int, int) a, (int, int, int) b)
        => (b.Item1 - a.Item1, b.Item2 - a.Item2, b.Item3 - a.Item3);

    public static Vector3 FromToV((int, int, int) a, (int, int, int) b)
        => new Vector3(b.Item1 - a.Item1, b.Item2 - a.Item2, b.Item3 - a.Item3);

    public static Vector3 AsV((int, int, int) a)
        => new Vector3(a.Item1, a.Item2, a.Item3);

    public static bool RoundEquals(Vector3 a, Vector3 b)
    {
        return
            Mathf.RoundToInt(a.x) == Mathf.RoundToInt(b.x)
            && Mathf.RoundToInt(a.y) == Mathf.RoundToInt(b.y)
            && Mathf.RoundToInt(a.z) == Mathf.RoundToInt(b.z);
    }

    public static bool ApproxSameVector(Vector3 a, Vector3 b) {
        return(Vector3.Magnitude(a-b) < 0.0001f);
    }

}