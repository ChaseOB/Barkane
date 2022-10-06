using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BarkaneEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class JointRenderer : MonoBehaviour, IRefreshable
{
    /**
     * FOR THE CONTEXT OF JOINT VFX...
     * A and B always represent the identity of the square, A always across the joint from B, and vice versa
     * 1 and 2 always represent the side o fthe square, A1 and A2 are always on the same side of the joint (but different facing direction), same for B
     * Pair1 contains both A and B for side 1, Pair2 contains both A and B for side 2
     */

    // [SerializeField] SquareRenderSettings squareRenderSettings; // for referencing the margin property
    [SerializeField] CreaseRenderSettings settings;
    [SerializeField] SquareRenderSettings squareRenderSettings;

    [SerializeField] MeshFilter filter;
    [SerializeField] MeshRenderer meshRenderer;

    [SerializeField, HideInInspector] private SquareSide a1, a2, b1, b2;
    // base color for tile sides
    [SerializeField, HideInInspector] private Color colorA1, colorB1, colorA2, colorB2;

    private enum FoldState
    {
        Overlap,
        Coplanar,
        Orthogonal,
        NonAdjacent
    }

    [SerializeField, HideInInspector] private FoldState foldState = FoldState.NonAdjacent;

    /// <summary>
    /// Can be called manually in inspector or automatically by other scene editor utilities.
    /// </summary>
    /// <exception cref="UnityException"></exception>
    void IRefreshable.Refresh()
    {
        var parent = transform.parent.GetComponent<PaperJoint>();
        if (parent.PaperSqaures.Count < 2)
        {
            throw new UnityException($"Cannot refresh joints without enough adjacent squares: {parent.PaperSqaures.Count}");
        }

        a1 = parent.PaperSqaures[0].TopHalf.GetComponent<SquareSide>();
        a2 = parent.PaperSqaures[0].BottomHalf.GetComponent<SquareSide>();
        b1 = parent.PaperSqaures[1].TopHalf.GetComponent<SquareSide>();
        b2 = parent.PaperSqaures[1].BottomHalf.GetComponent<SquareSide>();

        if (CoordUtils.DiffAxisCount(a1, a2) != 0 || CoordUtils.DiffAxisCount(b1, b2) != 0)
        {
            throw new UnityException("Incorrect square side references! A paper square has sides on different coordinates.");
        }

        Debug.Log($"Joint with tiles at ({a1.Coordinate}, {b1.Coordinate})");

        if (a1 != null)
        {
            foldState = FormPairs(a1, a2, b1, b2);
        }
        else
            throw new UnityException("Cannot find square side reference in joint renderer parent");

        UpdateColors();
        UpdateGeometry();
    }

    public bool IsAnimating = false;

    void Update()
    {
        if (foldState == FoldState.NonAdjacent) return;

        if (IsAnimating && Application.isPlaying)
            UpdateGeometry();
    }

    /// <summary>
    /// Reorganize square side references based on side parity.
    /// </summary>
    /// <returns>Classification of the fold type based on number of coordinates different between A and B. Pairs are rearranged as side effect.</returns>
    /// <exception cref="UnityException"></exception>
    private FoldState FormPairs(SquareSide a1, SquareSide a2, SquareSide b1, SquareSide b2)
    {
        switch(CoordUtils.DiffAxisCount(a1, b1))
        {
            // for overlapping case, just compare if the normals are opposite
            case 0:
                // pair a1 with b1, a2 with b2
                if (CoordUtils.RoundEquals(a1.transform.up, -b1.transform.up))
                {
                }
                // the other pairing
                else if (CoordUtils.RoundEquals(a1.transform.up, -b2.transform.up))
                {
                    this.b1 = b2;
                    this.b2 = b1;
                }
                // invalid
                else throw new UnityException("Cannot pair a1 with anything! (Overlapping Case)");

                return FoldState.Overlap;
            
            // for coplanar case, just compare if the normals match up
            case 1:
                // pair a1 with b1, a2 with b2
                if (CoordUtils.RoundEquals(a1.transform.up, b1.transform.up))
                {

                }
                // the other pairing
                else if (CoordUtils.RoundEquals(a1.transform.up, b2.transform.up))
                {
                    this.b1 = b2;
                    this.b2 = b1;
                }
                // invalid
                else throw new UnityException("Can't pair a1 with anything! (Coplanar Case)");

                return FoldState.Coplanar;
            case 2:
                // for orthogonal case, compare if the "towards" direction is in the same way as the normal (for both sides)
                // this compares whether two sides are facing "inwards" or "outwards"
                var a2b = CoordUtils.AsV(CoordUtils.FromTo(a1, b1));
                var b2a = -a2b;
                
                // pair a1 with b1, a2 with b2
                if (Mathf.Sign(Vector3.Dot(a1.transform.up, a2b)) == Mathf.Sign(Vector3.Dot(b1.transform.up, b2a)))
                {

                }
                // the other pairing
                else if (Mathf.Sign(Vector3.Dot(a1.transform.up, a2b)) == Mathf.Sign(Vector3.Dot(b1.transform.up, a2b)))
                {
                    this.b1 = b2;
                    this.b2 = b1;
                }
                // invalid
                else throw new UnityException("Can't pair a1 with anything! (Orthogonal Case)");

                return FoldState.Orthogonal;
            
            // somehow there are more than 3 axis different in a 3D space..?
            default:
                // this includes the possible 3 case when non of the coordinates are equal (the tiles aren't even adjacent!)
                throw new UnityException($"Joint { transform.parent } contains squares that aren't adjacent!");
        }
    }

    private void UpdateColors()
    {
        colorA1 = a1.EdgeTintedColor(settings.tintCorrection);
        colorA2 = a2.EdgeTintedColor(settings.tintCorrection);
        colorB1 = b1.EdgeTintedColor(settings.tintCorrection);
        colorB2 = b2.EdgeTintedColor(settings.tintCorrection);

        Debug.Log($"... colors: ({colorA1}, {colorB1}), ({colorA2}, {colorB2})");
    }

    private static readonly float squareSize = 2;

    private void OnDrawGizmosSelected()
    {
        if (filter.sharedMesh != null)
        {
            var vertices = filter.sharedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Handles.Label(transform.localToWorldMatrix.MultiplyPoint3x4(vertices[i] + filter.sharedMesh.normals[i] * 0.1f), i.ToString());
            }
        }
    }

    /// <summary>
    /// Update Joint appearance when the physical location/orientation of either side changes
    /// </summary>
    private void UpdateGeometry()
    {
        // lock to world space orientation
        transform.rotation = Quaternion.identity;

        var creaseNorm = Vector3.zero; // direction orthogonal to the crease itself, on the plane of the crease
        var creaseTangent = Vector3.zero; // direction along the crease
        var creaseBitangent = Vector3.zero;

        var toA = a1.transform.parent.position - transform.position;
        var toB = b1.transform.parent.position - transform.position;
        var aToB = b1.transform.parent.position - a1.transform.parent.position;

        var a1Up = a1.transform.up;
        var b1Up = b1.transform.up;
        var a2Up = a2.transform.up;
        var b2Up = b2.transform.up;

        // initialize creaseNorm and creaseTangent
        switch (foldState)
        {
            // norm goes inwards from the joint to the center of the overlapping tiles (i.e. the center of any one of them)
            case FoldState.Overlap:
                creaseNorm = toA.normalized;
                // the a2b vector is 0 in this case, we need another way to find crease tangent
                creaseTangent = Vector3.Cross(toA, a1Up).normalized;
                break;

            // norm follows the upwards direction of any tile side
            // since the crease deforms both +y and -y, there's no need to pick a particular side
            case FoldState.Coplanar:
                creaseNorm = a1.transform.up;
                creaseTangent = Vector3.Cross(aToB, a1Up).normalized;
                break;

            // norm is the average between the two upward directions of any pair
            // the first pair is picked just as convention, it doesn't guarantee the norm will face inward
            case FoldState.Orthogonal:
                creaseNorm = (a1Up + b1Up).normalized;
                creaseTangent = Vector3.Cross(aToB, a1Up).normalized;
                break;

            // invalid
            case FoldState.NonAdjacent:
                throw new UnityException("Cannot create geometry for non-adjacent tiles across a joint");
        }

        if (creaseNorm.sqrMagnitude < .1f || creaseTangent.sqrMagnitude < .1f)
            throw new UnityException("Crease normal cannot be initialized due to non-adjacent tiles");
        creaseBitangent = Vector3.Cross(creaseNorm, creaseTangent);

        Debug.Log($"... norm: {creaseNorm}, tangent: {creaseTangent}");

        // time points (t in 0~1) for both anchors and pivots
        // anchors are along the side of the tile faces
        var ts = new float[settings.creaseSegmentCount + 1];
        for (int i = 0; i < settings.creaseSegmentCount + 1; i++)
        {
            ts[i] = i / (float)settings.creaseSegmentCount;
        }
        ts[settings.creaseSegmentCount] = 1;

        // displacement for each pivot point along the crease normal
        var ys = new float[settings.creaseSegmentCount + 1];
        var xs = new float[settings.creaseSegmentCount + 1];
        var zs = new float[settings.creaseSegmentCount + 1];
        for (int i = 0; i < settings.creaseSegmentCount + 1; i++)
        {
            ys[i] = 2 * (Random.value - 0.5f) * settings.creaseDeviation.y;
            xs[i] = 2 * (Random.value - 0.5f) * settings.creaseDeviation.x;
            zs[i] = 2 * (Random.value - 0.5f) * settings.creaseDeviation.z;
        }

        // the pivots are duplicated
        var verts = new Vector3[8 * (settings.creaseSegmentCount + 1)];
        // not actually submeshes, just mentally think of each side of each tile as a submesh
        // the order of the meshes follow the (a, b) (a, b) ordering of the pairs
        var submeshOffset = 2 * (settings.creaseSegmentCount + 1);
        var pivotOffset = settings.creaseSegmentCount + 1;

        var norms = new Vector3[verts.Length];
        var colors = new Color[verts.Length];
        var uvs = new Vector2[verts.Length];

        if (filter.sharedMesh == null)
        {
            filter.sharedMesh = new Mesh()
            {
                name = "Joint Mesh"
            };
        }

        var creaseNorm1 = Vector3.Dot(creaseNorm, a1Up) > 0 ? creaseNorm : -creaseNorm;
        var creaseNorm2 = -creaseNorm1;

        var scaledSquareSize = squareSize * (1 - squareRenderSettings.margin);

        var toAN = toA.normalized;
        var toBN = toB.normalized;

        for (int i = 0; i < settings.creaseSegmentCount + 1; i++)
        {
            #region vertex filling
            var t = ts[i] - 0.5f;
            var pivotBase = t * scaledSquareSize * creaseTangent;
            
            // note that the margin is also affected by the size setting
            // the margin applies to a 01 (uv) square which is sized to produce the actual square
            verts[i] = pivotBase + squareRenderSettings.margin * toAN + a1Up * 0.0005f;
            verts[i + submeshOffset] = pivotBase + squareRenderSettings.margin * toBN + b1Up * 0.0005f;
            verts[i + 2 * submeshOffset] = pivotBase + squareRenderSettings.margin * toAN + a2Up * 0.0005f;
            verts[i + 3 * submeshOffset] = pivotBase + squareRenderSettings.margin * toBN + b2Up * 0.0005f;

            // the same pivots are duplicated to avoid color bleeding
            verts[i + pivotOffset] = pivotBase + (i == 0 || i == settings.creaseSegmentCount ? 0 : 1) * zs[i] * creaseTangent + ys[i] * creaseNorm + xs[i] * toAN;
            verts[i + pivotOffset + submeshOffset] = pivotBase + (i == 0 || i == settings.creaseSegmentCount ? 0 : 1) * zs[i] * creaseTangent + ys[i] * creaseNorm + xs[i] * toAN;
            verts[i + pivotOffset + 2 * submeshOffset] = pivotBase + (i == 0 || i == settings.creaseSegmentCount ? 0 : 1) * zs[i] * creaseTangent + ys[i] * creaseNorm + xs[i] * toAN;
            verts[i + pivotOffset + 3 * submeshOffset] = pivotBase + (i == 0 || i == settings.creaseSegmentCount ? 0 : 1) * zs[i] * creaseTangent + ys[i] * creaseNorm + xs[i] * toAN;
            #endregion

            #region normals filling

            // note that the margin is also affected by the size setting
            // the margin applies to a 01 (uv) square which is sized to produce the actual square
            norms[i] = a1Up;
            norms[i + submeshOffset] = b1Up;
            norms[i + 2 * submeshOffset] = a2Up;
            norms[i + 3 * submeshOffset] = b2Up;

            // all pivots point to crease normal
            norms[i + pivotOffset] = creaseNorm1;
            norms[i + pivotOffset + submeshOffset] = creaseNorm1;
            norms[i + pivotOffset + 2 * submeshOffset] = creaseNorm2;
            norms[i + pivotOffset + 3 * submeshOffset] = creaseNorm2;
            #endregion

            #region colors filling

            // note that the margin is also affected by the size setting
            // the margin applies to a 01 (uv) square which is sized to produce the actual square
            colors[i] = colorA1;
            colors[i + submeshOffset] = colorB1;
            colors[i + 2 * submeshOffset] = colorA2;
            colors[i + 3 * submeshOffset] = colorB2;

            // the same pivots are duplicated to avoid color bleeding
            colors[i + pivotOffset] = colorA1;
            colors[i + pivotOffset + submeshOffset] = colorB1;
            colors[i + pivotOffset + 2 * submeshOffset] = colorA2;
            colors[i + pivotOffset + 3 * submeshOffset] = colorB2;
            #endregion

            #region uvs filling

            // uvx always measure how "deep" in the cease it is (1 at the pivot, 1 - margin at the anchors)
            // uvy always link to t itself
            var uvSide = new Vector2(1 - squareRenderSettings.margin, ts[i]);
            var uvCenter = new Vector2(1, ts[i]);
            uvs[i] = uvSide;
            uvs[i + submeshOffset] = uvSide;
            uvs[i + 2 * submeshOffset] = uvSide;
            uvs[i + 3 * submeshOffset] = uvSide;

            // the same pivots are duplicated to avoid color bleeding
            uvs[i + pivotOffset] = uvCenter;
            uvs[i + pivotOffset + submeshOffset] = uvCenter;
            uvs[i + pivotOffset + 2 * submeshOffset] = uvCenter;
            uvs[i + pivotOffset + 3 * submeshOffset] = uvCenter;
            #endregion
        }

        // 3 points per triangle
        // 4 stripes per mesh
        // 2 triangles per each segment on each stripe
        var tris = new int[3 * 4 * 2 * settings.creaseSegmentCount];
        var triOffset = 3 * 2 * settings.creaseSegmentCount;

        var useCCW = Vector3.Dot(Vector3.Cross(verts[pivotOffset] - verts[0], verts[1] - verts[0]), a1Up) < 0;

        if (useCCW)
        {
            for (int i = 0, j = 0; i < settings.creaseSegmentCount; i++, j += 3 * 2)
            {
                tris[j] = i;
                tris[j + 1] = i + 1;
                tris[j + 2] = i + pivotOffset;
                tris[j + 3] = i + 1;
                tris[j + 4] = i + pivotOffset + 1;
                tris[j + 5] = i + pivotOffset;

                tris[triOffset + j] = i + submeshOffset;
                tris[triOffset + j + 1] = i + submeshOffset + pivotOffset;
                tris[triOffset + j + 2] = i + submeshOffset + 1;
                tris[triOffset + j + 3] = i + submeshOffset + 1;
                tris[triOffset + j + 4] = i + submeshOffset + pivotOffset;
                tris[triOffset + j + 5] = i + submeshOffset + pivotOffset + 1;

                tris[2 * triOffset + j] = i + 2 * submeshOffset;
                tris[2 * triOffset + j + 1] = i + 2 * submeshOffset + pivotOffset;
                tris[2 * triOffset + j + 2] = i + 2 * submeshOffset + 1;
                tris[2 * triOffset + j + 3] = i + 2 * submeshOffset + 1;
                tris[2 * triOffset + j + 4] = i + 2 * submeshOffset + pivotOffset;
                tris[2 * triOffset + j + 5] = i + 2 * submeshOffset + pivotOffset + 1;

                tris[3 * triOffset + j] = i + 3 * submeshOffset;
                tris[3 * triOffset + j + 1] = i + 3 * submeshOffset + 1;
                tris[3 * triOffset + j + 2] = i + 3 * submeshOffset + pivotOffset;
                tris[3 * triOffset + j + 3] = i + 3 * submeshOffset + 1;
                tris[3 * triOffset + j + 4] = i + 3 * submeshOffset + pivotOffset + 1;
                tris[3 * triOffset + j + 5] = i + 3 * submeshOffset + pivotOffset;
            }
        } else
        {
            for (int i = 0, j = 0; i < settings.creaseSegmentCount; i++, j += 3 * 2)
            {
                tris[j] = i;
                tris[j + 1] = i + pivotOffset;
                tris[j + 2] = i + 1;
                tris[j + 3] = i + 1;
                tris[j + 4] = i + pivotOffset;
                tris[j + 5] = i + pivotOffset + 1;

                tris[triOffset + j] = i + submeshOffset;
                tris[triOffset + j + 1] = i + submeshOffset + 1;
                tris[triOffset + j + 2] = i + submeshOffset + pivotOffset;
                tris[triOffset + j + 3] = i + submeshOffset + 1;
                tris[triOffset + j + 4] = i + submeshOffset + pivotOffset + 1;
                tris[triOffset + j + 5] = i + submeshOffset + pivotOffset;

                tris[2 * triOffset + j] = i + 2 * submeshOffset;
                tris[2 * triOffset + j + 1] = i + 2 * submeshOffset + 1;
                tris[2 * triOffset + j + 2] = i + 2 * submeshOffset + pivotOffset;
                tris[2 * triOffset + j + 3] = i + 2 * submeshOffset + 1;
                tris[2 * triOffset + j + 4] = i + 2 * submeshOffset + pivotOffset + 1;
                tris[2 * triOffset + j + 5] = i + 2 * submeshOffset + pivotOffset;

                tris[3 * triOffset + j] = i + 3 * submeshOffset;
                tris[3 * triOffset + j + 1] = i + 3 * submeshOffset + pivotOffset;
                tris[3 * triOffset + j + 2] = i + 3 * submeshOffset + 1;
                tris[3 * triOffset + j + 3] = i + 3 * submeshOffset + 1;
                tris[3 * triOffset + j + 4] = i + 3 * submeshOffset + pivotOffset;
                tris[3 * triOffset + j + 5] = i + 3 * submeshOffset + pivotOffset + 1;
            }
        }
        
        // Debug.Log(string.Join("\n", verts));
        // Debug.Log(string.Join(", ", tris));

        var m = filter.sharedMesh;
        m.vertices = verts;
        m.normals = norms;
        m.colors = colors;
        m.uv = uvs;
        m.triangles = tris;
    }
}