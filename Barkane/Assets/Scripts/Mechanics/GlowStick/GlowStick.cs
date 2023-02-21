using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;
using BarkaneEditor;

[ExecuteAlways]
public class GlowStick : SidedJointAddon, IDynamicMesh<GlowstickRenderSettings>, IRefreshable
{
    [SerializeField] private GlowstickRenderSettings settingsInner, settingsOuter;
    [SerializeField] private SquareRenderSettings squareRenderSettings;

    [SerializeField] public MeshRenderer innerRenderer, outerRenderer;
    [SerializeField] MeshFilter innerFilter, outerFilter;

    private Vector3[] vsInner, vsOuter;
    // private Vector3[] nsInner, nsOuter;

    private Transform visualRoot;

    public float ga2b;

    private void Awake()
    {
        visualRoot = transform.GetChild(0);
    }

    private void Update()
    {
        visualRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void LateUpdate()
    {
        UpdateMesh(
            innerFilter, settingsInner, squareRenderSettings.margin * settingsInner.marginCorrection, 
            ref vsInner
            // ref nsInner
            );
        UpdateMesh(
            outerFilter, settingsOuter, squareRenderSettings.margin * settingsOuter.marginCorrection, 
            ref vsOuter
            // ref nsOuter
            );
    }


    private void UpdateMesh(
        MeshFilter filter, GlowstickRenderSettings settings, float margin, 
        ref Vector3[] vs, 
        // ref Vector3[] ns, 
        bool force = false)
    {

        Mesh m;
        var firstSet = force || filter.sharedMesh == null || vs == null || vs.Length != settings.VCount;
        if (firstSet)
        {
            ClearAndInitBuffers(settings);
            m = new Mesh();
            m.MarkDynamic();
            filter.sharedMesh = m; 
        } else
        {
            m = filter.sharedMesh;
        }

        var (g, gSide) = FetchGeometry();

        // Order of joints: A, A1, A2, C, B2, B1, B
        // A
        vs[0] = g.pJ + g.nJ2A * (settings.halfLength + margin) + gSide.nA * settings.elevation;
        // ns[0] = g.nJ2A;
        Ring(ref vs, vs[0], gSide.nA, gSide.tJ, 1, settings);

        ga2b = gSide.a2b;

        float a2b = gSide.a2b < 0 ? gSide.a2b + 360 : gSide.a2b;

        Debug.DrawRay(g.pJ, gSide.tJ, Color.black);
        Debug.DrawRay(g.pJ + g.nJ2A, g.nJ2A, Color.red);
        Debug.DrawRay(g.pJ + g.nJ2A, gSide.nA, Color.magenta);
        Debug.DrawRay(g.pJ, gSide.nJ, Color.green);
        Debug.DrawRay(g.pJ + g.nJ2B, gSide.nB, Color.cyan);
        Debug.DrawRay(g.pJ + g.nJ2B, g.nJ2B, Color.blue);

        if (a2b > 20f && a2b < 160f) // bending inwards
        {
            // A1, A2, C, B2, B1
            //var shrinkCorrection = 1f / Mathf.Sin(Mathf.Deg2Rad * gSide.a2b / 2);
            //var j = g.pJ + gSide.nJ * (settings.elevation * shrinkCorrection);
            //Ring(ref vs, j, gSide.nJ, gSide.tJ, 1 + settings.resolution, settings, shrinkCorrection);
            //Ring(ref vs, j, gSide.nJ, gSide.tJ, 1 + 2 * settings.resolution, settings, shrinkCorrection);
            //Ring(ref vs, j, gSide.nJ, gSide.tJ, 1 + 3 * settings.resolution, settings, shrinkCorrection);
            //Ring(ref vs, j, gSide.nJ, gSide.tJ, 1 + 4 * settings.resolution, settings, shrinkCorrection);
            //Ring(ref vs, j, gSide.nJ, gSide.tJ, 1 + 5 * settings.resolution, settings, shrinkCorrection);

            // A1, A2, C, B2, B1
            var anchor = gSide.nJ * settings.elevation / Vector3.Dot(gSide.nJ, g.nJ2A);
            var dispA2 = g.nJ2A * margin + gSide.nA * settings.elevation;
            var dispB2 = g.nJ2B * margin + gSide.nB * settings.elevation;
            var dispC = QuadraticBezier(dispA2, anchor, dispB2, 0.5f);
            var dispA1 = QuadraticBezier(dispA2, anchor, dispB2, 0.25f);
            var dispB1 = QuadraticBezier(dispA2, anchor, dispB2, 0.75f);

            Ring(ref vs, dispA2 + g.pJ, gSide.nA, gSide.tJ, 1 + settings.resolution, settings);
            Ring(ref vs, dispA1 + g.pJ, (gSide.nA + gSide.nJ).normalized, gSide.tJ, 1 + 2 * settings.resolution, settings);
            Ring(ref vs, dispC + g.pJ, gSide.nJ, gSide.tJ, 1 + 3 * settings.resolution, settings);
            Ring(ref vs, dispB1 + g.pJ, (gSide.nB + gSide.nJ).normalized, gSide.tJ, 1 + 4 * settings.resolution, settings);
            Ring(ref vs, dispB2 + g.pJ, gSide.nB, gSide.tJ, 1 + 5 * settings.resolution, settings);
        } else if (a2b < 330f && a2b > 159f)// bending outwards
        {
            // A1, A2, C, B2, B1
            var anchor = gSide.nJ * settings.elevation / Vector3.Dot(gSide.nJ, g.nJ2A);
            var dispA2 = g.nJ2A * margin + gSide.nA * settings.elevation;
            var dispB2 = g.nJ2B * margin + gSide.nB * settings.elevation;
            var dispC = QuadraticBezier(dispA2, anchor, dispB2, 0.5f);
            var dispA1 = QuadraticBezier(dispA2, anchor, dispB2, 0.25f);
            var dispB1 = QuadraticBezier(dispA2, anchor, dispB2, 0.75f);

            Ring(ref vs, dispA2 + g.pJ, gSide.nA, gSide.tJ, 1 + settings.resolution, settings);
            Ring(ref vs, dispA1 + g.pJ, (gSide.nA + gSide.nJ).normalized, gSide.tJ, 1 + 2 * settings.resolution, settings);
            Ring(ref vs, dispC + g.pJ, gSide.nJ, gSide.tJ, 1 + 3 * settings.resolution, settings);
            Ring(ref vs, dispB1 + g.pJ, (gSide.nB + gSide.nJ).normalized, gSide.tJ, 1 + 4 * settings.resolution, settings);
            Ring(ref vs, dispB2 + g.pJ, gSide.nB, gSide.tJ, 1 + 5 * settings.resolution, settings);
        } else
        {
            // A1, A2, C, B2, B1
            // note nJ2A = nJ2B = nJ at extreme case
            var dispC = gSide.nJ * settings.elevation;
            var dispA2 = gSide.nA * settings.elevation;
            var dispB2 = gSide.nB * settings.elevation;
            var dispA1 = Vector3.Slerp(dispC, dispA2, 0.5f);
            var dispB1 = Vector3.Slerp(dispC, dispB2, 0.5f);

            Ring(ref vs, g.pJ + dispA2, gSide.nA, gSide.tJ, 1 + settings.resolution, settings);
            Ring(ref vs, g.pJ + dispA1, (gSide.nA + gSide.nJ).normalized, gSide.tJ, 1 + 2 * settings.resolution, settings);
            Ring(ref vs, g.pJ + dispC, gSide.nJ, gSide.tJ, 1 + 3 * settings.resolution, settings);
            Ring(ref vs, g.pJ + dispB1, (gSide.nB + gSide.nJ).normalized, gSide.tJ, 1 + 4 * settings.resolution, settings);
            Ring(ref vs, g.pJ + dispB2, gSide.nB, gSide.tJ, 1 + 5 * settings.resolution, settings);
        }
        // head B
        vs[^1] = g.pJ + g.nJ2B * (settings.halfLength + margin) + gSide.nB * settings.elevation;
        // ns[^1] = g.nJ2B;
        Ring(ref vs, vs[^1], gSide.nB, gSide.tJ, 1 + 6 * settings.resolution, settings);

        m.SetVertices(vs, 0, vs.Length, fConsiderBounds);
        m.RecalculateNormals();
        // m.normals = ns;
        // m.RecalculateNormals();

        if (firstSet)
        {
            m.SetTriangles(settings.indices, 0, false);
        }
    }

    private void Ring(
        ref Vector3[] vs,
        // ref Vector3[] ns,
        Vector3 o, Vector3 n, Vector3 t, int iStart, GlowstickRenderSettings settings, float rFactor = 1f)
    {
        for(int i = iStart, j = 0; i < iStart + settings.resolution; i++, j++)
        {
            var sin = settings.angles[j].x;
            var cos = settings.angles[j].y;

            vs[i] = o + settings.radius * rFactor * (sin * n + cos * t);
            // ns[i] = sin * n + cos * t;
        }
    }

    public void ClearAndInitBuffers(GlowstickRenderSettings settings)
    {
        if (settings == settingsInner)
        {
            vsInner = new Vector3[settingsInner.VCount];
            // nsInner = new Vector3[settingsInner.VCount];
        }
        if (settings == settingsOuter)
        {
            vsOuter = new Vector3[settingsOuter.VCount];
            // nsOuter = new Vector3[settingsOuter.VCount];
        }
    }

    public void EditorRefresh()
    {
        UpdateMesh(
            innerFilter, settingsInner, squareRenderSettings.margin * settingsInner.marginCorrection,
            ref vsInner, 
            // ref nsInner,
            true);
        UpdateMesh(
            outerFilter, settingsOuter, squareRenderSettings.margin * settingsOuter.marginCorrection,
            ref vsOuter,
            // ref nsOuter,
            true);
    }

    public void RuntimeRefresh()
    {
        UpdateMesh(
            innerFilter, settingsInner, squareRenderSettings.margin * settingsInner.marginCorrection,
            ref vsInner,
            // ref nsInner,
            true);
        UpdateMesh(
            outerFilter, settingsOuter, squareRenderSettings.margin * settingsOuter.marginCorrection,
            ref vsOuter,
            // ref nsOuter,
            true);
    }

    Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return (1 - t) * ((1 - t) * a + t * b) + t * ((1 - t) * b + t * c);
    }
}