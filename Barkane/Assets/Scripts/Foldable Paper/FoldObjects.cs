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

    public void TransferToLocalOcclusionMap(Matrix4x4 encode)
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

            Debug.DrawRay(
                encode.inverse.MultiplyPoint(OcclusionMap[center].center),
                encode.inverse.MultiplyVector(OcclusionMap[center].upwards),
                Color.cyan, 6);
        }

        // Debug.Log(OcclusionMap);
    }


    public void MergeWithGlobalOcclusionMap(
        OcclusionMap globalMap, Vector3 worldSpaceRotationRoot, Func<float, (Matrix4x4 encoder, Matrix4x4 decoder)> replay)
    {
        // var (encoder0, decoder0) = replay(0);
        var (encoderNear, decoderNear) = replay(0.9f);
        var (encoder1, decoder1) = replay(1);

        foreach (var (local, oq) in OcclusionMap)
        {
            var worldSpacePos = Vector3Int.RoundToInt(decoder1.MultiplyPoint(local));
            var worldSpaceUp = Vector3Int.RoundToInt(decoder1.MultiplyVector(oq.upwards));

            var alignedToNegative = worldSpaceUp.x < 0 || worldSpaceUp.y < 0 || worldSpaceUp.z < 0;

            // Matching position and direction
            if (globalMap.ContainsKey(worldSpacePos))
            {
                // The resolution strategy comes from the fact that joints have width
                // This means always insert new things "inward" w.r.t. rotation radial dir.
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

                var radial = worldSpacePos - worldSpaceRotationRoot;
                var matchingFactor = Vector3.Dot(radial, worldSpaceUp);
                var approachFromPositive = matchingFactor < -0.05f;

                // When matchingFactor is 0, it means radial is orthogonal to positive and we have an ambiguous case
                // The resolution for that can be figured out by rewinding the animation by a bit so that everything
                // is a little tilted.

                if (!approachFromPositive && matchingFactor < 0.05f)
                {
                    var nearEndPosition = decoderNear.MultiplyPoint(local);
                    var nearEndRadial = nearEndPosition - worldSpaceRotationRoot;
                    var nearEndMatchingFactor = Vector3.Dot(nearEndRadial, worldSpaceUp);

                    approachFromPositive = nearEndMatchingFactor < 0f;
                }

                Debug.Log($"approach from: {(approachFromPositive ? "+" : "-")} should flip: {(alignedToNegative ? "true":"false")}");

                if (approachFromPositive)
                {
                    // When approaching from positive, the new tiles (contents of the local occlusion map) covers the old tiles
                    // This means they come *after* the original items in the merged queue
                    globalMap[worldSpacePos].MergeApproachingFromPositive(
                        alignedToNegative ? oq.MakeFlippedCopy() : oq);
                }
                else
                {
                    // Coming from a local occlusion map content goes *before* the global content
                    globalMap[worldSpacePos].MergeApproachingFromNegative(
                        alignedToNegative ? oq.MakeFlippedCopy() : oq);
                }

                globalMap[worldSpacePos].UseAsGlobal();

                // Debug.Log($"Merge with global: {globalMap[corresponding]}");
            }
            else
            {
                // Insert local entry directly into global entry
                // Flip to always using positive direction
                globalMap[worldSpacePos] = alignedToNegative ? oq.MakeFlippedCopy() : oq;
                globalMap[worldSpacePos].UseAsGlobal();

                // Debug.Log("Insert direct");
            }

            globalMap[worldSpacePos].UpdateSpace(
                alignedToNegative ? -worldSpaceUp : worldSpaceUp,
                worldSpacePos,
                OcclusionQueue.Identity);

            Debug.DrawRay(globalMap[worldSpacePos].center, globalMap[worldSpacePos].upwards * 2, Color.magenta, 3);
        }

        Debug.Log(globalMap);
    }
}