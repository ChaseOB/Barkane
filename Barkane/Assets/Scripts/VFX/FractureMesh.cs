using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FractureMesh
{

    private readonly static Vector2 TL = new Vector2(0, 0);
    private readonly static Vector2 TR = new Vector2(1, 0);
    private readonly static Vector2 BL = new Vector2(0, 1);
    private readonly static Vector2 BR = new Vector2(1, 1);

    private readonly static int END_OF_TILE_CORNERS = 3;

    /// <summary>
    /// basic fractured mesh
    /// </summary>
    public static Mesh Create(FractureMeshSetting setting)
    {
        var (pivTop, pivLeft, pivOther) = GetDistinctUV(setting.margin, setting.triangleArea);

        var v2D = new Vector2[]
        {
            TL,
            TR,
            BL,
            BR,
            pivTop,
            pivLeft,
            pivOther
        };

        var vSrc = v2D.Select((v2d, i) => new Vector3
        (
            v2d.x - 0.5f,
            i > END_OF_TILE_CORNERS ? Random.value * setting.height : 0,
            v2d.y - 0.5f
        )).ToArray();

        var vDup = new Vector3[]
        {
            vSrc[0], vSrc[1], vSrc[4],
            vSrc[0], vSrc[5], vSrc[2],
            vSrc[0], vSrc[4], vSrc[5],
            vSrc[4], vSrc[6], vSrc[5],
            vSrc[2], vSrc[5], vSrc[6],
            vSrc[1], vSrc[3], vSrc[6],
            vSrc[1], vSrc[6], vSrc[4],
            vSrc[6], vSrc[3], vSrc[2]
        };

        // see paper shadergraph
        var nSrc = new Vector3[]
        {
            GetNorm(in vSrc, 0, 1, 4),
            GetNorm(in vSrc, 0, 5, 2),
            GetNorm(in vSrc, 0, 4, 5),
            GetNorm(in vSrc, 4, 6, 5),
            GetNorm(in vSrc, 2, 5, 6),
            GetNorm(in vSrc, 1, 3, 6),
            GetNorm(in vSrc, 1, 6, 4),
            GetNorm(in vSrc, 6, 3, 2)
        };

        var m = new Mesh
        {
            vertices = vDup,

            // see paper shadergraph
            normals = new Vector3[]
            {
                nSrc[0], nSrc[1], nSrc[4],
                nSrc[0], nSrc[5], nSrc[2],
                nSrc[0], nSrc[4], nSrc[5],
                nSrc[4], nSrc[6], nSrc[5],
                nSrc[2], nSrc[5], nSrc[6],
                nSrc[1], nSrc[3], nSrc[6],
                nSrc[1], nSrc[6], nSrc[4],
                nSrc[6], nSrc[3], nSrc[2]
            },

            // 8 faces, 16 triangles for double sided
            triangles = new int[]
            {
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
                9, 10, 11,
                12, 13, 14,
                15, 16, 17,
                18, 19, 20,
                21, 22, 23,

                0, 2, 1,
                3, 5, 4,
                6, 8, 7,
                9, 11, 10,
                12, 14, 13,
                15, 17, 16,
                18, 20, 19,
                21, 23, 22
            },

            uv = new Vector2[]
            {
                v2D[0], v2D[1], v2D[4],
                v2D[0], v2D[5], v2D[2],
                v2D[0], v2D[4], v2D[5],
                v2D[4], v2D[6], v2D[5],
                v2D[2], v2D[5], v2D[6],
                v2D[1], v2D[3], v2D[6],
                v2D[1], v2D[6], v2D[4],
                v2D[6], v2D[3], v2D[2]
            }
        };

        // m.RecalculateBounds();
        // m.RecalculateTangents();
        // m.Optimize();

        return m;
    }

    private static Vector3 GetNorm(in Vector3[] src, int a, int b, int c)
    {
        Vector3 dir = Vector3.Cross(src[b] - src[a], src[c] - src[a]);
        return (dir.y > 0 ? dir : -dir).normalized;
    }

    private static Vector2 GetCentroid(in Vector3[] src, int a, int b, int c)
    {
        return new Vector2(
            (src[a].x + src[b].x + src[c].x) / 3f,
            (src[a].z + src[b].z + src[c].z) / 3f
            );
    }

    /// <summary>
    /// return three distinct "well-centered" points in a 01 (uv) square
    /// </summary>
    /// <returns>top, left, right-bottom</returns>
    private static (Vector2, Vector2, Vector2) GetDistinctUV(float boundary, float triArea)
    {
        var randoms = new List<Vector2>(3);

        while (randoms.Count < 3)
        {
            var curr = new Vector2(
                Random.value * (1 - 2 * boundary) + boundary,
                Random.value * (1 - 2 * boundary) + boundary);

            var legal = true;
            foreach (var i in randoms)
            {
                if (Mathf.Abs(curr.x - i.x) < boundary || Mathf.Abs(curr.y - i.y) < boundary)
                {
                    legal = false;
                }
            }

            if (legal) randoms.Add(curr);
        }

        // nuke if triangle too narrow/small
        var area = .5f * ((randoms[1].x - randoms[0].x) * (randoms[2].y - randoms[0].y) - (randoms[2].x - randoms[0].x) * (randoms[1].y - randoms[0].y));
        if (area < triArea) return GetDistinctUV(boundary, triArea);


        // this is just MAX but expanded bc there's only 3 elements
        var top = randoms[0].y < randoms[1].y ?
            (randoms[0].y < randoms[2].y ? randoms[0] : randoms[2]) :
            (randoms[1].y < randoms[2].y ? randoms[1] : randoms[2]);
        randoms.Remove(top);

        var left = randoms[0].x < randoms[1].x ? randoms[0] : randoms[1];
        randoms.Remove(left);

        // nuke if top/left is in the angles formed between "other" and the corners
        // in principle the last two checks shouldn't be necessary, i think... but they do be
        var other = randoms[0];
        if (
            PointInTriangle(top, other, TR, BR)
            || PointInTriangle(left, other, BL, BR)
            || PointInTriangle(left, other, TR, BR)
            || PointInTriangle(top, other, BL, BR))
        {
            return GetDistinctUV(boundary, triArea);
        }

        return (top, left, other);
    }

    #region https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
    static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(pt, v1, v2);
        d2 = Sign(pt, v2, v3);
        d3 = Sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }
    #endregion
}
