using BarkaneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

using JointRenderer = BarkaneJoint.JointRenderer;
using JointPieceOwnership = BarkaneJoint.JointRenderer.JointPieceOwnership;
using System.Text;
using Unity.Mathematics;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PaperSquareFace))]
public class SquareSide : MonoBehaviour, IRefreshable
{
    public PaperSquare parentSquare { get; set; }

    public enum SideVisiblity
    {
        full, none
    }

    [SerializeField] MeshFilter mFilter;
    [SerializeField] MeshRenderer mRenderer;
    [SerializeField] CrumbleMeshGenerator meshGenerator;
    [SerializeField] public Material materialPrototype;

    [HideInInspector] public Material materialInstance;
    [SerializeField, HideInInspector] byte[] distanceTextureData;
    [SerializeField, HideInInspector] int distanceTextureWidth;
    [SerializeField, HideInInspector] SerializedMesh meshData;

    public Material MaterialPrototype => materialPrototype;

    public Vector3[] sprinkleVerts;
    public Vector3[] sprinkleNormals;

    public Transform sprinkleParent;

    public Color BaseColor, TintColor;

    public JointPieceCollection JointPieces { get; private set; }

    PaperSquareFace Metadata;

    public GameObject playerLoc;

    public SquareSide OtherSide => parentSquare.topSide == this ? parentSquare.bottomSide : parentSquare.topSide;

    public float YOffset;
    public float YOffsetJoint =>  parentSquare.topSide == this ? YOffset : -1 * YOffset;

    public GameObject visualParent;

    void IRefreshable.EditorRefresh()
    {
        UpdateMesh();
    }

    void IRefreshable.RuntimeRefresh()
    {
        // Debug.Log("Runtime refresh");
        PushData();
        RuntimeParticleUpdate();

        // CAUTION: keep the refresh order of JointRenderer after SquareSide
        JointPieces = new JointPieceCollection(transform);

        Metadata = GetComponent<PaperSquareFace>();
    }

    private void OnDrawGizmosSelected()
    {
        if (JointPieces != null)
        {
            JointPieces.Gizmo();
        }
    }

