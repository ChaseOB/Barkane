using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Barkane/Theme Asset")]
public class Theme : ScriptableObject
{
    public GameObject Sprinkle;
    public ThemeChoice themeEnum;

    [Header("Materials")]
    public Material WalkMat, WalkMatDark;
    public Material UnWalkMat;
    public Material crystalMat;
    
    [Header("Render Related")]
    public Color Silhouette;
    public Material Skybox;
}