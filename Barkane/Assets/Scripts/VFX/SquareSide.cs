using BarkaneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        while (transform.childCount > 0)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            } else
            {
                Destroy(transform.GetChild(0).gameObject);
                //throw new UnityException("VFXManager should not be evoked in the game!");
            }
        }

        //if (materialPrototype.shader == Shader.Find("Paper") && materialPrototype.GetInt("_UseSprinkles") == 1) //C: No GetBool, need to use GetInt. Also this is broken lol
        //{
            for (int i = 0; i < sprinkleVerts.Length; i++)
            {
                var go = Instantiate(VFXManager.Theme.Sprinkle, transform);
                go.transform.localPosition = sprinkleVerts[i];
                go.transform.up = transform.rotation * sprinkleNorms[i];
                go.transform.RotateAround(go.transform.up, Random.Range(0, 360));
            }
        //}
        
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
}
