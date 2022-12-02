using BarkaneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SquareSide : MonoBehaviour, IRefreshable
{
    [SerializeField] MeshFilter mFilter;
    [SerializeField] MeshRenderer mRenderer;
    [SerializeField] CrumbleMeshGenerator meshGenerator;
    [SerializeField] public Material materialPrototype;

    [HideInInspector] public Material materialInstance;
    [SerializeField, HideInInspector] byte[] distanceTextureData;
    [SerializeField, HideInInspector] int distanceTextureWidth;
    [SerializeField, HideInInspector] SerializedMesh meshData;

    public Material MaterialPrototype => materialPrototype;

    public (Vector3[], Vector3[]) sprinkles;
    // A: unsure why this is needed
    public Transform sprinkleParent;

    public Color BaseColor, TintColor;

    void IRefreshable.EditorRefresh()
    {
        UpdateMesh();
    }

    void IRefreshable.RuntimeRefresh()
    {
        PushData();
    }

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
            mRenderer.sharedMaterial = materialInstance;

        }
        materialInstance.SetColor("_Color", BaseColor);
        materialInstance.SetColor("_EdgeTint", TintColor);
        materialInstance.SetVector("_NormalOffset", new Vector2(Random.value, Random.value));
    }

    public void UpdateMesh()
    {

        var (mesh, texture, sprinkleVerts, sprinkleNorms) = meshGenerator.Create(materialPrototype);
        distanceTextureData = texture.EncodeToPNG();
        distanceTextureWidth = texture.width;
        materialInstance = new Material(materialPrototype)
        {
            name = $"hydrated {materialPrototype.name}"
        };
        meshData = new SerializedMesh(mesh);

        PushData();

#if UNITY_EDITOR
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
}
