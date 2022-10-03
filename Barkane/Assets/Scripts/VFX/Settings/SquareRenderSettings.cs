using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Barkane/Settings/Square Render Settings", fileName = "Square Render Settings")]
public class SquareRenderSettings : ScriptableObject
{
    [Range(0f, 1f)]
    public float margin;
    public Material creaseMaterial;
}
