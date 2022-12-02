using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneEditor;

[RequireComponent(typeof(FoldablePaper))]
public class VFXThemeAdapter : MonoBehaviour, IThemedItem
{
    [SerializeField] private Material silhouette;
    [HideInInspector, SerializeField] private Theme bakedTheme;
    
    public void UpdateTheme(Theme theme)
    {
        bakedTheme = theme;
    }

    private void Start()
    {
        // var skybox = Camera.main.GetComponent<Skybox>();
        silhouette.SetColor("_BaseColor", bakedTheme.Silhouette);
        // skybox.material = bakedTheme.Skybox;
    }
}
