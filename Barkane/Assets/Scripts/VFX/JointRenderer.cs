using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BarkaneEditor;

namespace BarkaneJoint
{
    [ExecuteAlways]
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

        public ((GameObject, GameObject), (GameObject, GameObject)) facePairs => ((a1.gameObject, b1.gameObject), (a2.gameObject, b2.gameObject));
        [SerializeField, HideInInspector] private SquareSide a1, a2, b1, b2;
        // base color for tile sides
        [SerializeField, HideInInspector] private Color colorA1, colorB1, colorA2, colorB2;

        [SerializeField, HideInInspector] private Vector3[] randoms;

        // time points (t in 0~1) for both anchors and pivots
        // anchors are along the side of the tile faces
        [SerializeField, HideInInspector] private float[] ts;

        [SerializeField] private GameObject indicator;
        [SerializeField] private MaskFoldParticles maskFoldParticles;

        internal JointGeometryData side1Geometry, side2Geometry;

        // buffers
        Vector3[] verts, norms;
        Color[] colors;
        Vector2[] uvs;

        /// <summary>
        /// Can be called manually in inspector or automatically by other scene editor utilities.
        /// </summary>
        /// <exception cref="UnityException"></exception>
        void IRefreshable.Refresh()
        {
            var parent = transform.parent.GetComponent<PaperJoint>();
            if (parent.PaperSquares.Count < 2)
            {
                throw new UnityException($"Cannot refresh joints without enough adjacent squares: {parent.PaperSquares.Count}");
            }

            a1 = parent.PaperSquares[0].TopHalf.GetComponent<SquareSide>();
            a2 = parent.PaperSquares[0].BottomHalf.GetComponent<SquareSide>();
            b1 = parent.PaperSquares[1].TopHalf.GetComponent<SquareSide>();
            b2 = parent.PaperSquares[1].BottomHalf.GetComponent<SquareSide>();

            if (CoordUtils.DiffAxisCount(a1, a2) != 0 || CoordUtils.DiffAxisCount(b1, b2) != 0)
            {
                throw new UnityException("Incorrect square side references! A paper square has sides on different coordinates.");
            }

            if (a1 != null)
            {
                FormPairs(a1, a2, b1, b2);
            }
            else
                throw new UnityException("Cannot find square side reference in joint renderer parent");

            ts = new float[settings.creaseSegmentCount + 1];
            for (int i = 0; i < settings.creaseSegmentCount + 1; i++)
            {
                ts[i] = i / (float)settings.creaseSegmentCount;
            }
            ts[settings.creaseSegmentCount] = 1;

            randoms = new Vector3[settings.creaseSegmentCount + 1];

            for (int i = 0; i < settings.creaseSegmentCount + 1; i++)
            {
                randoms[i] = new Vector3(
                    2 * (Random.value - 0.5f) * settings.creaseDeviation.x,
                    2 * (Random.value - 0.5f) * settings.creaseDeviation.y,
                    2 * (Random.value - 0.5f) * settings.creaseDeviation.z);
            }

            UpdateColors();
            UpdateGlowstick();
            RefreshBuffers();
        }

        public bool IsAnimating = false;

        public System.Action DisableMeshAction => new System.Action(delegate() {
           // indicator.SetActive(false);
            // meshRenderer.enabled = false;
        });

        public System.Action EnableMeshAction => new System.Action(delegate ()
        {
            //indicator.SetActive(true);
            // FormPairs(a1, a2, b1, b2);
            // colors stay the same across folds
            // UpdateColors();
            // meshRenderer.enabled = true;
        });

        void Update()
        {
            if (a1 == null || a2 == null || b1 == null || b2 == null) return;
            // clamping done internally, no need to pass in both sides separately
            // here side1 chosen
            (side1Geometry, side2Geometry) = JointGeometryData.GetPairs(a1, b1, this);
            // lock to world space orientation
            transform.rotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            UpdateMesh();
        }

        /// <summary>
        /// Reorganize square side references based on side parites and pairs are set as side effect
        /// </summary>
        private void FormPairs(SquareSide a1, SquareSide a2, SquareSide b1, SquareSide b2)
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
                    break;
            
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
                    break;
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
                    break;
            
