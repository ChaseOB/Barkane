using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteAlways]
public class GlowStick : MonoBehaviour
{
    [SerializeField] private bool jointSide; // true = side 1, false = side 2
    [SerializeField] private float elevation = .05f; // how far the glowstick "levitates" from the paper surface
    [SerializeField] private float halfLength = .5f;
    private readonly float calibrateLength = .4657456f; // how far the glowstick ends are from the joint, this is measured once and kept
    [SerializeField] private DeferredTransform[] deferred;
    [SerializeField] private Transform bonesEnd;
    private GameObject m_FaceA, m_FaceB;

    // lower, min 1 = more responsive but gittery; higher, max 1 = less responsive but smooth
    [SerializeField, Range(0, 1)] private float animationSmoothness;

    [SerializeField] private SquareRenderSettings squareRenderSettings;

    public void Start()
    {
        if (Application.isPlaying)
        {
            PullFaces();
        }
    }

    public void PullFaces()
    {
        var jr = transform.parent.GetComponentInChildren<JointRenderer>();

        var (one, two) = jr.facePairs;
        (m_FaceA, m_FaceB) = jointSide ? one : two;

#if UNITY_EDITOR
        if (deferred.Length != 4 || deferred.Contains(null))
            throw new UnityException("Bones Not Updated Correctly");
#endif

        foreach(var def in deferred)
        {
            def.responsiveness = 1 - animationSmoothness;
        }
    }

#if UNITY_EDITOR
    public bool SameSide(GlowStick other) => jointSide == other.jointSide;
#endif

    private void Update()
    {
        if (m_FaceA != null && m_FaceB != null) UpdateBones();
    }

    private void UpdateBones()
    {
        // lock position to faces
        var pJoint = transform.parent.position; // the location of the joint, NOT always the same as mid (see below)
        var nA = m_FaceA.transform.up;
        var nB = m_FaceB.transform.up;

        var nJ2A = (m_FaceA.transform.position - pJoint).normalized;
        var nJ2B = (m_FaceB.transform.position - pJoint).normalized;
        transform.localPosition = Vector3.zero;

        // always pin the starting tip
        deferred[0].wpos = pJoint + nJ2A * halfLength + nA * elevation;
        deferred[0].up = -nJ2A;

        // decide midpoint and ending tip depending on folding case
        var tJ = Vector3.Cross(nA, -nJ2A); // tangent along joint
        var angFold = Vector3.SignedAngle(nJ2A, nJ2B, tJ);
        var nJ = Quaternion.AngleAxis(angFold / 2f, tJ) * -nJ2A;

        if (angFold >= 10f && angFold < 80f)
            throw new UnityException("Cannot fold inwards");

        // folding outwards (270 degrees ~ 0 degrees)
        if (angFold > -80f && angFold < 10f)
        {
            // folding beyond 270 degrees
            deferred[1].wpos = pJoint + nA * elevation;
            deferred[2].wpos = pJoint + nJ * (elevation + 2 * squareRenderSettings.margin);
            deferred[3].wpos = pJoint + nB * elevation;
            deferred[1].up = (deferred[2].wpos - deferred[1].wpos).normalized;
            deferred[2].up = (deferred[3].wpos - deferred[2].wpos).normalized;
            deferred[3].up = nJ2B;
            deferred[3].scl = halfLength / calibrateLength;
        }
        // folding outwards (180 ~ 270 degrees)
        else if (angFold <= -80f && angFold > -170f)
        {
            deferred[1].wpos = pJoint + nA * elevation;
            deferred[2].wpos = pJoint + nJ * elevation;
            deferred[3].wpos = pJoint + nB * elevation;
            deferred[1].up = (deferred[2].wpos - deferred[1].wpos).normalized;
            deferred[2].up = (deferred[3].wpos - deferred[2].wpos).normalized;
            deferred[3].up = nJ2B;
            deferred[3].scl = halfLength / calibrateLength;
        }
        // folding inwards (90 ~ 180 degrees)
        else
        {
            deferred[1].wpos = pJoint + nA * elevation + nJ2A * (elevation + squareRenderSettings.margin);
            deferred[3].wpos = pJoint + nB * elevation + nJ2B * (elevation + squareRenderSettings.margin);
            deferred[2].wpos = (deferred[1].wpos + deferred[3].wpos) / 2f;
            deferred[1].up = (deferred[2].wpos - deferred[1].wpos).normalized;
            deferred[2].up = (deferred[3].wpos - deferred[1].wpos).normalized;
            deferred[3].up = nJ2B;
            deferred[3].scl = (halfLength - elevation - squareRenderSettings.margin) / calibrateLength;
        }
    }
}