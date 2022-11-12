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
    [SerializeField] private Transform[] bones;
    [SerializeField] private Transform bonesEnd;

    [SerializeField] private GameObject m_FaceA, m_FaceB;

    [SerializeField] private SquareRenderSettings squareRenderSettings;

    public void Start()
    {
        if (Application.isPlaying)
        {
            // actively pull from parent
            PullFaces();
        }
        // calibrateLength = (bonesEnd.position - bones[3].position).magnitude;
        // Debug.Log(calibrateLength);
    }

    private void PullFaces()
    {
        var jr = transform.parent.GetComponentInChildren<JointRenderer>();
        SetFaces(jr.facePairs);
    }

#if UNITY_EDITOR
    public void SetFaces(((GameObject, GameObject), (GameObject, GameObject)) pairs)
    {
        var (one, two) = pairs;
        (m_FaceA, m_FaceB) = jointSide ? one : two;
        if (bones.Length != 4 || bones.Contains(null))
            throw new UnityException("Bones Not Updated Correctly");
    }

    public bool SameSide(GlowStick other) => jointSide == other.jointSide;
#endif

    private void LateUpdate()
    {
        if (m_FaceA == null || m_FaceB == null)
        {
            if (Application.isPlaying)
            {
                try
                {
                    PullFaces();
                } catch (System.Exception)
                {
                    Debug.LogError("glowstick faces not set appropriately");
                }
            }
        } else
        {
            UpdateBones();
        }
    }

    public float angFold;

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
        angFold = Vector3.SignedAngle(nJ2A, nJ2B, tJ);
        var nJ = Quaternion.AngleAxis(angFold / 2f, tJ) * -nJ2A;

        if (angFold >= 10f && angFold < 80f)
            throw new UnityException("Cannot fold inwards");

        // folding outwards (270 degrees ~ 0 degrees, plus some margin)
        if (angFold > -80f && angFold < 10f)
        {
            // folding beyond 270 degrees
            bones[1].position = pJoint + nA * elevation;
            bones[2].position = pJoint + nJ * (elevation + squareRenderSettings.margin);
            bones[3].position = pJoint + nB * elevation;
            bones[3].localScale = new Vector3(1, halfLength / calibrateLength, 1);
            bones[1].up = (bones[2].position - bones[1].position).normalized;
            bones[2].up = (bones[3].position - bones[2].position).normalized;
            bones[3].up = nJ2B;
        }
        // folding outwards (180 ~ 270 degrees)
        else if (angFold <= -80f)
        {
            bones[1].position = pJoint + nA * elevation;
            bones[2].position = pJoint + nJ * elevation;
            bones[3].position = pJoint + nB * elevation;
            bones[3].localScale = new Vector3(1, halfLength / calibrateLength, 1);
            bones[1].up = (bones[2].position - bones[1].position).normalized;
            bones[2].up = (bones[3].position - bones[2].position).normalized;
            bones[3].up = nJ2B;
        }
        // folding inwards
        else
        {
            bones[1].position = pJoint + nA * elevation + nJ2A * (elevation + squareRenderSettings.margin);
            bones[3].position = pJoint + nB * elevation + nJ2B * (elevation + squareRenderSettings.margin);
            bones[3].localScale = new Vector3(1, (halfLength - elevation - squareRenderSettings.margin) / calibrateLength, 1);
            bones[2].position = (bones[1].position + bones[3].position) / 2f;
            bones[1].up = (bones[2].position - bones[1].position).normalized;
            bones[2].up = (bones[3].position - bones[1].position).normalized;
            bones[3].up = nJ2B;
        }
    }
}