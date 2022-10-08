using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

[ExecuteAlways]
public class CrumbleMeshGenerator : MonoBehaviour, BarkaneEditor.ILoadable
{

    private readonly static Vector2 TL = new Vector2(0, 0);
    private readonly static Vector2 TR = new Vector2(1, 0);
    private readonly static Vector2 BL = new Vector2(0, 1);
    private readonly static Vector2 BR = new Vector2(1, 1);

    private readonly static int END_OF_TILE_CORNERS = 3;

    [SerializeField] private ComputeShader crumbleShader, blurShader;
    [SerializeField] private FractureMeshSettings setting;

    public void Load()
    {
        crumbleShader.SetFloat("resolution", setting.resolution);
        blurShader.SetFloat("resolution", setting.resolution);
    }

    
    /// <summary>
    /// create relevant information for updating paper tile geometry
    /// </summary>
    /// <param name="baseMat">prototype of new material to be applied to the mesh</param>
    /// <returns>mesh, material, sprinkle positions, sprinkle normals</returns>
    public (Mesh, Material, Texture2D, Vector3[], Vector3[]) Create(Material baseMat)
    {
        var (pivTop, pivLeft, pivOther) = GetDistinctUV(setting.margin, setting.mainTriangleArea);

        // sizeof(Vector3) doesn't work
        // instead we assume each float is 4 bytes, Vector2 is a C# struct of 2 floats
        var vBuf = new ComputeBuffer(24, 2 * 4);
        var normBuf = new RenderTexture(setting.resolution, setting.resolution, 24);

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

        var v2DDup = new Vector2[]
        {
            v2D[0], v2D[1], v2D[4],
            v2D[0], v2D[5], v2D[2],
            v2D[0], v2D[4], v2D[5],
            v2D[4], v2D[6], v2D[5],
            v2D[2], v2D[5], v2D[6],
            v2D[1], v2D[3], v2D[6],
            v2D[1], v2D[6], v2D[4],
            v2D[6], v2D[3], v2D[2]
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

        // ensure correct normal direction and CCW
        var triangles = new int[3 * 8];
        for (int i = 0, j = 0; i < 8; i++, j += 3)
        {
            var centroid = (Vector3) GetCentroid(in vDup, j, j + 1, j + 2);
            var arm1 = centroid - vDup[j];
            var arm2 = centroid - vDup[j + 1];

            // nuke if triangle too narrow
            if (DoubleArea(vDup, j, j + 1, j + 2) < setting.allTriangleArea)
            {
                vBuf.Dispose();
                return Create(baseMat);
            }

            triangles[j] = j;
            if (Vector3.Dot(Vector3.Cross(arm1, arm2), transform.up) > 0)
            {
                // CCW face up
                triangles[j + 1] = j + 2;
                triangles[j + 2] = j + 1;
            } else
            {
                // CW face down
                triangles[j + 2] = j + 1;
                triangles[j + 1] = j + 2;
            }
        }

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

            triangles = triangles,

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
        m.Optimize();

        // get base normal map from compute shader and pass that to the material
        var mat = new Material(baseMat);
        mat.name = $"hydrated [{baseMat.name}]";
        vBuf.SetData(v2DDup, 0, 0, 24);
        crumbleShader.SetBuffer(0, "pivots", vBuf);

        // render texture creation and use
        // https://youtu.be/BrZ4pWwkpto
        normBuf.enableRandomWrite = true;
        normBuf.Create();
        crumbleShader.SetTexture(0, "Result", normBuf);
        Dispatch(crumbleShader);

        blurShader.SetTexture(0, "Result", normBuf);
        blurShader.SetTexture(1, "Result", normBuf);

        // repeat gaussian blur for several times
        for (int i = 0; i < setting.blurLoop; i++)
        {
            // gaussian blur is approximated by horizontal + vertical box blurs
            Dispatch(blurShader, 0); // the horizontal pass
            Dispatch(blurShader, 1); // the vertical pass
        }

        mat.SetVector("YOverride", new Vector4(transform.up.x, transform.up.y, transform.up.z, 1));

        int sprinkleCount = setting.sprinkleCount + Random.Range(0, setting.sprinkleBonus);
        var sprinkleVerts = new Vector3[sprinkleCount];
        var sprinkleNorms = new Vector3[sprinkleCount];
        for (int i = 0; i < sprinkleCount; i++)
        {
            var uv = new Vector2(Random.value, Random.value);
            // check against every triangle...
            for(int t = 0; t < 24; t += 3)
            {
                if (PointInTriangle(uv, v2DDup[triangles[t]], v2DDup[triangles[t + 1]], v2DDup[triangles[t + 2]]))
                {
                    // get displaced sprinkle position within the mesh
                    sprinkleVerts[i] = EvaluateBarycentric(
                        FindBarycentric(uv, v2DDup[triangles[t]], v2DDup[triangles[t + 1]], v2DDup[triangles[t + 2]]),
                        vDup[triangles[t]], vDup[triangles[t + 1]], vDup[triangles[t + 2]]
                        );

                    // get normal from the traingle, note that the given order is always CW as specified by Unity
                    sprinkleNorms[i] = Vector3.Cross(
                        vDup[triangles[t + 1]] - vDup[triangles[t]],
                        vDup[triangles[t + 2]] - vDup[triangles[t]])
                        .normalized;

                    // bumping
                    sprinkleVerts[i] += sprinkleNorms[i] * setting.sprinkleElevation;

                    continue;
                }
            }
        }

        vBuf.Dispose();

        // transfer distance from GPU to CPu
        var cpuTexture = new Texture2D(setting.resolution, setting.resolution);
        // https://answers.unity.com/questions/37134/is-it-possible-to-save-rendertextures-into-png-fil.html
        RenderTexture.active = normBuf;
        cpuTexture.ReadPixels(new Rect(0, 0, setting.resolution, setting.resolution), 0, 0);
        cpuTexture.Apply();
        RenderTexture.active = null;
        DestroyImmediate(normBuf);

        mat.SetTexture("Dist", cpuTexture);

        return (m, mat, cpuTexture, sprinkleVerts, sprinkleNorms);
    }

    private void Dispatch(ComputeShader cs, int kernelIndex = 0)
    {
        cs.Dispatch(kernelIndex, setting.groupSize, setting.groupSize, 1);
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

    private static float DoubleArea(Vector3[] src, int a, int b, int c)
    {
        return (src[b].x - src[a].x) * (src[c].z - src[a].z) - (src[c].x - src[a].x) * (src[b].z - src[a].z);
    }

    private static float DoubleArea(List<Vector2> src, int a, int b, int c)
    {
        return (src[b].x - src[a].x) * (src[c].y - src[a].y) - (src[c].x - src[a].x) * (src[b].y - src[a].y);
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

        // nuke if triangle (parallelogram) too narrow/small
        if (DoubleArea(randoms, 0, 1, 2) < triArea) return GetDistinctUV(boundary, triArea);

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

    #region https://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
    (float, float, float) FindBarycentric(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v0 = b - a, v1 = c - a, v2 = p - a;
        var d00 = Vector2.Dot(v0, v0);
        var d01 = Vector2.Dot(v0, v1);
        var d11 = Vector2.Dot(v1, v1);
        var d20 = Vector2.Dot(v2, v0);
        var d21 = Vector2.Dot(v2, v1);
        var denom = d00 * d11 - d01 * d01;
        var v = (d11 * d20 - d01 * d21) / denom;
        var w = (d00 * d21 - d01 * d20) / denom;
        var u = 1.0f - v - w;
        return (u, v, w);
    }

    Vector3 EvaluateBarycentric((float, float, float) uvw, Vector3 a, Vector3 b, Vector3 c)
        => uvw.Item1 * a + uvw.Item2 * b + uvw.Item3 * c;
    #endregion
}
