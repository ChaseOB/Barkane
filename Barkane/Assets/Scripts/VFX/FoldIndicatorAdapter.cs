using BarkaneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FoldIndicatorAdapter : MonoBehaviour
{
    [SerializeField] private MeshRenderer mr;

    private void OnEnable()
    {
        mr.sharedMaterial = VFXManager.Theme.crystalMat;
    }
}
