using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TFOC/MeshSettings/FractureMeshSetting")]
public class FractureMeshSetting : ScriptableObject
{
    [Range(0, 0.5f)] public float margin;
    [Range(0, 0.5f)] public float height;
    [Range(0, 1)] public float triangleArea;
}
