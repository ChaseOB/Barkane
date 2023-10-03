using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BarkaneEditor;
using UnityEngine.Rendering;
using UnityEngine.ProBuilder;
using System;

namespace BarkaneJoint
{
    [ExecuteAlways]
    public class JointRenderer : MonoBehaviour, IRefreshable, IDynamicMesh<CreaseRenderSettings>
    {
        /**
         * FOR THE CONTEXT OF JOINT VFX...
         * A and B always represent the identity of the square, A always across the joint from B, and vice versa
         * 1 and 2 always represent the side of the square, A1 and A2 are always on the same side of the joint (but different facing direction), same for B
         * Pair1 contains both A and B for side 1, Pair2 contains both A and B for side 2
         */

        // [SerializeField] SquareRenderSettings squareRenderSettings; // for referencing the margin property
        [SerializeField] CreaseRenderSettings settings;
        [SerializeField] SquareRenderSettings squareRenderSettings;

        [SerializeField] MeshFilter fA1, fA2, fB1, fB2;
        [SerializeField] MeshRenderer mrA1, mrA2, mrB1, mrB2;

        public ((GameObject, GameObject), (GameObject, GameObject)) facePairs => ((a1.gameObject, b1.gameObject), (a2.gameObject, b2.gameObject));
        [SerializeField] private SquareSide a1, a2, b1, b2;

        [SerializeField, HideInInspector] private Vector3[] randoms;

        [SerializeField] private GameObject indicator;
        [SerializeField] private MaskFoldParticles maskFoldParticles;

        internal JointGeometryData jointGeometry = new JointGeometryData();
        internal JointGeometryData.JointSideGeometryData
            jointGeometry1 = new JointGeometryData.JointSideGeometryData(),
            jointGeometry2 = new JointGeometryData.JointSideGeometryData();

        [SerializeField] private Material materialPrototype;

        // buffers
        private Vector3[] vA1, vA2, vB1, vB2;// nA1, nA2, nB1, nB2;

        float scaledSquareSize => squareRenderSettings.squareSize * (1 - squareRenderSettings.margin);

        public bool Debug;
        public Vector3 offset = Vector3.zero;


        public float tStart;
        public float tMid;
        public Vector3 pivotBaseStart; 
        public Vector3 pivotBaseMid; 


        public Material goodMat;
        public Material badMat;
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
                    2 * (UnityEngine.Random.value - 0.5f) * settings.creaseDeviation.x,
                    2 * (UnityEngine.Random.value - 0.5f) * settings.creaseDeviation.y,
                    2 * (UnityEngine.Random.value - 0.5f) * settings.creaseDeviation.z);
            }

#if UNITY_EDITOR
            ValidateSidedAddon<GlowStick>();
            ValidateSidedAddon<Tape>();
