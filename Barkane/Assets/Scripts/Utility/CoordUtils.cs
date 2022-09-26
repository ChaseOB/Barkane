using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordUtils
{
    public static bool Coplanar((int, int, int) a, (int, int, int) b)
    {
        var diffAxisCount = (a.Item1 == b.Item1 ? 1 : 0) + (a.Item2 == b.Item2 ? 1 : 0) + (a.Item3 == b.Item3 ? 1 : 0);
        return diffAxisCount <= 1;
    }

    public static (int, int, int) Average((int, int, int) a, (int, int, int) b)
        => ((a.Item1 + b.Item1) / 2, (a.Item2 + b.Item2) / 2, (a.Item3 + b.Item3) / 2);

    public static (int, int, int) FromTo((int, int, int) a, (int, int, int) b)
        => (b.Item1 - a.Item1, b.Item2 - a.Item2, b.Item3 - a.Item3);

    public static Vector3 FromToV((int, int, int) a, (int, int, int) b)
        => new Vector3(b.Item1 - a.Item1, b.Item2 - a.Item2, b.Item3 - a.Item3);

    public static Vector3 AsV((int, int, int) a)
        => new Vector3(a.Item1, a.Item2, a.Item3);

}