using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedMesh
{
    [SerializeField, HideInInspector] byte[] serializedMeshData;

    public SerializedMesh(Mesh mesh)
    {
        mesh.Optimize();

        name = mesh.name;

        VerticesCount = (uint) mesh.vertexCount;
        IndicesCount = mesh.GetIndexCount(0);

        Vertices = mesh.vertices;
        Indices = mesh.triangles;
        Colors = mesh.colors;
        UVs = mesh.uv;
    }

    #region https://gist.github.com/zcyemi/b90171186c06e3fb1f24ed5336f60ead
    [SerializeField, HideInInspector] private string name;
    [SerializeField, HideInInspector] private uint VerticesCount;
    [SerializeField, HideInInspector] private uint IndicesCount;

    [SerializeField, HideInInspector] private Vector3[] Vertices;
    [SerializeField, HideInInspector] private Color[] Colors;
    [SerializeField, HideInInspector] private Vector3[] Normals;
    [SerializeField, HideInInspector] private Vector2[] UVs;

    [SerializeField, HideInInspector] private int[] Indices;

    public Mesh Rehydrated => new Mesh()
    {
        name = name,
        vertices = Vertices,
        normals = Normals,
        colors = Colors,
        uv = UVs,
        triangles = Indices
    };
    #endregion
}