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

    public Vector3 a, jA, j, jB, b;

    private void Update()
    {
        // lock to world space orientation
        transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        UpdateMesh(squareRenderSettings.margin);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + a, 0.03f);
        Gizmos.DrawSphere(transform.position +jA, 0.03f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position +j, 0.03f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position +jB, 0.03f);
        Gizmos.DrawSphere(transform.position +b, 0.03f);

    }

    private void UpdateMesh(float margin)
    {
        if (vs == null || vs.Length != settings.VCount) ClearAndInitBuffers(settings);

        var (g, gSide) = FetchGeometry();

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
        a = (g.pA - transform.position) + (g.edgeA - g.pA) * (1- settings.halfLength);  //g.nJ2A.normalized * (settings.halfLength + margin) + gSide.nA * settings.elevation +;
        vs[0] = a;
        Ring(ref vs,
            vs[0],
            gSide.nA,
            gSide.tJ,
            1);
        // if (gSide.a2b > 20f && gSide.a2b < 160f) // bending inwards
        // {
        //     print("case 1");
        //     // 3 inner joints collapse together
        //     var shrinkCorrection = 1f / Mathf.Sin(Mathf.Deg2Rad * gSide.a2b / 2);
        //     var j = gSide.nJ * (settings.elevation * shrinkCorrection);
        //     Ring(
        //         ref vs,
        //         j,
        //         gSide.nJ,
        //         gSide.tJ,
        //         1 + 4);
        //     Ring(
        //         ref vs,
        //         j,
        //         gSide.nJ,
        //         gSide.tJ,
        //         1 + 2 * 4);
        //     Ring(
        //         ref vs,
        //         j,
        //         gSide.nJ,
        //         gSide.tJ,
        //         1 + 3 * 4);
        // }
        // else // bending outwards
        // {
        //     print("case 2");
            // near joint on side A
           // jA = g.nJ2A.normalized * margin + gSide.nA * settings.elevation;
            jA = g.edgeA - transform.position + gSide.nA * settings.elevation;
            Ring(
                ref vs,
                jA,
                gSide.nA,
                gSide.tJ,
                1 + 4);
            // joint
            j = gSide.nJ * settings.elevation + g.offset;
            Ring(
                ref vs,
                j,
                gSide.nJ,
                gSide.tJ,
                1 + 2 * 4);
            // near joint on side B
           // jB = g.nJ2B.normalized * margin + gSide.nB * settings.elevation;
              jB = g.edgeB - transform.position + gSide.nB * settings.elevation;
            Ring(
                ref vs,
                jB,
                gSide.nB,
                gSide.tJ,
                1 + 3 * 4);
        //}
        // head B
        b = (g.pB - transform.position) + (g.edgeB - g.pB) * (1- settings.halfLength);  //.normalized * settings.halfLength; //g.nJ2B.normalized * (settings.halfLength + margin) + gSide.nB * settings.elevation;
        vs[^1] = b;
        Ring(
            ref vs,
            vs[vs.Length - 1],
            gSide.nB,
            gSide.tJ,
            1 + 4 * 4);

        m.SetVertices(vs, 0, vs.Length, fConsiderBounds);
        m.RecalculateNormals();

        if (firstSet)
        {
            m.SetTriangles(settings.ids, 0, false, 0);
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
