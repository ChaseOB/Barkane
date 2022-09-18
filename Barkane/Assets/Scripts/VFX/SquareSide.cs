using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareSide : MonoBehaviour
{
    MeshFilter mFilter;
    MeshRenderer mRenderer;
    CrumbleMeshGenerator meshGenerator;

    // Start is called before the first frame update
    void Start()
    {
        mFilter = GetComponent<MeshFilter>();
        mRenderer = GetComponent<MeshRenderer>();
        meshGenerator = GetComponent<CrumbleMeshGenerator>();

        UpdateMesh();

        StartCoroutine(RefreshMesh());
    }

    private IEnumerator RefreshMesh()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            UpdateMesh();
        }
    }

    private void UpdateMesh()
    {
        var (mesh, material) = meshGenerator.Create(mRenderer.material);
        mFilter.mesh = mesh;
        mRenderer.sharedMaterials = new Material[] { material };
    }
}
