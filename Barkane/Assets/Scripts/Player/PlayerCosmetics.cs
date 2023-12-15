using System;
using System.Collections.Generic;
using UnityEngine;
public class PlayerCosmetics : Singleton<PlayerCosmetics>
{
    [SerializeField] private List<GameObject> CosmeticGOs = new List<GameObject>();
    [SerializeField] private List<string> CosmeticNames = new List<string>();

    public Material PlayerMat;
    public Material BloodyMat;
    public MeshRenderer meshRenderer;

    private GameObject enabledCosmetic;
    private Dictionary<string, GameObject> cosmeticDict = new Dictionary<string, GameObject>();

    private void Awake() {
        InitializeSingleton(gameObject);

        if(CosmeticGOs.Count != CosmeticNames.Count)
            Debug.LogWarning("Cosmetic GO and names counts are not equal");

        cosmeticDict = new Dictionary<string, GameObject>();
        for(int i = 0; i < CosmeticNames.Count; i++) 
            cosmeticDict.Add(CosmeticNames[i], CosmeticGOs[i]);
        
        EnableCosmetic(SaveSystem.Current?.GetCosmetic());
    }

    public void EnableCosmetic(string name)
    {
        if(name == null || name == "None" || name == "") 
        { 
            DisableAllCosmetics();
            return;
        }
        
        enabledCosmetic?.SetActive(false);
        GameObject go = cosmeticDict[name];
        if(go == null) {
            Debug.LogWarning("Invalid Cosmetic Name");
            return;
        }
        enabledCosmetic = go;
        enabledCosmetic.SetActive(true);
        meshRenderer.material = name == "knife" ? BloodyMat : PlayerMat;

        // var c = Cosmetic.CosmeticEnum.GLASSES;
        // string s = c.ToString();
        // Cosmetic.CosmeticEnum e = (Cosmetic.CosmeticEnum) Enum.Parse(typeof(Cosmetic.CosmeticEnum), s);
    }

    public void DisableAllCosmetics()
    {
        enabledCosmetic?.SetActive(false);
        enabledCosmetic = null;
    }

    // public void DisableCosmetic(Cosmetic.CosmeticEnum)
    // {

    // }
}

[System.Serializable]
public class CosmeticData
{
    public Cosmetic.CosmeticEnum cosmeticEnum;
    public GameObject gameObject;
    public Texture2D playerTex;
}


