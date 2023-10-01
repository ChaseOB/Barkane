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

    private JointRenderer jointRenderer;

    public Vector3 a, dA1, dA2, dC, dB2, dB1, b, anchor;
    public JointGeometryData g;

    private void Awake()
    {
        visualRoot = transform.GetChild(0);
        jointRenderer = transform.parent.GetComponentInChildren<JointRenderer>();
    }

    private void Update()
    {
        visualRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void LateUpdate()
    {
        UpdateMesh(
            innerFilter, settingsInner, squareRenderSettings.margin, 
            ref vsInner
            // ref nsInner
            );
        UpdateMesh(
            outerFilter, settingsOuter, squareRenderSettings.margin, 
            ref vsOuter
            // ref nsOuter
            );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(a, 0.03f);
        Gizmos.DrawSphere(dA2 + g.pJ, 0.03f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(dC + g.pJ, 0.03f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(dA1 + g.pJ, 0.03f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(dB1 + g.pJ, 0.03f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(b, 0.03f);
        Gizmos.DrawSphere(dB2 + g.pJ, 0.03f);
        
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(anchor + g.pJ, 0.03f);


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
        this.g = g;

        // Order of joints: A, A2, A1, C, B1, B2, B
        // A
        //vs[0] = 
        a = g.pA + (g.edgeA - g.pA) * (1- settings.halfLength) + gSide.nA * settings.elevation ; //g.pJ + g.nJ2A.normalized * (settings.halfLength + margin) + gSide.nA * settings.elevation ; // + jointRenderer.GetParentSquareOffsets().Item1; A abchor
        vs[0] = a;
        // ns[0] = g.nJ2A;
        Ring(ref vs, vs[0], gSide.nA, gSide.tJ, 1, settings);

        ga2b = gSide.a2b;

        // Ideally correctionT should be minimal at small outward angles and suddenly jump to 1 at folding over
        // Here we approximate by taking it to some power > 1 so smaller fractions are compressed
        var correctionT = Mathf.Max(-Vector3.Dot(gSide.nJ, g.nJ2A.normalized), 0);
        correctionT *= correctionT;
        correctionT *= correctionT;
        var correction = Vector3.zero; //correctionT * gSide.nJ * margin * settings.marginCorrection;
        //var anchor = gSide.nJ * settings.elevation + g.pJ; 
        anchor = gSide.nJ * settings.elevation * (Mathf.Abs(Vector3.Dot(gSide.nJ, g.nJ2A.normalized)) + 1);
        dA2 = g.edgeA - g.pJ + gSide.nA * settings.elevation + g.yOffsetB; //g.nJ2A.normalized * margin + gSide.nA * settings.elevation;
        dB2 = g.edgeB - g.pJ + gSide.nB * settings.elevation + Vector3.Project(g.yOffsetA, (g.pB - g.edgeB).normalized);//g.nJ2B.normalized * margin + gSide.nB * settings.elevation;
        dC = QuadraticBezier(dA2, anchor, dB2, 0.5f);
        //dC = gSide.nJ * settings.elevation;
        dA1 = QuadraticBezier(dA2, anchor, dB2, 0.25f);
        dB1 = QuadraticBezier(dA2, anchor, dB2, 0.75f);

        Ring(ref vs, g.pJ + dA2 + correction, gSide.nA, gSide.tJ, 1 + settings.resolution, settings);
        Ring(ref vs, g.pJ + dA1 + correction, DDQuadraticBezier(dA2, anchor, dB2, 0.25f, gSide.tJ), gSide.tJ, 1 + 2 * settings.resolution, settings);
        Ring(ref vs, g.pJ + dC + correction, gSide.nJ, gSide.tJ, 1 + 3 * settings.resolution, settings);
        Ring(ref vs, g.pJ + dB1 + correction, DDQuadraticBezier(dA2, anchor, dB2, 0.75f, gSide.tJ), gSide.tJ, 1 + 4 * settings.resolution, settings);
        Ring(ref vs, g.pJ + dB2 + correction, gSide.nB, gSide.tJ, 1 + 5 * settings.resolution, settings);

        // Debug.DrawRay(g.pJ, gSide.tJ, Color.black);
        // Debug.DrawRay(g.pJ, DDQuadraticBezier(dA2, anchor, dB2, 0.25f, gSide.tJ), Color.red);
        // Debug.DrawRay(g.pJ, gSide.nJ, Color.green);
        // Debug.DrawRay(g.pJ, DDQuadraticBezier(dA2, anchor, dB2, 0.75f, gSide.tJ), Color.blue);

        // head B
        
        b =  g.pB + (g.edgeB - g.pB) * (1- settings.halfLength) + gSide.nB * settings.elevation; //g.pJ + g.nJ2B.normalized * (settings.halfLength + margin) + gSide.nB * settings.elevation ;//+ Vector3.up * 0.5f;
        vs[^1] = b;
        // ns[^1] = g.nJ2B;
        Ring(ref vs, vs[^1], gSide.nB, gSide.tJ, 1 + 6 * settings.resolution, settings);

        m.SetVertices(vs, 0, vs.Length, fConsiderBounds);
        m.RecalculateNormals();

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

    // wikipedia
    Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return (1 - t) * ((1 - t) * a + t * b) + t * ((1 - t) * b + t * c);
    }

    Vector3 DQuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return 2 * ((c - 2 * b + a) * t + b - a);
    }

    // Normal direction
    Vector3 DDQuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t, Vector3 axis)
    {
        var T = DQuadraticBezier(a, b, c, t);

        return Vector3.Cross(axis, T).normalized;
    }
}