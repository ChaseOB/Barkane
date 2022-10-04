using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Barkane/Settings/Crease Render Settings")]
public class CreaseRenderSettings : ScriptableObject
{
    public Material creaseMaterial;
    [Range(0, 40)]
    public int creaseSegmentCount;
    [Range(0, 0.1f)]
    public float creaseElevation;
}
