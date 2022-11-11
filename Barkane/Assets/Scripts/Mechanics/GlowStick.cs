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

#if UNITY_EDITOR
    public void SetFaces((GameObject, GameObject) one, (GameObject, GameObject) two)
    {
        (m_FaceA, m_FaceB) = jointSide ? one : two;
        if (bones.Length != 4 || bones.Contains(null))
            throw new UnityException("Bones Not Updated Correctly");
    }

    public bool SameSide(GlowStick other) => jointSide == other.jointSide;
#endif

    private void Update()
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
        var nA = m_FaceA.transform.up;
        var nB = m_FaceB.transform.up;

        var nJ2A = (pA - pJoint).normalized;
        var nJ2B = (pB - pJoint).normalized;
        transform.localPosition = Vector3.zero;

        // always pin the starting tip
        bones[0].position = pJoint + nJ2A * halfLength + nA * elevation;
        bones[0].up = -nJ2A;

        // decide midpoint and ending tip depending on folding case
        var tJ = Vector3.Cross(nA, -nJ2A); // tangent along joint
        var angFold = Vector3.SignedAngle(nJ2A, nJ2B, tJ);
        var nJ = Quaternion.AngleAxis(angFold / 2f, tJ) * -nJ2A;

        if (angFold > 5f && angFold < 89f)
            throw new UnityException("Cannot fold inwards");

        // folding outwards (270 degrees ~ 0 degrees, plus some margin)
        if (angFold > -89f && angFold < 5f)
        {
            // folding beyond 270 degrees
            bones[1].position = pJoint + nA * elevation;
            bones[2].position = pJoint + nJ * elevation;
            bones[3].position = pJoint + nB * elevation;
            bones[1].up = (bones[2].position - bones[1].position).normalized;
            bones[2].up = (bones[3].position - bones[2].position).normalized;
            bones[3].up = nJ2B;
        }
        // folding outwards (180 ~ 270 degrees)
        else if (angFold <= -89f)
        {
            bones[1].position = pJoint + nA * elevation;
            bones[2].position = pJoint + nJ * elevation;
            bones[3].position = pJoint + nB * elevation;
            bones[1].up = (bones[2].position - bones[1].position).normalized;
            bones[2].up = (bones[3].position - bones[2].position).normalized;
            bones[3].up = nJ2B;
        }
        // folding inwards
        else
        {
            bones[1].position = pJoint + nA * elevation + nJ2A * (elevation + squareRenderSettings.margin);
            bones[3].position = pJoint + nB * elevation + nJ2B * (elevation + squareRenderSettings.margin);
            bones[2].position = (bones[1].position + bones[3].position) / 2f;
            bones[1].up = (bones[2].position - bones[1].position).normalized;
            bones[2].up = (bones[3].position - bones[1].position).normalized;
            bones[3].up = nJ2B;
        }
    }
}