using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BarkaneEditor;
using UnityEngine.Rendering;
using UnityEngine.ProBuilder;

namespace BarkaneJoint
{
    [ExecuteAlways]
    public class JointRenderer : MonoBehaviour, IRefreshable, IDynamicMesh<CreaseRenderSettings>
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

        [SerializeField] MeshFilter fA1, fA2, fB1, fB2;
        [SerializeField] MeshRenderer mrA1, mrA2, mrB1, mrB2;

        public ((GameObject, GameObject), (GameObject, GameObject)) facePairs => ((a1.gameObject, b1.gameObject), (a2.gameObject, b2.gameObject));
        [SerializeField, HideInInspector] private SquareSide a1, a2, b1, b2;

        [SerializeField, HideInInspector] private Vector3[] randoms;

        [SerializeField] private GameObject indicator;
        [SerializeField] private MaskFoldParticles maskFoldParticles;

        internal JointGeometryData side1Geometry, side2Geometry;

        [SerializeField] private Material materialPrototype;

        // buffers
        private Vector3[] vA1, vA2, vB1, vB2;// nA1, nA2, nB1, nB2;

        float scaledSquareSize => squareRenderSettings.squareSize * (1 - squareRenderSettings.margin);

        /// <summary>
        /// Can be called manually in inspector or automatically by other scene editor utilities.
        /// </summary>
        /// <exception cref="UnityException"></exception>
        void IRefreshable.EditorRefresh()
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

            randoms = new Vector3[settings.creaseSegmentCount + 1];

            for (int i = 0; i <= settings.creaseSegmentCount; i++)
            {
                randoms[i] = new Vector3(
                    2 * (Random.value - 0.5f) * settings.creaseDeviation.x,
                    2 * (Random.value - 0.5f) * settings.creaseDeviation.y,
                    2 * (Random.value - 0.5f) * settings.creaseDeviation.z);
            }

