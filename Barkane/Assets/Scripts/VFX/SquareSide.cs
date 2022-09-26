using BarkaneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SquareSide : MonoBehaviour, IRefreshable
{
    [SerializeField] MeshFilter mFilter;
    [SerializeField] MeshRenderer mRenderer;
    [SerializeField] CrumbleMeshGenerator meshGenerator;
    [SerializeField] Material materialPrototype;
    [SerializeField, HideInInspector] Material materialCurrent;
    
    void IRefreshable.Refresh()
    {
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        var (mesh, material) = meshGenerator.Create(materialPrototype);
        mFilter.mesh = mesh;
        materialCurrent = material;
        mRenderer.sharedMaterials = new Material[] { materialCurrent };
    }
}
