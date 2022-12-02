using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Barkane/Theme Asset")]
public class Theme : ScriptableObject
{
    public GameObject Sprinkle;
    public ThemeChoice themeEnum;

    [Header("Materials")]
    public Material WalkMat;
    public Material UnWalkMat;
    public Material JointParticle;
    public Material crystalMat;

}