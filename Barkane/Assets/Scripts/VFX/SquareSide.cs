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
    [SerializeField] Material materialPrototype;

    [SerializeField, HideInInspector] Texture2D distanceTexture;
    [SerializeField, HideInInspector] Material materialInstance;

    public Material MaterialPrototype => materialPrototype;

    public (Vector3[], Vector3[]) sprinkles;

    void IRefreshable.Refresh()
    {
        UpdateMesh();
    }

    private void Start()
    {
        PushMaterial();
    }

    private void PushMaterial()
    {
        if (materialInstance != null &&  distanceTexture != null)
        {
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
        var (mesh, material, texture, sprinkleVerts, sprinkleNorms) = meshGenerator.Create(materialPrototype);
        mFilter.sharedMesh = mesh;
        distanceTexture = texture;
        materialInstance = material;
        PushMaterial();

        while (transform.childCount > 0)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            } else
            {
                throw new UnityException("VFXManager should not be evoked in the game!");
            }
        }

        for (int i = 0; i < sprinkleVerts.Length; i++)
        {
            var go = Instantiate(VFXManager.Theme.Sprinkle, transform);
            go.transform.localPosition = sprinkleVerts[i];
            go.transform.up = transform.rotation * sprinkleNorms[i];
        }
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
