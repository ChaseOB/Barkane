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

    public Material MaterialPrototype => materialPrototype;

    public (Vector3[], Vector3[]) sprinkles;
    
    void IRefreshable.Refresh()
    {
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        var (mesh, material, sprinkleVerts, sprinkleNorms) = meshGenerator.Create(materialPrototype);
        mFilter.mesh = mesh;
        mRenderer.sharedMaterials = new Material[] { material };

        while(transform.childCount > 0)
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


    public (int, int, int) Coordinate => (
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y),
        Mathf.RoundToInt(transform.position.z));

    public static implicit operator (int, int, int)(SquareSide s) => s.Coordinate;
}
