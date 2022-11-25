using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnMeshIndex
{
    public static int[] Create(int resolution)
    {
        // main body: 5 segments, each is a ring of resolution quads, each quad 2 triangles, each triangle 3 vertices
        // 2 end caps: resolution triangles, each triangle 3 vertices
        var ids = new int[(5 - 1) * resolution * 2 * 3 + 2 * resolution * 3];
        var t = 0;

        // beginning cap
        for (int i = 0; i < resolution - 1; i++)
        {
            ids[t++] = 0;
            ids[t++] = 1 + i + 1;
            ids[t++] = 1 + i;
        }
        ids[t++] = 0;
        ids[t++] = 1;
        ids[t++] = resolution;

        // main cylinder
        for (int i = 0; i < (5 - 1); i++)
        {
            // first resolution - 1 quads
            var j = 0;
            for (; j < resolution - 1; j++)
            {
                ids[t++] = 1 + j + i * resolution;
                ids[t++] = 1 + j + i * resolution + 1;
                ids[t++] = 1 + j + i * resolution + resolution;
                ids[t++] = 1 + j + i * resolution + resolution;
                ids[t++] = 1 + j + i * resolution + 1;
                ids[t++] = 1 + j + i * resolution + resolution + 1;
            }
            // last quad loops back to j=0
            ids[t++] = 1 + j + i * resolution;
            ids[t++] = 1 + 0 + i * resolution;
            ids[t++] = 1 + j + i * resolution + 1;
            ids[t++] = 1 + j + i * resolution + 1;
            ids[t++] = 1 + j + i * resolution + resolution;
            ids[t++] = 1 + j + i * resolution;
        }

        // ending cap, order is reversed
        for (int i = 0; i < resolution - 1; i++)
        {
            ids[t++] = 5 * resolution + 1;
            ids[t++] = 1 + i + 4 * resolution;
            ids[t++] = 1 + i + 4 * resolution + 1;
        }
        ids[t++] = 5 * resolution + 1;
        ids[t++] = resolution + 4 * resolution;
        ids[t++] = 4 * resolution + 1;

        return ids;
    }
}