                // somehow there are more than 3 axis different in a 3D space..?
                default:
                    // this includes the possible 3 case when non of the coordinates are equal (the tiles aren't even adjacent!)
                    throw new UnityException($"Joint { transform.parent } contains squares that aren't adjacent!");
            }
        }

    private void UpdateColors()
    {
        colorA1 = a1.EdgeTintedColor();
        colorA2 = a2.EdgeTintedColor();
        colorB1 = b1.EdgeTintedColor();
        colorB2 = b2.EdgeTintedColor();
    }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            //if (filter.sharedMesh != null)
            //{
            //    var vertices = filter.sharedMesh.vertices;
            //    for (int i = 0; i < vertices.Length; i++)
            //    {
            //        Handles.Label(transform.localToWorldMatrix.MultiplyPoint3x4(vertices[i] + filter.sharedMesh.normals[i] * 0.1f), i.ToString());
            //    }
            //}

            if (side1Geometry != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, side1Geometry.nJ);
            }

        }
    #endif

        public void ShowLine(bool value)
        {
            indicator.SetActive(value);
            if (value)
            {
                maskFoldParticles?.Emit();
            }
            else
            {
                maskFoldParticles?.UnEmit();
            }
        }

#if UNITY_EDITOR
        private void UpdateGlowstick()
        {
            var sticks = transform.parent.GetComponentsInChildren<GlowStick>();
            if (sticks.Length == 0) return;
            switch(sticks.Length)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    var (stick1, stick2) = (sticks[0], sticks[1]);
                    if (stick1.SameSide(stick2))
                        throw new UnityException("When 2 glowsticks on the same joint, they must be on the different side of the joint!");
                    break;
                default:
                    throw new UnityException("There cannot be more than 2 glowsticks, at most 1 on each side of the joint!");
            }
        }
