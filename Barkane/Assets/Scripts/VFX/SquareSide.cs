using BarkaneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

using JointRenderer = BarkaneJoint.JointRenderer;
using JointPieceOwnership = BarkaneJoint.JointRenderer.JointPieceOwnership;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SquareSide : MonoBehaviour, IRefreshable
{
    public PaperSquare parentSquare { get; set; }

    public enum SideVisiblity
    {
        full, ghost, none
    }

    [SerializeField] MeshFilter mFilter;
    [SerializeField] MeshRenderer mRenderer;
    [SerializeField] CrumbleMeshGenerator meshGenerator;
    [SerializeField] public Material materialPrototype;

    [HideInInspector] public Material materialInstance;
    [SerializeField, HideInInspector] byte[] distanceTextureData;
    [SerializeField, HideInInspector] int distanceTextureWidth;
    [SerializeField, HideInInspector] SerializedMesh meshData;

    public Material materialOverride
    {
        get => m_MaterialOverride;
        private set
        {
            m_MaterialOverride = value;
            mRenderer.sharedMaterial = value;
        }
    }
    public Material m_MaterialOverride;

    public Material MaterialPrototype => materialPrototype;

    public Vector3[] sprinkleVerts;
    public Vector3[] sprinkleNormals;

    public Transform sprinkleParent;

    public Color BaseColor, TintColor;

    public JointPieceCollection JointPieces { get; private set; }

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
    }

    public SideVisiblity Visibility
    {
        get => m_SideVisiblity;
        set
        {
            m_SideVisiblity = value;

            switch (value)
            {
                case SideVisiblity.full:
                    mRenderer.enabled = true;
                    materialOverride = materialInstance;
                    break;
                case SideVisiblity.ghost:
                    mRenderer.enabled = true;
                    materialOverride = VFXManager.Theme.GhostMat;
                    break;
                case SideVisiblity.none:
                    mRenderer.enabled = false;
                    break;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(value != SideVisiblity.none);
            }
        }
    }

    private SideVisiblity m_SideVisiblity;

    private void PushData()
    {
        if (materialInstance == null)
        {
            materialInstance = new Material(materialPrototype)
            {
                name = $"rehydrated {materialPrototype.name}"
            };
        }
        if (materialInstance != null)
        {
            var distanceTexture = new Texture2D(distanceTextureWidth, distanceTextureWidth);
            distanceTexture.LoadImage(distanceTextureData);
            distanceTexture.Apply();

            mFilter.sharedMesh = meshData.Rehydrated;
            materialInstance.SetTexture("Dist", distanceTexture);
            // mRenderer.sharedMaterial = materialInstance;

        }
        materialInstance.SetColor("_Color", BaseColor);
        materialInstance.SetColor("_EdgeTint", TintColor);
        materialInstance.SetVector("_NormalOffset", new Vector2(Random.value, Random.value));

        materialOverride = materialInstance;
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
        if (pRoot == null) return;
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

        public JointPieceVisibility Visibilities { get; private set; }

        // YZX orientation, where local Y points up and +Z is the starting axis

        public JointPieceCollection(Transform root)
        {
            this.root = root;
            jpos = new JointPieceOwnership[] { null, null, null, null };
            Visibilities = AllPiecesVisible;
        }

        public void Register(JointPieceOwnership jpo)
        {
            var p = jpo.PieceParent.transform.position;
            this[p - root.position] = jpo;
        }

        public void UseAsInitialMask()
        {
            var v = JointPieceVisibility.None;

            for (ushort i = 0; i < 4; i++)
            {
                if (jpos[i] != null) v = (JointPieceVisibility)((ushort)v | ((ushort)1 << i));
            }

            Visibilities = v;
        }

        public void AlignAndMask(JointPieceCollection prev)
        {
            Visibilities &= ~RotateVisibilities(prev.Visibilities, (ushort)tr2Idx(prev.root));
            UpdateVisibility();
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
                if (jpos[i] != null) jpos[i].Renderer.enabled = ((ushort)Visibilities & ((ushort) 1 << i)) != 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="">World space direction of the joint piece relative to the square side's center</param>
        /// <returns></returns>
        public JointPieceOwnership this[Vector3 wDir]
        {
            get => jpos[wDir2Idx(wDir)];
            private set
            {
                var idx = wDir2Idx(wDir);
                if (jpos[idx] != null)
                    throw new UnityException("Joint renderer slot can only be written once!");
                jpos[idx] = value;
            }
        }

        private int tr2Idx(Transform alignment) => wDir2Idx(alignment.right);

        private int wDir2Idx(Vector3 wDir)
        {
            var lDir = Vector3Int.RoundToInt(root.InverseTransformDirection(wDir));
            if (lDir.z > 0)
                return 0;
            else if (lDir.z < 0)
                return 1;
            else if (lDir.x > 0)
                return 2;
            else if (lDir.x < 0)
                return 3;
            else
                throw new IndexOutOfRangeException($"{wDir}");
        }

        [System.Flags]
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
    }
}
