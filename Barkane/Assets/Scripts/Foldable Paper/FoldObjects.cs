using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;

public class FoldObjects {
    public List<GameObject> foldSquares; //C: every square being folded
    public List<GameObject> foldJoints; //C: the non-line joints being folded
    public List<GameObject> foldLineJoints; //C: joints along the fold line
    public Transform squareParent;
    public Transform jointParent;

    public List<PaperSquare> PaperSquaresCache;

    public Dictionary<Vector3Int, OcclusionQueue> OcclusionMap = new Dictionary<Vector3Int, OcclusionQueue>();

    public FoldObjects() {
        foldSquares = new List<GameObject>();
        foldJoints = new List<GameObject>();
        foldLineJoints = new List<GameObject>();
    }

    public FoldObjects(Transform sp, Transform jp) {
        foldSquares = new List<GameObject>();
        foldJoints = new List<GameObject>();
        foldLineJoints = new List<GameObject>();
        squareParent = sp;
        jointParent = jp;
    }

    public void EnableJointMeshes()
    {
        foreach(GameObject go in foldLineJoints)
        {
            PaperJoint pj = go.GetComponent<PaperJoint>();
            JointRenderer jr = pj?.JointRenderer;
            jr?.EnableMeshAction();
            jr?.ShowLine(false, false);
        }
    }

    public void DisableJointMeshes()
    {
        foreach(GameObject go in foldLineJoints)
        {
            PaperJoint pj = go.GetComponent<PaperJoint>();
            JointRenderer jr = pj?.JointRenderer;
            jr?.DisableMeshAction();
            jr?.ShowLine(true);
        }
    }

    public void OnFoldHighlight(bool select)
    {
        foreach (GameObject go in foldSquares)
            go.GetComponent<PaperSquare>().OnFoldHighlight(select);
    }

    //foldStart is true when starting a fold and false when ending a fold
    public void OnFold(bool foldStart)
    {
        
    }

    public Vector3 CalculateCenter()
    {
        List<Vector3> vectors = new List<Vector3>();
        foreach(GameObject ps in foldSquares){
            vectors.Add(ps.transform.position);
        }
        return CoordUtils.CalculateCenter(vectors);
    }

    public void MergeWithGlobalOcclusionMap(Dictionary<Vector3Int, OcclusionQueue> globalMap, Matrix4x4 finalTransform)
    {
        foreach (var (local, oq) in OcclusionMap)
        {
            var corresponding = Vector3Int.RoundToInt(finalTransform.MultiplyPoint(local));
            var correspondingUp = Vector3Int.RoundToInt(finalTransform.MultiplyVector(oq.upwards));

            var alignedToNegative = correspondingUp.x < 0 || correspondingUp.y < 0 || correspondingUp.z < 0;

            // matching position and direction
            if (globalMap.ContainsKey(corresponding))
            {
                // figure out direction of the merge, 
                globalMap[corresponding].MergeAndDispose(alignedToNegative ? oq.MakeFlippedCopy() : oq);
            } else
            {
                // insert local entry directly into global entry
                // flip to always using positive direction, this isn't really necessary but is good for convention
                globalMap.Add(corresponding, alignedToNegative ? oq.MakeFlippedCopy() : oq);
            }
        }
    }

    public void TransferToLocalOcclusionMap()
    {
        OcclusionMap.Clear();
        PaperSquaresCache = new List<PaperSquare>();
        foreach (GameObject ps in foldSquares)
        {
            PaperSquaresCache.Add(ps.GetComponent<PaperSquare>());
        }

        foreach (var ps in PaperSquaresCache)
        {
            ps.EjectFromGlobalQueue();
            var center = Vector3Int.RoundToInt(ps.transform.localPosition);
            if (OcclusionMap.ContainsKey(center))
            {
                var q = OcclusionQueue.MakeOcclusionQueue(center);
                if (q == null)
                {
                    throw new UnityException("Local occlusion map entry could not be created");
                }
                else
                {
                    q.Enqueue(ps);
                }
            }
            else
            {
                OcclusionMap[center].Enqueue(ps);
            }
        }
    }
}