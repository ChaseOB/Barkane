using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Barkane/Theme Asset")]
public class Theme : ScriptableObject
{
    public GameObject Sprinkle;
    public ThemeChoice themeEnum;
    public string musicStringName;

    [Header("Materials")]
    public Material WalkMat;
    public Color WalkColor, WalkTint;
    public Material UnWalkMat;
    public Color UnwalkColor, UnwalkTint;
    public Material JointParticle;
    public Material crystalMat;
    
    [Header("Render Related")]
    public Color Silhouette;

    public Material GhostMat => m_GhostMat;
    private Material m_GhostMat;
    public Material GhostMatPrototype;

    private void OnEnable()
    {
        m_GhostMat = new Material(GhostMatPrototype);
        m_GhostMat.SetColor("_Color", Silhouette);
    }
}