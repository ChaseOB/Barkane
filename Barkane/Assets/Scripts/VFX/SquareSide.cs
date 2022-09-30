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

    public Material MaterialPrototype => materialPrototype;
    
    void IRefreshable.Refresh()
    {
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        var (mesh, material) = meshGenerator.Create(materialPrototype);
        mFilter.mesh = mesh;
        mRenderer.sharedMaterials = new Material[] { material };
    }


    public (int, int, int) Coordinate => (
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y),
        Mathf.RoundToInt(transform.position.z));

    public static implicit operator (int, int, int)(SquareSide s) => s.Coordinate;
}
