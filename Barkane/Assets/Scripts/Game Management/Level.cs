using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Barkane/Level")]
public class Level : ScriptableObject
{
    public string levelName;
    public GameObject levelObject;
    public Theme theme;
    public string cosmeticUnlock;
}
