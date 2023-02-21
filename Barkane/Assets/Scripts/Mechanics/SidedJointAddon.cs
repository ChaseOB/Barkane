using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;
using UnityEngine.Rendering;

public abstract class SidedJointAddon : MonoBehaviour
{
    [SerializeField] protected bool jointSide; // true = side 1, false = side 2

    protected JointRenderer jr;

#if UNITY_EDITOR
    public bool SameSide(SidedJointAddon other) => jointSide == other.jointSide;
#endif

    internal (JointGeometryData, JointGeometryData.JointSideGeometryData) FetchGeometry()
    {
        if (jr == null) jr = transform.parent.GetComponentInChildren<JointRenderer>();
        return jointSide ? (jr.jointGeometry, jr.jointGeometry1) : (jr.jointGeometry, jr.jointGeometry2);
    }

    public const MeshUpdateFlags fQuiet =
        MeshUpdateFlags.DontValidateIndices
        | MeshUpdateFlags.DontResetBoneBounds
        | MeshUpdateFlags.DontNotifyMeshUsers
        | MeshUpdateFlags.DontRecalculateBounds;

    public const MeshUpdateFlags fConsiderBounds =
        MeshUpdateFlags.DontValidateIndices
        | MeshUpdateFlags.DontResetBoneBounds;
}

public interface IDynamicMesh<T> where T : IDynamicMeshRenderSettings
{
    void ClearAndInitBuffers(T settings);
}