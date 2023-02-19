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

    private Vector3[] vsInner, nsInner, vsOuter, nsOuter;

    private Transform visualRoot;

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
        UpdateMesh(innerFilter, settingsInner, squareRenderSettings.margin * settingsInner.marginCorrection, ref vsInner, ref nsInner);
        UpdateMesh(outerFilter, settingsOuter, squareRenderSettings.margin * settingsOuter.marginCorrection, ref vsOuter, ref nsOuter);
    }


    private void UpdateMesh(MeshFilter filter, GlowstickRenderSettings settings, float margin, ref Vector3[] vs, ref Vector3[] ns, bool force = false)
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

        var g = FetchGeometry();

        // Order of joints: A, A1, A2, C, B2, B1, B
        // A
        vs[0] = g.pJ + g.nJ2A * (settings.halfLength + margin) + g.nA * settings.elevation;
        ns[0] = g.nJ2A;
        Ring(ref vs, ref ns, vs[0], g.nA, g.tJ, 1, settings);

        if (g.a2b > 20f && g.a2b < 160f) // bending inwards
        {
            // A1, A2, C, B2, B1
            var shrinkCorrection = 1f / Mathf.Sin(Mathf.Deg2Rad * g.a2b / 2);
            var j = g.pJ + g.nJ * (settings.elevation * shrinkCorrection);
            Ring(ref vs, ref ns, j, g.nJ, g.tJ, 1 + settings.resolution, settings, shrinkCorrection);
            Ring(ref vs, ref ns, j, g.nJ, g.tJ, 1 + 2 * settings.resolution, settings, shrinkCorrection);
            Ring(ref vs, ref ns, j, g.nJ, g.tJ, 1 + 3 * settings.resolution, settings, shrinkCorrection);
            Ring(ref vs, ref ns, j, g.nJ, g.tJ, 1 + 4 * settings.resolution, settings, shrinkCorrection);
            Ring(ref vs, ref ns, j, g.nJ, g.tJ, 1 + 5 * settings.resolution, settings, shrinkCorrection);
        } else if (Mathf.Abs(g.a2b) > 30f)// bending outwards
        {
            // A1, A2, C, B2, B1
            var dispC = g.nJ * settings.elevation;
            var dispA2 = g.nJ2A * margin + g.nA * settings.elevation;
            var dispB2 = g.nJ2B * margin + g.nB * settings.elevation;
            var dispA1 = Vector3.Slerp(dispC, dispA2, 0.5f);
            var dispB1 = Vector3.Slerp(dispC, dispB2, 0.5f);

            Ring(ref vs, ref ns, dispA2 + g.pJ, g.nA, g.tJ, 1 + settings.resolution, settings);
            Ring(ref vs, ref ns, dispA1 + g.pJ, (g.nA + g.nJ).normalized, g.tJ, 1 + 2 * settings.resolution, settings);
            Ring(ref vs, ref ns, dispC + g.pJ, g.nJ, g.tJ, 1 + 3 * settings.resolution, settings);
            Ring(ref vs, ref ns, dispB1 + g.pJ, (g.nB + g.nJ).normalized, g.tJ, 1 + 4 * settings.resolution, settings);
            Ring(ref vs, ref ns, dispB2 + g.pJ, g.nB, g.tJ, 1 + 5 * settings.resolution, settings);
        } else
        {
            // A1, A2, C, B2, B1
            // note nJ2A = nJ2B = nJ at extreme case
            var dispC = - g.nJ * settings.elevation;
            var dispA2 = g.nA * settings.elevation;
            var dispB2 = g.nB * settings.elevation;
            var dispA1 = Vector3.Slerp(dispC, dispA2, 0.5f);
            var dispB1 = Vector3.Slerp(dispC, dispB2, 0.5f);

            Debug.DrawRay(g.pJ + dispA2, g.nA, Color.red);
            Debug.DrawRay(g.pJ + dispA1, g.nA, Color.blue);
            Debug.DrawRay(g.pJ + dispC, Vector3.Slerp(g.nJ2A, g.nJ2B, 0.5f), Color.green);
            Debug.DrawRay(g.pJ + dispB1, g.nB, Color.cyan);
            Debug.DrawRay(g.pJ + dispB2, g.nB, Color.yellow);

            Ring(ref vs, ref ns, g.pJ + dispA2, g.nA, g.tJ, 1 + settings.resolution, settings);
            Ring(ref vs, ref ns, g.pJ + dispA1, (g.nA - g.nJ).normalized, g.tJ, 1 + 2 * settings.resolution, settings);
            Ring(ref vs, ref ns, g.pJ + dispC, -g.nJ, g.tJ, 1 + 3 * settings.resolution, settings);
            Ring(ref vs, ref ns, g.pJ + dispB1, (g.nB - g.nJ).normalized, g.tJ, 1 + 4 * settings.resolution, settings);
            Ring(ref vs, ref ns, g.pJ + dispB2, g.nB, g.tJ, 1 + 5 * settings.resolution, settings);
        }
        // head B
        vs[^1] = g.pJ + g.nJ2B * (settings.halfLength + margin) + g.nB * settings.elevation;
        ns[^1] = g.nJ2B;
        Ring(ref vs, ref ns, vs[^1], g.nB, g.tJ, 1 + 6 * settings.resolution, settings);

        m.vertices = vs;
        m.normals = ns;
        // m.RecalculateNormals();

        if (firstSet)
        {
            m.triangles = settings.indices;
        }
    }

    private void Ring(ref Vector3[] vs, ref Vector3[] ns, Vector3 o, Vector3 n, Vector3 t, int iStart, GlowstickRenderSettings settings, float rFactor = 1f)
    {
        for(int i = iStart, j = 0; i < iStart + settings.resolution; i++, j++)
        {
            var sin = settings.angles[j].x;
            var cos = settings.angles[j].y;

            vs[i] = o + settings.radius * rFactor * (sin * n + cos * t);
            ns[i] = sin * n + cos * t;
        }
    }

    public void ClearAndInitBuffers(GlowstickRenderSettings settings)
    {
        if (settings == settingsInner)
        {
            vsInner = new Vector3[settingsInner.VCount];
            nsInner = new Vector3[settingsInner.VCount];
        }
        if (settings == settingsOuter)
        {
            vsOuter = new Vector3[settingsOuter.VCount];
            nsOuter = new Vector3[settingsOuter.VCount];
        }
    }

    public void EditorRefresh()
    {
        UpdateMesh(innerFilter, settingsInner, squareRenderSettings.margin * settingsInner.marginCorrection, ref vsInner, ref nsInner, true);
        UpdateMesh(outerFilter, settingsOuter, squareRenderSettings.margin * settingsOuter.marginCorrection, ref vsOuter, ref nsOuter, true);
    }

    public void RuntimeRefresh()
    {
        UpdateMesh(innerFilter, settingsInner, squareRenderSettings.margin * settingsInner.marginCorrection, ref vsInner, ref nsInner, true);
        UpdateMesh(outerFilter, settingsOuter, squareRenderSettings.margin * settingsOuter.marginCorrection , ref vsOuter, ref nsOuter, true);
    }
}