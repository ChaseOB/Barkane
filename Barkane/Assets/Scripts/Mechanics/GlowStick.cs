using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BarkaneJoint;
using BarkaneEditor;

[ExecuteAlways]
public class GlowStick : SidedJointAddon, IRefreshable
{
    [SerializeField] private GlowstickRenderSettings settingsInner, settingsOuter;
    [SerializeField] private SquareRenderSettings squareRenderSettings;

    [SerializeField] MeshRenderer innerRenderer, outerRenderer;
    [SerializeField] MeshFilter innerFilter, outerFilter;

    private Vector3[] vsInner, nsInner, vsOuter, nsOuter;

    private void Start()
    {
        Refresh();
    }

    private void LateUpdate()
    {
        UpdateMesh(innerFilter, settingsInner, squareRenderSettings.margin, vsInner, nsInner);
        UpdateMesh(outerFilter, settingsOuter, squareRenderSettings.margin, vsOuter, nsOuter);
    }

    private void UpdateMesh(MeshFilter filter, GlowstickRenderSettings settings, float margin, Vector3[] vs, Vector3[] ns)
    {
        Mesh m;
        var firstSet = filter.sharedMesh == null;
        if (firstSet)
        {
            m = new Mesh();
            m.MarkDynamic();
            filter.sharedMesh = m;
        } else
        {
            m = filter.sharedMesh;
        }

        var g = FetchGeometry();

        // head A
        vs[0] = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2A * (settings.halfLength + margin) + g.nA * settings.elevation);
        ns[0] = transform.worldToLocalMatrix.MultiplyVector(g.nJ2A);
        Ring(
            ref vs, ref ns, 
            vs[0],
            transform.worldToLocalMatrix.MultiplyVector(g.nA),
            transform.worldToLocalMatrix.MultiplyVector(g.tJ),
            1, settings);
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
                1 + settings.resolution, settings, shrinkCorrection);
            Ring(
                ref vs, ref ns,
                j,
                transform.worldToLocalMatrix.MultiplyVector(g.nJ),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 2 * settings.resolution, settings, shrinkCorrection);
            Ring(
                ref vs, ref ns,
                j,
                transform.worldToLocalMatrix.MultiplyVector(g.nJ),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 3 * settings.resolution, settings, shrinkCorrection);
        } else // bending outwards
        {
            // near joint on side A
            var jA = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2A * margin + g.nA * settings.elevation);
            Ring(
                ref vs, ref ns,
                jA,
                transform.worldToLocalMatrix.MultiplyVector(g.nA),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + settings.resolution, settings);
            // joint
            var j = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ * settings.elevation);
            Ring(
                ref vs, ref ns,
                j,
                transform.worldToLocalMatrix.MultiplyVector(g.nJ),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 2 * settings.resolution, settings);
            // near joint on side B
            var jB = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2B * margin + g.nB * settings.elevation);
            Ring(
                ref vs, ref ns,
                jB,
                transform.worldToLocalMatrix.MultiplyVector(g.nB),
                transform.worldToLocalMatrix.MultiplyVector(g.tJ),
                1 + 3 * settings.resolution, settings);
        } 
        // head B
        vs[vs.Length - 1] = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2B * (settings.halfLength + margin) + g.nB * settings.elevation);
        ns[ns.Length - 1] = transform.worldToLocalMatrix.MultiplyVector(g.nJ2B);
        Ring(
            ref vs, ref ns,
            vs[vs.Length - 1],
            transform.worldToLocalMatrix.MultiplyVector(g.nB),
            transform.worldToLocalMatrix.MultiplyVector(g.tJ),
            1 + 4 * settings.resolution, settings);

        m.vertices = vs;
        m.normals = ns;
        m.MarkModified();

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

    public void Refresh()
    {
        innerFilter.sharedMesh = null;
        outerFilter.sharedMesh = null;

        vsInner = new Vector3[5 * settingsInner.resolution + 2];
        nsInner = new Vector3[5 * settingsInner.resolution + 2];
        vsOuter = new Vector3[5 * settingsOuter.resolution + 2];
        nsOuter = new Vector3[5 * settingsOuter.resolution + 2];
    }
}