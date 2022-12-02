using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Barkane/Settings/Crease Render Settings")]
public class CreaseRenderSettings : ScriptableObject, IDynamicMeshRenderSettings
{
    public Material creaseMaterial;
    [Range(0, 40)]
    public int creaseSegmentCount;
    public Vector3 creaseDeviation;
    [Range(0, 0.1f)]
    public float overlapCreaseCorrectionFactor;

    public int VCount => 2 * (creaseSegmentCount + 1);

    // buffer index constants
    public int SubmeshOffset => 2 * (creaseSegmentCount + 1);
    public int PivotOffset => creaseSegmentCount + 1;

    [SerializeField, HideInInspector]
    public (int[], int[], int[], int[]) GetTris(bool useCCW)
    {
        // 3 points per triangle
        // 4 stripes per mesh
        // 2 triangles per each segment on each stripe
        var trisA1 = new int[3 * 2 * creaseSegmentCount];
        var trisA2 = new int[3 * 2 * creaseSegmentCount];
        var trisB1 = new int[3 * 2 * creaseSegmentCount];
        var trisB2 = new int[3 * 2 * creaseSegmentCount];

        var triOffset = 3 * 2 * creaseSegmentCount;

        if (useCCW)
        {
            for (int i = 0, j = 0; i < creaseSegmentCount; i++, j += 3 * 2)
            {
                trisA1[j + 0] = i;
                trisA1[j + 1] = i + 1;
                trisA1[j + 2] = i + PivotOffset;
                trisA1[j + 3] = i + 1;
                trisA1[j + 4] = i + PivotOffset + 1;
                trisA1[j + 5] = i + PivotOffset;

                trisB1[j + 0] = i;
                trisB1[j + 1] = i + PivotOffset;
                trisB1[j + 2] = i + 1;
                trisB1[j + 3] = i + 1;
                trisB1[j + 4] = i + PivotOffset;
                trisB1[j + 5] = i + PivotOffset + 1;

                trisA2[j + 0] = i;
                trisA2[j + 1] = i + PivotOffset;
                trisA2[j + 2] = i + 1;
                trisA2[j + 3] = i + 1;
                trisA2[j + 4] = i + PivotOffset;
                trisA2[j + 5] = i + PivotOffset + 1;

                trisB2[j + 0] = i;
                trisB2[j + 1] = i + 1;
                trisB2[j + 2] = i + PivotOffset;
                trisB2[j + 3] = i + 1;
                trisB2[j + 4] = i + PivotOffset + 1;
                trisB2[j + 5] = i + PivotOffset;
            }
        }
        else
        {
            for (int i = 0, j = 0; i < creaseSegmentCount; i++, j += 3 * 2)
            {
                trisA1[j + 0] = i;
                trisA1[j + 1] = i + PivotOffset;
                trisA1[j + 2] = i + 1;
                trisA1[j + 3] = i + 1;
                trisA1[j + 4] = i + PivotOffset;
                trisA1[j + 5] = i + PivotOffset + 1;

                trisB1[j + 0] = i;
                trisB1[j + 1] = i;
                trisB1[j + 2] = i + PivotOffset;
                trisB1[j + 3] = i + 1;
                trisB1[j + 4] = i + PivotOffset + 1;
                trisB1[j + 5] = i + PivotOffset;

                trisA2[j + 0] = i;
                trisA2[j + 1] = i + 1;
                trisA2[j + 2] = i + PivotOffset;
                trisA2[j + 3] = i + 1;
                trisA2[j + 4] = i + PivotOffset + 1;
                trisA2[j + 5] = i + PivotOffset;

                trisB2[j + 0] = i;
                trisB2[j + 1] = i + PivotOffset;
                trisB2[j + 2] = i + 1;
                trisB2[j + 3] = i + 1;
                trisB2[j + 4] = i + PivotOffset;
                trisB2[j + 5] = i + PivotOffset + 1;
            }
        }

        return (trisA1, trisA2, trisB1, trisB2);
    }

    [HideInInspector]
    public int[]
        tA1CCW, tA2CCW, tB1CCW, tB2CCW,
        tA1CW, tA2CW, tB1CW, tB2CW;

    [HideInInspector] public float[] ts;
}

[CustomEditor(typeof(CreaseRenderSettings))]
public class CreaseRenderSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Bake Indices"))
        {
            var t = target as CreaseRenderSettings;
            var CCW = t.GetTris(true);
            t.tA1CCW = CCW.Item1;
            t.tB1CCW = CCW.Item2;
            t.tA2CCW = CCW.Item3;
            t.tB2CCW = CCW.Item4;
            var CW = t.GetTris(false);
            t.tA1CW = CW.Item1;
            t.tB1CW = CW.Item2;
            t.tA2CW = CW.Item3;
            t.tB2CW = CW.Item4;

            t.ts = new float[t.creaseSegmentCount + 1];
            for (int i = 0; i < t.creaseSegmentCount + 1; i++)
            {
                t.ts[i] = (float)i / t.creaseSegmentCount - .5f;
            }
            t.ts[t.creaseSegmentCount] = .5f;

            Debug.Log(string.Join(", ", t.ts));

            EditorUtility.SetDirty(target as CreaseRenderSettings);
        }
    }
}