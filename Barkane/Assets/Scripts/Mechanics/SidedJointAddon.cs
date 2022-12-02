using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;

public abstract class SidedJointAddon : MonoBehaviour
{
    [SerializeField] protected bool jointSide; // true = side 1, false = side 2

    protected JointRenderer jr;

#if UNITY_EDITOR
    public bool SameSide(SidedJointAddon other) => jointSide == other.jointSide;
#endif

    internal JointGeometryData FetchGeometry()
    {
        if (jr == null) jr = transform.parent.GetComponentInChildren<JointRenderer>();
        return jointSide ? jr.side1Geometry : jr.side2Geometry;
    }
}

public interface IDynamicMesh<T> where T : IDynamicMeshRenderSettings
{
    void ClearAndInitBuffers(T settings);
}