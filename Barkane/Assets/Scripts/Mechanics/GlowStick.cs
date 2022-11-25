using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BarkaneJoint;
using BarkaneEditor;

[ExecuteAlways]
public class GlowStick : MonoBehaviour, IRefreshable
{
    [SerializeField] private bool jointSide; // true = side 1, false = side 2

    [SerializeField] private GlowstickRenderSettings settingsInner, settingsOuter;

    [SerializeField] private SquareRenderSettings squareRenderSettings;

    private JointRenderer jr;

    [SerializeField] MeshRenderer innerRenderer, outerRenderer;
    [SerializeField] MeshFilter innerFilter, outerFilter;


#if UNITY_EDITOR
    public bool SameSide(GlowStick other) => jointSide == other.jointSide;
#endif

    private void Update()
    {
        if (jr == null) jr = transform.parent.GetComponentInChildren<JointRenderer>();
        UpdateMesh(innerFilter, settingsInner, squareRenderSettings.margin);
        UpdateMesh(outerFilter, settingsOuter, squareRenderSettings.margin);
    }

    private void UpdateMesh(MeshFilter filter, GlowstickRenderSettings settings, float margin)
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

        var vs = new Vector3[5 * settings.resolution + 2];
        var ns = new Vector3[5 * settings.resolution + 2];

        var g = jointSide ? jr.side1Geometry : jr.side2Geometry;

        // head A
        vs[0] = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2A * (settings.halfLength + squareRenderSettings.margin) + g.nA * settings.elevation);
        ns[0] = transform.worldToLocalMatrix.MultiplyVector(g.nJ2A);
        Ring(
            ref vs, ref ns, 
            vs[0],
            transform.worldToLocalMatrix.MultiplyVector(g.nA),
            transform.worldToLocalMatrix.MultiplyVector(g.tJ),
            1, settings);
        // near joint on side A
        var jA = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2A * squareRenderSettings.margin + g.nA * settings.elevation);
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
        var jB = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2B * squareRenderSettings.margin + g.nB * settings.elevation);
        Ring(
            ref vs, ref ns,
            jB,
            transform.worldToLocalMatrix.MultiplyVector(g.nB),
            transform.worldToLocalMatrix.MultiplyVector(g.tJ),
            1 + 3 * settings.resolution, settings);
        // head B
        vs[vs.Length - 1] = transform.worldToLocalMatrix.MultiplyPoint(g.pJ + g.nJ2B * (settings.halfLength + squareRenderSettings.margin) + g.nB * settings.elevation);
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

    private void Ring(ref Vector3[] vs, ref Vector3[] ns, Vector3 o, Vector3 n, Vector3 t, int iStart, GlowstickRenderSettings settings)
    {
        for(int i = iStart, j = 0; i < iStart + settings.resolution; i++, j++)
        {
            var sin = settings.angles[j].x;
            var cos = settings.angles[j].y;

            vs[i] = o + settings.radius * (sin * n + cos * t);
            ns[i] = sin * n + cos * t;
        }
    }

    public void Refresh()
    {
        innerFilter.sharedMesh = null;
        outerFilter.sharedMesh = null;
    }
}