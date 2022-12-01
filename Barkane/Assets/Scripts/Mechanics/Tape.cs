using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;
using BarkaneEditor;
using System.Drawing.Printing;

[ExecuteAlways]
public class Tape : SidedJointAddon, IDynamicMesh<TapeRenderSettings>
{
    [SerializeField] private TapeRenderSettings settings;
    [SerializeField] private SquareRenderSettings squareRenderSettings;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;

    Vector3[] vs, ns;
    Vector2[] ringShifts; // randomize corners to look less tidy

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
        }
        else
        {
            m = meshFilter.sharedMesh;
        }

        // head A
        vs[0] = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2A * (settings.halfLength + margin) + g.nA * settings.elevation);
        ns[0] = transform.worldToLocalMatrix.MultiplyVector(g.nJ2A);
        Ring(
            ref vs, ref ns,
            vs[0],
            transform.worldToLocalMatrix.MultiplyVector(g.nA),
            transform.worldToLocalMatrix.MultiplyVector(g.tJ),
            transform.worldToLocalMatrix.MultiplyVector(g.nJ2A),
            1,
            ringShifts[0], ringShifts[1]);
        if (g.a2b > 20f && g.a2b < 160f) // bending inwards
        {
            // 3 inner joints collapse together
            var shrinkCorrection = 1f / Mathf.Sin(Mathf.Deg2Rad * g.a2b / 2);
            var j = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ * (settings.elevation * shrinkCorrection));
            Ring(
                ref vs, ref ns,
                j,
                transform.worldToLocalMatrix.MultiplyVector(g.nJ),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 4);
            Ring(
                ref vs, ref ns,
                j,
                transform.worldToLocalMatrix.MultiplyVector(g.nJ),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 2 * 4);
            Ring(
                ref vs, ref ns,
                j,
                transform.worldToLocalMatrix.MultiplyVector(g.nJ),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 3 * 4);
        }
        else // bending outwards
        {
            // near joint on side A
            var jA = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2A * margin + g.nA * settings.elevation);
            Ring(
                ref vs, ref ns,
                jA,
                transform.worldToLocalMatrix.MultiplyVector(g.nA),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 4);
            // joint
            var j = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ * settings.elevation);
            Ring(
                ref vs, ref ns,
                j,
                transform.worldToLocalMatrix.MultiplyVector(g.nJ),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 2 * 4);
            // near joint on side B
            var jB = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2B * margin + g.nB * settings.elevation);
            Ring(
                ref vs, ref ns,
                jB,
                transform.worldToLocalMatrix.MultiplyVector(g.nB),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 3 * 4);
        }
        // head B
        vs[vs.Length - 1] = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2B * (settings.halfLength + margin) + g.nB * settings.elevation);
        ns[ns.Length - 1] = transform.worldToLocalMatrix.MultiplyVector(g.nJ2B);
        Ring(
            ref vs, ref ns,
            vs[vs.Length - 1],
            transform.worldToLocalMatrix.MultiplyVector(g.nB),
            transform.worldToLocalMatrix.MultiplyVector(g.tJ),
            transform.worldToLocalMatrix.MultiplyVector(g.nJ2B),
            1 + 4 * 4,
            ringShifts[2], ringShifts[3]);

        m.vertices = vs;
        m.normals = ns;

        if (firstSet)
        {
            m.triangles = settings.ids;
        }
    }

    private void Ring(ref Vector3[] vs, ref Vector3[] ns, Vector3 c, Vector3 n, Vector3 t, int iStart)
    {
        var w = settings.width * t;
        var h = settings.thickness * n;
        vs[iStart] = c - w - h;
        vs[iStart + 1] = c - w + h;
        vs[iStart + 2] = c + w + h;
        vs[iStart + 3] = c + w - h;
    }

    private void Ring(ref Vector3[] vs, ref Vector3[] ns, Vector3 c, Vector3 n, Vector3 t, Vector3 z, int iStart, Vector2 randomL, Vector2 randomR)
    {
        var w = settings.width * t;
        var h = settings.thickness * n;
        vs[iStart] = c - w - h + randomL.x * t + randomL.y * z;
        vs[iStart + 1] = c - w + h + randomL.x * t + randomL.y * z;
        vs[iStart + 2] = c + w + h + randomR.x * t + randomR.y * z;
        vs[iStart + 3] = c + w - h + randomR.x * t + randomR.y * z;
    }

    public void ClearAndInitBuffers(TapeRenderSettings settings)
    {
        meshFilter.sharedMesh = null;

        vs = new Vector3[settings.VCount];
        ns = new Vector3[settings.VCount];

        ringShifts = new Vector2[] {
            new Vector2(Random.value * settings.randomizeX, Random.value * settings.randomizeY),
            new Vector2(Random.value * settings.randomizeX, Random.value * settings.randomizeY),
            new Vector2(Random.value * settings.randomizeX, Random.value * settings.randomizeY),
            new Vector2(Random.value * settings.randomizeX, Random.value * settings.randomizeY)
        };
    }
}
