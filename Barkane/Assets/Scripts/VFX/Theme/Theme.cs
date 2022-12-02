using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Barkane/Theme Asset")]
public class Theme : ScriptableObject
{
    public GameObject Sprinkle;

    [Header("Materials")]
    public Material WalkMat;
    public Material UnWalkMat;
    public Material JointParticle;
    public Gradient JointParticleGrad1;
    public Gradient JointParticleGrad2;
}