using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;
using BarkaneEditor;
using System.Drawing.Printing;
using UnityEditor;

[ExecuteAlways]
public class Tape : SidedJointAddon, IDynamicMesh<TapeRenderSettings>
{
    [SerializeField] private TapeRenderSettings settings;
    [SerializeField] private SquareRenderSettings squareRenderSettings;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;

    Vector3[] vs;
    Vector2[] ringShifts; // randomize corners to look less tidy

    private void Update()
    {
        // lock to world space orientation
        transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        UpdateMesh(squareRenderSettings.margin);
    }

    private void UpdateMesh(float margin)
    {
        if (vs == null || vs.Length != settings.VCount) ClearAndInitBuffers(settings);

        var g = FetchGeometry();

        var firstSet = meshFilter.sharedMesh == null;
        Mesh m;
        if (firstSet)
        {
            m = new Mesh();
            meshFilter.sharedMesh = m;
            m.MarkDynamic();
        }
        else
        {
            m = meshFilter.sharedMesh;
        }

        // head A
        vs[0] = g.nJ2A * (settings.halfLength + margin) + g.nA * settings.elevation;
        Ring(ref vs,
            vs[0],
            g.nA,
            g.tJ,
            1);
        if (g.a2b > 20f && g.a2b < 160f) // bending inwards
        {
            // 3 inner joints collapse together
            var shrinkCorrection = 1f / Mathf.Sin(Mathf.Deg2Rad * g.a2b / 2);
            var j = g.nJ * (settings.elevation * shrinkCorrection);
            Ring(
                ref vs,
                j,
                g.nJ,
                g.tJ,
                1 + 4);
            Ring(
                ref vs,
                j,
                g.nJ,
                g.tJ,
                1 + 2 * 4);
            Ring(
                ref vs,
                j,
                g.nJ,
                g.tJ,
                1 + 3 * 4);
        }
        else // bending outwards
        {
            // near joint on side A
            var jA = g.nJ2A * margin + g.nA * settings.elevation;
            Ring(
                ref vs,
                jA,
                g.nA,
                g.tJ,
                1 + 4);
            // joint
            var j = g.nJ * settings.elevation;
            Ring(
                ref vs,
                j,  
                g.nJ,
                g.tJ,
                1 + 2 * 4);
            // near joint on side B
            var jB = g.nJ2B * margin + g.nB * settings.elevation;
            Ring(
                ref vs,
                jB,
                g.nB,
                g.tJ,
                1 + 3 * 4);
        }
        // head B
        vs[^1] = g.nJ2B * (settings.halfLength + margin) + g.nB * settings.elevation;
        Ring(
            ref vs,
            vs[vs.Length - 1],
            g.nB,
            g.tJ,
            1 + 4 * 4);

        m.vertices = vs;
        m.RecalculateNormals();
        m.RecalculateBounds();

        if (firstSet)
        {
            m.triangles = settings.ids;
        }
    }

    private void Ring(ref Vector3[] vs, Vector3 c, Vector3 n, Vector3 t, int iStart)
    {
        var w = settings.width * t;
        var h = settings.thickness * n;

        vs[iStart] = c - w - h;
        vs[iStart + 1] = c - w + h;
        vs[iStart + 2] = c + w + h;
        vs[iStart + 3] = c + w - h;
    }

    public void ClearAndInitBuffers(TapeRenderSettings settings)
    {
        meshFilter.sharedMesh = null;

        vs = new Vector3[settings.VCount];
    }
}
