using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareSide : MonoBehaviour
{
    MeshFilter meshFilter;
    [SerializeField] private FractureMeshSetting meshSettings;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = FractureMesh.Create(meshSettings);

        StartCoroutine(RefreshMesh());
    }

    private IEnumerator RefreshMesh()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            meshFilter.mesh = FractureMesh.Create(meshSettings);
        }
    }
}