    public SideVisiblity Visibility
    {
        get => m_SideVisiblity;
        set
        {
            m_SideVisiblity = value;

            //mRenderer.enabled = value == SideVisiblity.full;

            playerLoc.SetActive(value == SideVisiblity.full);

            if (Metadata.Shard)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    // TODO: find more efficient way to do this, also generalize for all objects instead of just crystal
                    var curr = transform.GetChild(i);
                    if (curr.GetComponent<CrystalShard>() != null) continue;
                    transform.GetChild(i).gameObject.SetActive(value != SideVisiblity.none);
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(value != SideVisiblity.none);
                }
            }
        }
    }

    [SerializeField] private SideVisiblity m_SideVisiblity; //for debugging

    private void PushData()
    {
        if (materialInstance == null)
        {
            materialInstance = new Material(materialPrototype)
            {
                name = $"rehydrated {materialPrototype.name}"
            };
        } else
        {
            var distanceTexture = new Texture2D(distanceTextureWidth, distanceTextureWidth);
            distanceTexture.LoadImage(distanceTextureData);
            distanceTexture.Apply();

            mFilter.sharedMesh = meshData.Rehydrated;
            materialInstance.SetTexture("Dist", distanceTexture);

        }
        materialInstance.SetColor("_Color", BaseColor);
        materialInstance.SetColor("_EdgeTint", TintColor);
        materialInstance.SetVector("_NormalOffset", new Vector2(Random.value, Random.value));
        mRenderer.sharedMaterial = materialInstance;
    }

    public void RuntimeParticleUpdate()
    {
        // completely ignores prefab structure. this avoids the unpacking issue

        // the while loop version goes into infinite loop for some reason
        List<GameObject> prev = new List<GameObject>();
        for (int i = 0; i < sprinkleParent.childCount; i++)
        {
            prev.Add(sprinkleParent.GetChild(i).gameObject);
        }
        foreach (var p in prev) { Destroy(p); }

        //while (sprinkleParent.childCount > 0)
        //{
        //    Debug.Log(sprinkleParent.childCount);
        //    Destroy(sprinkleParent.GetChild(0).gameObject);
        //}

        if (VFXManager.Theme.Sprinkle == null || materialPrototype.GetFloat("_UseSprinkles") < 0.5f) return;

        var ct = sprinkleVerts.Length;
        for (int i = 0; i < ct; i++)
        {
            var child = Instantiate(VFXManager.Theme.Sprinkle, sprinkleParent);
            child.transform.localPosition = sprinkleVerts[i];
            child.transform.up = transform.rotation * sprinkleNormals[i];
            child.transform.Rotate(child.transform.up, Random.value * 360f); // paper-parallel rotation
            child.SetActive(true);
        }
    }

    public void UpdateMesh()
    {

        var (mesh, texture, sprinkleVerts, sprinkleNorms) = meshGenerator.Create(materialPrototype);
        this.sprinkleVerts = sprinkleVerts;
        this.sprinkleNormals = sprinkleNorms;

        distanceTextureData = texture.EncodeToPNG();
        distanceTextureWidth = texture.width;
        materialInstance = new Material(materialPrototype)
        {
            name = $"hydrated {materialPrototype.name}"
        };
        meshData = new SerializedMesh(mesh);

        PushData();

#if UNITY_EDITOR
        if (!PrefabUtility.IsPartOfAnyPrefab(this)) return;
        var pRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(this);
        if (pRoot == null || pRoot.GetComponent<FoldablePaper>() == null) return;
        var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(pRoot);
        Debug.Log($"Auto apply changes {this.name} at prefab {pRoot.name}, asset path: {path}");
        PrefabUtility.ApplyObjectOverride(this, path, InteractionMode.UserAction);
       /* var sprinkleCount = sprinkleVerts.Length;

        var paperIsPrefab = LevelEditorManager.IsEditingPrefab;
        var prefabRoot = paperIsPrefab ?
            PrefabUtility.GetOutermostPrefabInstanceRoot(this)
            : null as GameObject;

        if (paperIsPrefab)
            {
                foreach (Transform child in sprinkleParent.transform)
                {
                    Destroy(child.gameObject);
                }
            } else
            {
                 foreach (Transform child in sprinkleParent.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }


        // Debug.Log($"Editing square side of prefab { prefabRoot }");
        var newlyAdded = sprinkleParent.transform.childCount < sprinkleCount ?
            new GameObject[sprinkleCount - sprinkleParent.transform.childCount]
            : new GameObject[0];



        if (sprinkleParent.transform.childCount > sprinkleCount)
        {
            if (paperIsPrefab)
            {
                for (int i = sprinkleParent.transform.childCount; i < sprinkleCount; i++)
                {
                    var child = sprinkleParent.transform.GetChild(i);
                    child.gameObject.SetActive(false);
                }
            } else
            {
                for (int i = sprinkleCount; i > sprinkleParent.transform.childCount; i--)
                {
                    DestroyImmediate(sprinkleParent.transform.GetChild(i - 1));
                }
            }
        }

        for (int i = 0; i < newlyAdded.Length; i++)
        {
            newlyAdded[i] = Instantiate(VFXManager.Theme.Sprinkle, transform);
            newlyAdded[i].transform.parent = sprinkleParent;
        }

        //if (materialPrototype.shader == Shader.Find("Paper") && materialPrototype.GetInt("_UseSprinkles") == 1) //C: No GetBool, need to use GetInt. Also this is broken lol
        //{
        for (int i = 0; i < sprinkleCount; i++)
        {
            var sprinkle = sprinkleParent.transform.GetChild(i);
            sprinkle.localPosition = sprinkleVerts[i];
            sprinkle.up = transform.rotation * sprinkleNorms[i];
            sprinkle.Rotate(sprinkle.up, Random.value * 360f);
            sprinkle.gameObject.SetActive(true);
        }
        
        if (paperIsPrefab && prefabRoot != null)
        {
            PrefabUtility.ApplyPrefabInstance(prefabRoot, InteractionMode.AutomatedAction);
        }*/
        //}
#endif

    }

    public void updateLayer(bool player) {
        if(player)
            gameObject.layer = LayerMask.NameToLayer("PaperNoOutline");
        else
            gameObject.layer = LayerMask.NameToLayer("Paper");
    }
    public (int, int, int) Coordinate => (
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y),
        Mathf.RoundToInt(transform.position.z));

    public static implicit operator (int, int, int)(SquareSide s) => s.Coordinate;

    private void Awake()
    {
        MakeParent();
    }
    
    public void SetYPositionOffset(float offset)
    {
        YOffset = offset;
        if(visualParent == null)
        {
            MakeParent();
        }
        // int c = transform.childCount;
        // for(int i = 0; i < transform.childCount; i++)
        // {
        //     Transform t = transform.GetChild(i);
        //     t.localPosition = YOffset * Vector3.up;
        // }
        visualParent.transform.localPosition = YOffset * Vector3.up;
        if(materialInstance == null) return;
        materialInstance.SetFloat("_YOffset", offset);
    }

    private void MakeParent()
    {
        visualParent = new("Visuals");
       // visualParent.transform.parent = this.transform;
        visualParent.transform.localPosition = transform.position;
        visualParent.transform.localRotation = quaternion.identity;
        while(transform.childCount > 0)
        {
            Transform t = transform.GetChild(0);
            t.parent = visualParent.transform;
        }
        visualParent.transform.parent = this.transform;
    }

    #region overlap

    public void ToggleMesh(bool val)
    {
        Debug.Log("changing mesh");
        mRenderer.enabled = val;
        sprinkleParent.gameObject.SetActive(val);
    }
    #endregion

    public class JointPieceCollection
    {
        /// <summary>
        /// In counter clock-wise order
        /// </summary>
        public JointPieceOwnership[] jpos;
        Transform root; 

        public JointPieceVisibility Visibilities {
            get => m_Visibilities;
            private set
            {
                m_Visibilities = value;
                UpdateVisibility();
            }
        }

        private JointPieceVisibility m_Visibilities;

        // YZX orientation, where local Y points up and +Z is the starting axis

        public JointPieceCollection(Transform root)
        {
            this.root = root;
            jpos = new JointPieceOwnership[] { null, null, null, null };
            Visibilities = AllPiecesVisible;
        }

        public void Register(JointPieceOwnership jpo)
        {
            var p = jpo.PieceParent.jointGeometry.pJ;
            this[p - root.position] = jpo;
        }

        public void UseAsInitialMask()
        {
            Visibilities = AllPiecesVisible;
        }

        public void AlignAndMask(JointPieceCollection prev, bool useDebug = false)
        {
            var v = JointPieceVisibility.None;

            var sb = new StringBuilder();

            if (useDebug) Debug.Log($"Merge spv prev: { prev.root.position } -> curr: { root.position } ");

            for (int i = 0; i < 4; i++)
            {
                if (prev.jpos[i] == null)
                {
                    var dir = Idx2LocalDir(i);
                    var idx = WorldDir2Idx(Vector3Int.RoundToInt(prev.root.TransformDirection(dir)));
                    var occluded = ((ushort)prev.Visibilities & (1 << i)) >> i;

                    if (useDebug) sb.Append($"Open slot with with state: {(occluded == 1 ? "already blocked" : "not blocked yet")}\n... {i} -> {idx}@{Vector3Int.RoundToInt(prev.root.TransformDirection(dir))} | ");
                    // if the i'th entry of prev is NOT occupied, follow the original bit
                    v |= (JointPieceVisibility)(occluded << idx);
                }
#if UNITY_EDITOR
                else
                {
                    var dir = Idx2LocalDir(i);
                    var idx = WorldDir2Idx(Vector3Int.RoundToInt(prev.root.TransformDirection(dir)));
                    if (useDebug) sb.Append($"Close slot, automatically hiding {i} -> {idx}@{Vector3Int.RoundToInt(prev.root.TransformDirection(dir))} | ");
                    // if the i'th entry of prev is occupied, then block the visibility in this side
                    // v &= ~(JointPieceVisibility)(1 << idx);
                }
#endif
            }

            if (useDebug) Debug.Log(sb.ToString());
            // if (useDebug) Debug.Log(v);

            Visibilities = v; // RotateVisibilities(vPrev, (ushort)Tr2Idx(prev.root));
        }

        private static JointPieceVisibility RotateVisibilities(JointPieceVisibility start, ushort iterations)
        {
            ushort startVal = (ushort)start;
            ushort reverse = (ushort)(4 - iterations);

            return (JointPieceVisibility)(((startVal << iterations) | (startVal >> reverse)) & (ushort) 0b1111);
        }

        private void UpdateVisibility()
        {
            for (ushort i = 0; i < 4; i++)
            {
                if (jpos[i] != null) jpos[i].Renderer.enabled = true;
                //((ushort)Visibilities & ((ushort) 1 << i)) != 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="">World space direction of the joint piece relative to the square side's center</param>
        /// <returns></returns>
        public JointPieceOwnership this[Vector3 wDir]
        {
            get => jpos[WorldDir2Idx(wDir)];
            private set
            {
                var idx = WorldDir2Idx(wDir);
                if (jpos[idx] != null)
                    throw new UnityException("Joint renderer slot can only be written once!");
                jpos[idx] = value;
            }
        }

        private static readonly Vector3Int[] dirs = new Vector3Int[]
        {
            Vector3Int.forward,
            Vector3Int.left,
            Vector3Int.back,
            Vector3Int.right,
        };

        private Vector3Int Idx2LocalDir(int i) => dirs[i];

        private int WorldDir2Idx(Vector3 wDir)
        {
            var lDir = Vector3Int.RoundToInt(root.InverseTransformDirection(wDir));
            if (lDir.z > 0)
                return 0;
            else if (lDir.z < 0)
                return 2;
            else if (lDir.x > 0)
                return 3;
            else if (lDir.x < 0)
                return 1;
            else
                throw new IndexOutOfRangeException($"{wDir}");
        }

        [Flags]
        public enum JointPieceVisibility
        {
            None = 0,
            First = 1 << 0,
            Second = 1 << 1,
            Third = 1 << 2,
            Fourth = 1 << 3
        }

        public static JointPieceVisibility AllPiecesVisible =
            JointPieceVisibility.First
            | JointPieceVisibility.Second
            | JointPieceVisibility.Third
            | JointPieceVisibility.Fourth;

        public void Gizmo()
        {
            for (int i = 0; i < 4; i++)
            {
                if (jpos[i] == null) continue;

                var visible = jpos[i].Renderer.enabled;

               // Handles.Label(root.TransformDirection(Idx2LocalDir(i)) + root.position + root.up * 0.5f, $"{(visible ? "(" : "")}{i}{(visible ? ")" : "")}");
            }
        }
    }
}
