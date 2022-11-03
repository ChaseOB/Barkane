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

    [SerializeField, HideInInspector] Material materialInstance;
    [SerializeField, HideInInspector] byte[] distanceTextureData;
    [SerializeField, HideInInspector] int distanceTextureWidth;
    [SerializeField, HideInInspector] SerializedMesh meshData;

    public Material MaterialPrototype => materialPrototype;

    public (Vector3[], Vector3[]) sprinkles;
    // A: unsure why this is needed
    // public Transform sprinkleParent;

    void IRefreshable.Refresh()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            UpdateMesh();
        } else
        {
            PushData();
        }
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
            mRenderer.sharedMaterials = new Material[] { materialInstance };
        }
    }


    private void Update()
    {
        if (materialInstance != null)
        {
            materialInstance.SetVector("YOverride", transform.up);
        }
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
        var sprinkleCount = sprinkleVerts.Length;

        var isPartOfPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(this);
        var prefabRoot = isPartOfPrefabInstance ?
            PrefabUtility.GetOutermostPrefabInstanceRoot(this)
            : null as GameObject;

        // Debug.Log($"Editing square side of prefab { prefabRoot }");
        var newlyAdded = transform.childCount < sprinkleCount ?
            new GameObject[sprinkleCount - transform.childCount]
            : new GameObject[0];

        if (transform.childCount > sprinkleCount)
        {
            if (isPartOfPrefabInstance)
            {
                for (int i = transform.childCount; i < sprinkleCount; i++)
                {
                    var child = transform.GetChild(i);
                    child.gameObject.SetActive(false);
                }
            } else
            {
                for (int i = sprinkleCount; i > transform.childCount; i--)
                {
                    DestroyImmediate(transform.GetChild(i - 1));
                }
            }
        }

        for (int i = 0; i < newlyAdded.Length; i++)
        {
            newlyAdded[i] = Instantiate(VFXManager.Theme.Sprinkle, transform);
        }

        //if (materialPrototype.shader == Shader.Find("Paper") && materialPrototype.GetInt("_UseSprinkles") == 1) //C: No GetBool, need to use GetInt. Also this is broken lol
        //{
        for (int i = 0; i < sprinkleCount; i++)
        {
            var sprinkle = transform.GetChild(i);
            sprinkle.localPosition = sprinkleVerts[i];
            sprinkle.up = transform.rotation * sprinkleNorms[i];
            sprinkle.Rotate(sprinkle.up, Random.value * 360f);
            sprinkle.gameObject.SetActive(true);
        }

        if (isPartOfPrefabInstance && prefabRoot != null)
        {
            PrefabUtility.ApplyPrefabInstance(prefabRoot, InteractionMode.AutomatedAction);
        }
        //}
#endif

    }

    /// <summary>
    /// see lerp in paper shader
    /// </summary>
    /// <returns>evaluates the properly tinted color for paper edge</returns>
    public Color EdgeTintedColor(float correction)
    {
        var baseColor = materialPrototype.GetColor("_Color");
        var tintColor = materialPrototype.GetColor("_EdgeTint");

        return new Color(
            Mathf.Lerp(baseColor.r, tintColor.r, tintColor.a + correction),
            Mathf.Lerp(baseColor.g, tintColor.g, tintColor.a + correction),
            Mathf.Lerp(baseColor.b, tintColor.b, tintColor.a + correction)
            ); 
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
       // sprinkleParent.gameObject.SetActive(val);
    }
    #endregion
}