            ValidateSidedAddon<GlowStick>();
            ValidateSidedAddon<Tape>();
            UpdateMesh(true);
        }

        void IRefreshable.RuntimeRefresh()
        {

        }

        public bool IsAnimating = false;

        public System.Action DisableMeshAction => new System.Action(delegate() {
        });

        public System.Action EnableMeshAction => new System.Action(delegate ()
        {
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

            //if (side1Geometry != null)
            //{
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawRay(transform.position, side1Geometry.nJ);
            //}
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
        private void ValidateSidedAddon<T>() where T:SidedJointAddon
        {
            var sticks = transform.parent.GetComponentsInChildren<T>();
            if (sticks.Length == 0) return;
            switch(sticks.Length)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    var (stick1, stick2) = (sticks[0], sticks[1]);
                    if (stick1.SameSide(stick2))
                        throw new UnityException($"When 2 {typeof(T).Name} on the same joint, they must be on the different side of the joint!");
                    break;
                default:
                    throw new UnityException($"There cannot be more than 2 { typeof(T).Name }s, at most 1 on each side of the joint!");
            }
        }
#endif
        public void ClearAndInitBuffers(CreaseRenderSettings settings)
        {
            // the pivots are duplicated
            vA1 = new Vector3[settings.VCount];
            vA2 = new Vector3[settings.VCount];
            vB1 = new Vector3[settings.VCount];
            vB2 = new Vector3[settings.VCount];
        }

        private void UpdateVertexNormals()
        {
            for (int i = 0; i <= settings.creaseSegmentCount; i++)
            {
                #region vertex filling
                var t = settings.ts[i];
                var pivotBase = t * scaledSquareSize * side1Geometry.tJ;
                var margin = squareRenderSettings.margin + .0005f;

                // note that the margin is also affected by the size setting
                // the margin applies to a 01 (uv) square which is sized to produce the actual square
                vA1[i] = pivotBase + margin * side1Geometry.nJ2A;// + side1Geometry.nA * 0.0006f;
                vB1[i] = pivotBase + margin * side1Geometry.nJ2B;// + side1Geometry.nB * 0.0006f;
                vA2[i] = pivotBase + margin * side1Geometry.nJ2A;// + side2Geometry.nA * 0.0006f;
                vB2[i] = pivotBase + margin * side1Geometry.nJ2B;// + side2Geometry.nB * 0.0006f;

                //// randomize when angles are significant
                vA1[i + settings.PivotOffset] = pivotBase;
                vB1[i + settings.PivotOffset] = pivotBase;
                vA2[i + settings.PivotOffset] = pivotBase;
                vB2[i + settings.PivotOffset] = pivotBase;
                #endregion

                //#region normals filling
                //// note that the margin is also affected by the size setting
                //// the margin applies to a 01 (uv) square which is sized to produce the actual square
                //ns[i] = side1Geometry.nA;
                //ns[i + settings.SubmeshOffset] = side1Geometry.nB;
                //ns[i + 2 * settings.SubmeshOffset] = side2Geometry.nA;
                //ns[i + 3 * settings.SubmeshOffset] = side2Geometry.nB;

                //// all pivots point to crease normal
                //ns[i + settings.PivotOffset] = side1Geometry.nJ;
                //ns[i + settings.PivotOffset + settings.SubmeshOffset] = side1Geometry.nJ;
                //ns[i + settings.PivotOffset + 2 * settings.SubmeshOffset] = side2Geometry.nJ;
                //ns[i + settings.PivotOffset + 3 * settings.SubmeshOffset] = side2Geometry.nJ;
                //#endregion
            }

            // randomize middle vertices at significant fold angles
            if (Mathf.Abs(side1Geometry.a2b) > 10f)
            {
                for (int i = 1; i < settings.creaseSegmentCount - 1; i++)
                {
                    vA1[i + settings.PivotOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;

                    vB1[i + settings.PivotOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;

                    vA2[i + settings.PivotOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;

                    vB2[i + settings.PivotOffset] +=
                        randoms[i].z * side1Geometry.tJ
                        + randoms[i].y * side1Geometry.nJ
                        + randoms[i].x * side1Geometry.nJ2A;

                    //// note that the margin is also affected by the size setting
                    //// the margin applies to a 01 (uv) square which is sized to produce the actual square
                    //ns[i] = side1Geometry.nA;
                    //ns[i + settings.SubmeshOffset] = side1Geometry.nB;
                    //ns[i + 2 * settings.SubmeshOffset] = side2Geometry.nA;
                    //ns[i + 3 * settings.SubmeshOffset] = side2Geometry.nB;

                    //// all pivots point to crease normal
                    //ns[i + settings.PivotOffset] = side1Geometry.nJ;
                    //ns[i + settings.PivotOffset + settings.SubmeshOffset] = side1Geometry.nJ;
                    //ns[i + settings.PivotOffset + 2 * settings.SubmeshOffset] = side2Geometry.nJ;
                    //ns[i + settings.PivotOffset + 3 * settings.SubmeshOffset] = side2Geometry.nJ;
                }
            }

            fA1.sharedMesh.vertices = vA1;
            fA2.sharedMesh.vertices = vA2;
            fB1.sharedMesh.vertices = vB1;
            fB2.sharedMesh.vertices = vB2;
        }

        private Material BindColor(Material mat, SquareSide src, string name)
        {
            var m = new Material(mat)
            {
                name = $"{mat.name} {name}"
            };
            m.SetColor("_Color", src.baseColor);
            m.SetColor("_EdgeTint", src.tintColor);
            return m;
        }

        private void Setup(SquareSide side, MeshFilter f, MeshRenderer mr, string name)
        {
            f.sharedMesh = new Mesh() {
                name = $"Joint Mesh {name}",
            };

            f.sharedMesh.MarkDynamic();

            mr.sharedMaterial = BindColor(materialPrototype, side, name);
        }

        private void FullSetup()
        {
            Setup(a1, fA1, mrA1, "A1");
            Setup(a2, fA2, mrA2, "A2");
            Setup(b1, fB1, mrB1, "B1");
            Setup(b2, fB2, mrB2, "B2");
        }

        private void UpdateMesh(bool force=false)
        {
            var init = force || vA1 == null || vA1.Length != settings.VCount;
            if (init)
            {
                ClearAndInitBuffers(settings);
                FullSetup();
                return;
            }

            UpdateVertexNormals();

            if (init)
            {
                fA1.sharedMesh.triangles = settings.tA1CW;
                fA2.sharedMesh.triangles = settings.tA2CW;
                fB1.sharedMesh.triangles = settings.tB1CW;
                fB2.sharedMesh.triangles = settings.tB2CW;
            }

            fA1.sharedMesh.MarkModified();
            fA2.sharedMesh.MarkModified();
            fB1.sharedMesh.MarkModified();
            fB2.sharedMesh.MarkModified();
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