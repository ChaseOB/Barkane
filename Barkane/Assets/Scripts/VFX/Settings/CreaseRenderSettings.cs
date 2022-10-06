using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Barkane/Settings/Crease Render Settings")]
public class CreaseRenderSettings : ScriptableObject
{
    public Material creaseMaterial;
    [Range(0, 40)]
    public int creaseSegmentCount;
    public Vector3 creaseDeviation;
    [Range(0, 1)]
    public float tintCorrection;
}