#endif
            UpdateMesh(true);
        }

        void IRefreshable.RuntimeRefresh() {
            UpdateGeometryData();
            UpdateMesh(true);

            // CAUTION: keep the refresh order of JointRenderer after SquareSide
            PushRelationToParent();
        }

        public bool IsAnimating = false;

        public System.Action DisableMeshAction => new System.Action(delegate() {
        });

        public System.Action EnableMeshAction => new System.Action(delegate ()
        {
        });

        void Update()
        {
            // PullVisibility(a1, mrA1);
            // PullVisibility(a2, mrA2);
            // PullVisibility(b1, mrB1);
            // PullVisibility(b2, mrB2);
            UpdateGeometryData();
        }

        void UpdateGeometryData()
        {

            if (a1 == null || a2 == null || b1 == null || b2 == null) return;
            // clamping done internally, no need to pass in both sides separately
            // here side1 chosen
            JointGeometryData.Update(a1, b1, this, squareRenderSettings.margin, ref jointGeometry, ref jointGeometry1, ref jointGeometry2);
            // lock to world space orientation
            transform.rotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            UpdateMesh();
            indicator.transform.localPosition = Quaternion.Inverse(transform.parent.localRotation) * offset;
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

    //#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(jointGeometry.edgeA, 0.03f);
            Gizmos.DrawLine(jointGeometry.pA, jointGeometry.pJ);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(jointGeometry.edgeB, 0.03f);
            Gizmos.DrawLine(jointGeometry.pB, jointGeometry.pJ);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(jointGeometry.pJ, 0.03f);
            Gizmos.DrawLine(jointGeometry.pJ, jointGeometry.pJ + jointGeometry1.tJ);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(this.transform.position + pivotBaseStart, 0.03f);

            Gizmos.color = Color.black;
            Gizmos.DrawSphere(this.transform.position + pivotBaseMid, 0.03f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(jointGeometry.pJ, jointGeometry.nJ2A);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(jointGeometry.pJ, jointGeometry.nJ2B);

        }
   // #endif

        public void ShowLine(bool value, bool staySelected = false)
        {
            indicator.SetActive(value || staySelected);
            // if (value || staySelected)
            // {
            //     maskFoldParticles?.Emit();
            // }
            // else
            // {
            //     maskFoldParticles?.UnEmit();
            // }
        }

        public void SetLineActive(bool active)
        {
            indicator.GetComponent<MeshRenderer>().material = active ? goodMat : badMat;
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
                tStart = settings.tsStart[i];
                tMid = settings.tsMid[i];

                pivotBaseStart = jointGeometry.offset + (jointGeometry1.tJ * tStart * scaledSquareSize);
                pivotBaseMid = jointGeometry.offset + (jointGeometry1.tJ * tMid * scaledSquareSize);
                //var margin = squareRenderSettings.margin + .001f;

                vA1[i] = pivotBaseMid + jointGeometry.nJ2A ;
                vB1[i] = pivotBaseMid + jointGeometry.nJ2B ;
                vA2[i] = pivotBaseMid + jointGeometry.nJ2A ;
                vB2[i] = pivotBaseMid + jointGeometry.nJ2B ;

                List<Vector3Int> squarepositions = new List<Vector3Int>()
                {
                    Vector3Int.RoundToInt(a1.parentSquare.transform.position),
                    Vector3Int.RoundToInt(a2.parentSquare.transform.position)
                };
                //Vector3 offset = Vector3.zero;
                if(Debug)
                {
                    print(CoordUtils.DiffAxisCount(a1, b1));
                }
                offset = Vector3.zero;
                switch(CoordUtils.DiffAxisCount(a1, b1))
                {
                    case 0: //squares in same position, should be at midpoint of squares
                    case 1:
                        offset = b2.parentSquare.transform.rotation * new Vector3(0, b2.YOffsetJoint, 0) + a2.parentSquare.transform.rotation * new Vector3(0, a2.YOffsetJoint, 0);
                        offset *= 0.5f;
                        break;
                    case 2: //sqaures at 90* angle, offset by both y offsets
                        offset = b2.parentSquare.transform.rotation * new Vector3(0, b2.YOffsetJoint, 0) + a2.parentSquare.transform.rotation * new Vector3(0, a2.YOffsetJoint, 0);
                        break;
                    default: //squares coplaner, don't move
                        break;
                }
              //  pivotBaseStart += offset;
                if(Debug)
                {
                    print(offset);
                }
                vA1[i + settings.PivotOffset] = pivotBaseStart;
                vB1[i + settings.PivotOffset] = pivotBaseStart;
                vA2[i + settings.PivotOffset] = pivotBaseStart;
                vB2[i + settings.PivotOffset] = pivotBaseStart;
                #endregion
            }

            // randomize middle vertices at significant fold angles
            // if(Debug)
            //     print(Mathf.Abs(jointGeometry1.a2b));
            if (true) //(Mathf.Abs(jointGeometry1.a2b) > 10f)
            {
                for (int i = 1; i < settings.creaseSegmentCount - 1; i++)
                {
                    vA1[i + settings.PivotOffset] +=
                        randoms[i].z * jointGeometry1.tJ
                        + randoms[i].y * jointGeometry1.nJ
                        + randoms[i].x * jointGeometry.nJ2A;

                    vB1[i + settings.PivotOffset] +=
                        randoms[i].z * jointGeometry1.tJ
                        + randoms[i].y * jointGeometry1.nJ
                        + randoms[i].x * jointGeometry.nJ2A;

                    vA2[i + settings.PivotOffset] +=
                        randoms[i].z * jointGeometry1.tJ
                        + randoms[i].y * jointGeometry1.nJ
                        + randoms[i].x * jointGeometry.nJ2A;

                    vB2[i + settings.PivotOffset] +=
                        randoms[i].z * jointGeometry1.tJ
                        + randoms[i].y * jointGeometry1.nJ
                        + randoms[i].x * jointGeometry.nJ2A;
                }
            }

            fA1.sharedMesh.SetVertices(vA1, 0, vA1.Length, flags: SidedJointAddon.fConsiderBounds);
            fA2.sharedMesh.SetVertices(vA2, 0, vA1.Length, flags: SidedJointAddon.fConsiderBounds);
            fB1.sharedMesh.SetVertices(vB1, 0, vA1.Length, flags: SidedJointAddon.fConsiderBounds);
            fB2.sharedMesh.SetVertices(vB2, 0, vA1.Length, flags: SidedJointAddon.fConsiderBounds);
        }

        private Material BindColor(Material m, SquareSide src)
        {
            m.SetColor("_Color", src.BaseColor);
            m.SetColor("_EdgeTint", src.TintColor);
            return m;
        }

        private void Setup(SquareSide side, MeshFilter f, MeshRenderer mr, string name)
        {
            f.sharedMesh = new Mesh() {
                name = $"Joint Mesh {name}",
            };

            f.sharedMesh.MarkDynamic();
            var mInst = new Material(materialPrototype)
            {
                name = $"{materialPrototype.name} {name}"
            };
            mr.sharedMaterial = BindColor(mInst, side);
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
            }

            UpdateVertexNormals();

            if (init)
            {
                fA1.sharedMesh.SetTriangles(settings.tA1CCW, 0, false);
                fA2.sharedMesh.SetTriangles(settings.tA2CCW, 0, false);
                fB1.sharedMesh.SetTriangles(settings.tB1CCW, 0, false);
                fB2.sharedMesh.SetTriangles(settings.tB2CCW, 0, false);
            }
        }

        public (Vector3, Vector3) GetParentSquareOffsets()
        {
            return( a1.parentSquare.transform.rotation * new Vector3(0, a1.YOffsetJoint, 0),
                b1.parentSquare.transform.rotation * new Vector3(0, b1.YOffsetJoint, 0));
        }

        private void PushRelationToParent()
        {
            a1.JointPieces.Register(new JointPieceOwnership
            {
                PieceParent = this,
                Renderer = mrA1
            });

            a2.JointPieces.Register(new JointPieceOwnership
            {
                PieceParent = this,
                Renderer = mrA2
            });

            b1.JointPieces.Register(new JointPieceOwnership
            {
                PieceParent = this,
                Renderer = mrB1
            });

            b2.JointPieces.Register(new JointPieceOwnership
            {
                PieceParent = this,
                Renderer = mrB2
            });
        }

        public class JointPieceOwnership
        {
            public JointRenderer PieceParent { get; internal set; }
            public MeshRenderer Renderer { get; internal set; }
        }
    }

    public class JointGeometryData
    {
        internal class JointSideGeometryData
        {
            public Vector3 nA, nB, nJ, tJ;
            public float a2b;
        }

        public Vector3 pA, pB, pJ, offset;
        public Vector3 nJ2A, nJ2B;
        public Vector3 edgeA, edgeB;
        public Vector3 yOffsetA, yOffsetB;
      //  public float margin;

        internal static void Update(SquareSide a, SquareSide b, JointRenderer j, float margin,
            ref JointGeometryData g,
            ref JointSideGeometryData g1, ref JointSideGeometryData g2)
        {

            g.yOffsetA = a.transform.rotation * new Vector3(0, a.YOffset, 0);
            g.yOffsetB = b.transform.rotation * new Vector3(0, b.YOffset, 0);

            g.pA = a.transform.position + g.yOffsetA; 
            g.pB = b.transform.position + g.yOffsetB;

            Vector3 offset = Vector3.zero;
                switch(CoordUtils.DiffAxisCount(a, b))
                {
                    case 0: //squares in same position, should be at midpoint of squares
                    case 1:
                        offset = b.parentSquare.transform.rotation * new Vector3(0, b.YOffsetJoint, 0) + a.parentSquare.transform.rotation * new Vector3(0, a.YOffsetJoint, 0);
                        offset *= 0.5f;
                        break;
                    case 2: //sqaures at 90* angle, offset by both y offsets
                        offset = b.parentSquare.transform.rotation * new Vector3(0, b.YOffsetJoint, 0) + a.parentSquare.transform.rotation * new Vector3(0, a.YOffsetJoint, 0);
                        break;
                    default: //squares coplaner, don't move
                        break;
                }
            g.offset = offset;
           // Debug.Log(offset);

            g.pJ =  j.transform.position + offset;

            g.edgeA = (j.transform.position - a.transform.position) * (1 - margin) + g.pA;
            g.edgeB = (j.transform.position - b.transform.position)* (1 - margin ) + g.pB;
            
            g.nJ2A = (g.edgeA - g.pJ);
            g.nJ2B = (g.edgeB - g.pJ);

            g1.nA = a.transform.up;
            g1.nB = b.transform.up;
            g1.tJ = Vector3.Cross(g1.nA, g.nJ2A).normalized;
            g1.a2b = Vector3.SignedAngle(g1.nA, g1.nB, g1.tJ);
            
            // for large angles pA and pB are easy to cancel each other out (pA + pB approximates pJ) which is bad bc the first method will have a 0
            // for small angles nA and nB are easy to cancel each other out which is bad bc the second method will have a 0
            // overall, we favor using the second method bc it's shorter, so the threshold is set to 5 degrees and not something larger
            // it is possible to do this thresholding without the angle, but the angle is also used elsewhere so might as well
            g1.nJ = (g1.a2b <= 100f && g1.a2b >= -100f ? g1.nA + g1.nB : -g.nJ2A - g.nJ2B).normalized;
            //g1.nJ = (g1.nA + g1.nB).normalized;
            // TODO: simply below, remove as many trigs as possible

            g2.nA = -g1.nA;
            g2.nB = -g1.nB;
            g2.tJ = -g1.tJ;
            g2.a2b = Vector3.SignedAngle(g2.nA, g2.nB, g2.tJ);

            // for large angles pA and pB are easy to cancel each other out (pA + pB approximates pJ) which is bad bc the first method will have a 0
            // for small angles nA and nB are easy to cancel each other out which is bad bc the second method will have a 0
            // overall, we favor using the second method bc it's shorter, so the threshold is set to 5 degrees and not something larger
            // it is possible to do this thresholding without the angle, but the angle is also used elsewhere so might as well
            g2.nJ = (g2.a2b <= 100f && g2.a2b >= -100f ? g2.nA + g2.nB : -g.nJ2A - g.nJ2B).normalized;
            //g2.nJ = (g2.nA + g2.nB).normalized;

            ///SOURCE OF THE PROBLEM: nJ is 0 when A and B are back-to-back. how to fix? idk bro
        }
    }
}