using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;
using System;

public class FoldObjects {
    public List<GameObject> foldSquares; //C: every square being folded
    public List<GameObject> foldJoints; //C: the non-line joints being folded
    public List<GameObject> foldLineJoints; //C: joints along the fold line
    public Transform squareParent;
    public Transform jointParent;

    public List<PaperSquare> PaperSquaresCache;

    public OcclusionMap OcclusionMap = new OcclusionMap();

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

    public void TransferToLocalOcclusionMap(Matrix4x4 encode, Matrix4x4 decode)
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
            var center = Vector3Int.RoundToInt(encode.MultiplyPoint(ps.transform.position));

            if (!OcclusionMap.ContainsKey(center))
            {
                // use constant encoder factory as local structure does not change during fold
                var q = OcclusionQueue.MakeOcclusionQueue(center, ()=>encode);
                if (q == null)
                    throw new UnityException($"Local occlusion map entry could not be created { center }");
                else
                    q.Enqueue(ps);
                OcclusionMap[center] = q;
            }
            else
                OcclusionMap[center].Enqueue(ps);

            //Debug.DrawRay(
            //    decode.MultiplyPoint(OcclusionMap[center].Offset),
            //    decode.MultiplyVector(OcclusionMap[center].SpatialBasis.normal),
            //    Color.cyan, 6);
        }

        // Debug.Log(OcclusionMap);
    }


    public void MergeWithGlobalOcclusionMap(
        OcclusionMap globalMap, Vector3 worldSpaceRotationRoot, Vector3 ccwAxis, Func<float, (Matrix4x4 encoder, Matrix4x4 decoder)> replay)
    {
        var (encoder1, decoder1) = replay(1);

        foreach (var (local, oq) in OcclusionMap)
        {
            var worldSpacePos = Vector3Int.RoundToInt(decoder1.MultiplyPoint(local));
            var worldSpaceUp = Vector3Int.RoundToInt(decoder1.MultiplyVector(oq.SpatialBasis.normal));

            var alignedToNegative = worldSpaceUp.x < 0 || worldSpaceUp.y < 0 || worldSpaceUp.z < 0;

            // Matching position and direction
            if (globalMap.ContainsKey(worldSpacePos))
            {
                bool approachFromPositive;
                var radial = worldSpacePos - worldSpaceRotationRoot;
                var matchingFactor = Vector3.Dot(radial, worldSpaceUp);
                if (Mathf.Abs(matchingFactor) < 0.05f)
                {
                    // Case where the tile is on the folding plane
                    approachFromPositive = Vector3.Dot(Vector3.Cross(radial, ccwAxis), alignedToNegative ? -worldSpaceUp : worldSpaceUp) > 0;
                } else
                {
                    // Inambiguous case where the tile has some slanted radial direction against the rotation center
                    // Always insert new things "inward" w.r.t. rotation radial dir.
                    // ex.
                    //     __
                    //      |
                    // |    |
                    // |____| --> +x
                    // ^ existing wall stays outward
                    //  _____
                    // ||   |
                    // |____| --> +x
                    //  ^ new wall comes inward
                    // 
                    // ... in this case the new wall is approaching from positive, since it's radial is facing -x

                    // Obviously, in real life you can have the top flap going outwards *or* inwards, but
                    // here we want to simply it to only one case

                    // Note the inward direction is always offsetted *against* the radial
                    // Since axis is always on an offsetted joint, the distance to the new tile center is always nonzero
                    // This means we won't get a zero vector and the dot product is usable
                    approachFromPositive = matchingFactor < 0;
                }

                // Debug.Log($"approach from: {(approachFromPositive ? "+" : "-")} { local } -> { worldSpacePos } should flip: {(alignedToNegative ? "true":"false")}");

                if (approachFromPositive)
                    // When approaching from positive, the new tiles (contents of the local occlusion map) covers the old tiles
                    // This means they come *after* the original items in the merged queue
                    globalMap[worldSpacePos].MergeApproachingFromPositive(
                        alignedToNegative ? oq.MakeFlippedCopy() : oq);
                else
                    // Coming from a local occlusion map content goes *before* the global content
                    globalMap[worldSpacePos].MergeApproachingFromNegative(
                        alignedToNegative ? oq.MakeFlippedCopy() : oq);
            }
            else
            {
                // Insert local entry directly into global entry
                // Flip to always using positive direction
                globalMap[worldSpacePos] = alignedToNegative ? oq.MakeFlippedCopy() : oq;
            }

            globalMap[worldSpacePos].UpdateSpace(
                alignedToNegative ? -worldSpaceUp : worldSpaceUp,
                worldSpacePos,
                OcclusionQueue.IdentityEncoder);
            globalMap[worldSpacePos].UseAsGlobal();

            // Debug.DrawRay(globalMap[worldSpacePos].center, globalMap[worldSpacePos].upwards * 2, Color.magenta, 3);
        }

        // Debug.Log(globalMap);
    }
}