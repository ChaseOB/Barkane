using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteAlways]
public class GlowStick : MonoBehaviour
{
    public GameObject FaceA => m_FaceA;
    public GameObject FaceB => m_FaceB;

    [SerializeField] private bool jointSide; // true = side 1, false = side 2
    [SerializeField] private float elevation = .05f; // how far the glowstick "levitates" from the paper surface
    [SerializeField] private float halfLength = .5f; // how far the glowstick ends are from the joint
    [SerializeField, Range(0, 0.2f)] private float critical = .05f;
    [SerializeField] private Transform[] bones;

    private GameObject m_FaceA, m_FaceB;

    [SerializeField] private SquareRenderSettings squareRenderSettings;

    public GameObject FaceAView, FaceBView;

#if UNITY_EDITOR
    public void SetFaces((GameObject, GameObject) one, (GameObject, GameObject) two)
    {
        (m_FaceA, m_FaceB) = jointSide ? one : two;
        if (bones.Length != 5 || bones.Contains(null))
            throw new UnityException("Bones Not Updated Correctly");
        FaceAView = m_FaceA;
        FaceBView = m_FaceB;
    }

    public bool SameSide(GlowStick other) => jointSide == other.jointSide;
#endif

#if UNITY_EDITOR
    private void Update()
#else
    private void FixedUpdate()
#endif
    {
        if (m_FaceA == null || m_FaceB == null) return;

        UpdateBones();
    }


    private void UpdateBones()
    {
        // lock position to faces
        var pJoint = transform.parent.position; // the location of the joint, NOT always the same as mid (see below)
        var pA = m_FaceA.transform.position;
        var pB = m_FaceB.transform.position;
        var mid = (pA + pB) / 2; // the average between the central coordinates of squares A and B, abbrieviate below as M
        var nA = m_FaceA.transform.up;
        var nB = m_FaceB.transform.up;

        var a2B = pB - pA;

        var m2J = pJoint - mid;

        var nJ2A = (pA - pJoint).normalized;
        var nJ2B = (pB - pJoint).normalized;
        transform.localPosition = Vector3.zero;

        // always pin the ends
        bones[0].position = pJoint + nJ2A * halfLength + nA * elevation;
        bones[0].up = -nJ2A;
        bones[4].position = pJoint + nJ2B * halfLength + nB * elevation;
        bones[4].up = nJ2B;

        // folded over, tricky case...
        if (a2B.sqrMagnitude < critical)
        {
            bones[1].position = pJoint + nA * elevation;
            bones[1].up = -nJ2A;
            bones[2].up = (nJ2B - nJ2A).normalized;
            bones[3].position = pJoint + nB * elevation;
            bones[3].up = nJ2B;
            bones[2].position = (bones[1].position + bones[3].position) / 2f + -nJ2A * elevation;
            return;
        }

        // significant fold angle to proceed with rest of the placement
        // this way the glowstick doesn't glitch at flat-ish angles
        var nJ2M = -m2J.normalized;
        var indicator = Vector3.Dot(nJ2M, nA);

        if (indicator > 0 || indicator < 0 && indicator < (critical - 1))
        {
            // folding inwards
            bones[1].position = pJoint + nJ2A * (elevation + squareRenderSettings.margin) + nA * elevation;
            bones[1].up = -nJ2A;
            bones[2].up = (nJ2B - nJ2A).normalized;
            bones[3].position = pJoint + nJ2B * (elevation + squareRenderSettings.margin) + nB * elevation;
            bones[3].up = nJ2B;
            bones[2].position = (bones[1].position + bones[3].position) / 2f;
        }
        else
        {
            // folding outwards
            bones[1].position = pJoint + nA * elevation;
            bones[1].up = -nJ2A;
            bones[2].up = (nJ2B - nJ2A).normalized;
            bones[3].position = pJoint + nB * elevation;
            bones[3].up = nJ2B;
            bones[2].position = (bones[1].position + bones[3].position) / 2f;
        }
    }
}