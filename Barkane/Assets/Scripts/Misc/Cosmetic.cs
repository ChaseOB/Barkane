using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Barkane/Cosmetic")]
public class Cosmetic : ScriptableObject
{
    public string cosmeticName;
    public GameObject cosmeticPrefab;
    public bool unlocked;
    public Sprite unlockOverrideSprite;
}