#endif

        private void RefreshBuffers()
        {
            // the pivots are duplicated
            verts = new Vector3[8 * (settings.creaseSegmentCount + 1)];
            norms = new Vector3[verts.Length];
            colors = new Color[verts.Length];
            uvs = new Vector2[verts.Length];
        }

        private void UpdateMesh()
        {
            if (verts == null) RefreshBuffers();

            // not actually submeshes, just mentally think of each side of each tile as a submesh
            // the order of the meshes follow the (a, b) (a, b) ordering of the pairs
            var submeshOffset = 2 * (settings.creaseSegmentCount + 1);
            var pivotOffset = settings.creaseSegmentCount + 1;

            var scaledSquareSize = squareRenderSettings.squareSize * (1 - squareRenderSettings.margin);

            for (int i = 0; i <= settings.creaseSegmentCount; i++)
            {
                #region vertex filling
                var t = ts[i] - 0.5f;
                var pivotBase = t * scaledSquareSize * side1Geometry.tJ;
            
                // note that the margin is also affected by the size setting
                // the margin applies to a 01 (uv) square which is sized to produce the actual square
                verts[i] = pivotBase + squareRenderSettings.margin * side1Geometry.nJ2A + side1Geometry.nA * 0.0005f;
                verts[i + submeshOffset] = pivotBase + squareRenderSettings.margin * side1Geometry.nJ2B + side1Geometry.nB * 0.0005f;
                verts[i + 2 * submeshOffset] = pivotBase + squareRenderSettings.margin * side1Geometry.nJ2A + side2Geometry.nA * 0.0005f;
                verts[i + 3 * submeshOffset] = pivotBase + squareRenderSettings.margin * side1Geometry.nJ2B + side2Geometry.nB * 0.0005f;

                //// randomize when angles are significant
                verts[i + pivotOffset] = pivotBase;
                verts[i + pivotOffset + submeshOffset] = pivotBase;
                verts[i + pivotOffset + 2 * submeshOffset] = pivotBase;
                verts[i + pivotOffset + 3 * submeshOffset] = pivotBase;
                #endregion

                #region normals filling

                // note that the margin is also affected by the size setting
                // the margin applies to a 01 (uv) square which is sized to produce the actual square
                norms[i] = side1Geometry.nA;
                norms[i + submeshOffset] = side1Geometry.nB;
                norms[i + 2 * submeshOffset] = side2Geometry.nA;
                norms[i + 3 * submeshOffset] = side2Geometry.nB;

                // all pivots point to crease normal
                norms[i + pivotOffset] = side1Geometry.nJ;
                norms[i + pivotOffset + submeshOffset] = side1Geometry.nJ;
                norms[i + pivotOffset + 2 * submeshOffset] = side2Geometry.nJ;
                norms[i + pivotOffset + 3 * submeshOffset] = side2Geometry.nJ;
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

            // randomize middle vertices
            if (Mathf.Abs(side1Geometry.a2b) > 10f)
            {
                for (int i = 1; i < settings.creaseSegmentCount - 1; i++)
                {
                    var t = ts[i] - 0.5f;
                    var pivotBase = t * scaledSquareSize * side1Geometry.tJ;

                    verts[i + pivotOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;

                    verts[i + pivotOffset + submeshOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;

                    verts[i + pivotOffset + 2 * submeshOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;

                    verts[i + pivotOffset + 3 * submeshOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;
                }
            }

            // 3 points per triangle
            // 4 stripes per mesh
            // 2 triangles per each segment on each stripe
            var tris = new int[3 * 4 * 2 * settings.creaseSegmentCount];
            var triOffset = 3 * 2 * settings.creaseSegmentCount;

            var useCCW = Vector3.Dot(Vector3.Cross(verts[pivotOffset] - verts[0], verts[1] - verts[0]), side1Geometry.nA) < 0;

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

            if (filter.sharedMesh == null)
            {
                filter.sharedMesh = new Mesh()
                {
                    name = "Joint Mesh",
                    vertices = verts,
                    normals = norms,
                    colors = colors,
                    uv = uvs,
                    triangles = tris
                };
                filter.sharedMesh.MarkDynamic();
            } else
            {
                var m = filter.sharedMesh;
                m.name = "Joint Mesh";
                m.vertices = verts;
                m.normals = norms;
                m.colors = colors;
                m.uv = uvs;
                m.triangles = tris;
            }
        }
    }

    internal class JointGeometryData
    {
        public Vector3 pA, pB, pJ;
        public Vector3 nA, nB, nJ, nJ2A, nJ2B, tJ;
        public float a2b;

        internal static (JointGeometryData, JointGeometryData) GetPairs(SquareSide a, SquareSide b, JointRenderer j)
        {
            var pA = a.transform.position;
            var pB = b.transform.position;
            var pJ = j.transform.position;

            var nA = a.transform.up;
            var nB = b.transform.up;

            var nJ2A = (pA - pJ).normalized;
            var nJ2B = (pB - pJ).normalized;

            var tJ = Vector3.Cross(nA, nJ2A); // tangent along joint
            var a2b = -Vector3.SignedAngle(nJ2A, nJ2B, tJ);

            // for large angles pA and pB are easy to cancel each other out (pA + pB approximates pJ) which is bad bc the first method will have a 0
            // for small angles nA and nB are easy to cancel each other out which is bad bc the second method will have a 0
            // overall, we favor using the second method bc it's shorter, so the threshold is set to 5 degrees and not something larger
            // it is possible to do this thresholding without the angle, but the angle is also used elsewhere so might as well
            var nJ = (a2b < 5f && a2b > -5f ? pJ - (pA + pB) / 2 : nA + nB).normalized;

            JointGeometryData side1 = new JointGeometryData()
            {
                pA = pA,
                pB = pB,
                pJ = pJ,
                nA = nA,
                nB = nB,
                nJ2A = nJ2A,
                nJ2B = nJ2B,
                tJ = tJ,
                a2b = a2b,
                nJ = nJ
            };

            JointGeometryData side2 = new JointGeometryData()
            {
                pA = pA,
                pB = pB,
                pJ = pJ,
                nA = -nA,
                nB = -nB,
                nJ2A = nJ2A,
                nJ2B = nJ2B,
                tJ = -tJ,
                a2b = -a2b,
                nJ = -nJ
            };

            return (side1, side2);
        }
    }
}