using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCosmetics : Singleton<PlayerCosmetics>
{
    [SerializeField] private List<GameObject> CosmeticGOs = new List<GameObject>();
    [SerializeField] private List<string> CosmeticNames = new List<string>();

    private GameObject enabledCosmetic;
    private Dictionary<string, GameObject> cosmeticDict = new Dictionary<string, GameObject>();

    private void Awake() {
        InitializeSingleton(this.gameObject);

        if(CosmeticGOs.Count != CosmeticNames.Count)
            Debug.LogWarning("Cosmetic GO and names counts are not equal");

        cosmeticDict = new Dictionary<string, GameObject>();
        for(int i = 0; i < CosmeticNames.Count; i++) 
            cosmeticDict.Add(CosmeticNames[i], CosmeticGOs[i]);
        
        EnableCosmetic(PlayerPrefs.GetString("CurrentCosmetic", "None"));
    }

    public void EnableCosmetic(string name)
    {
        if(name == "None") return;
        
        enabledCosmetic?.SetActive(false);
        GameObject go = cosmeticDict[name];
        if(go == null) {
            Debug.LogWarning("Invalid Cosmetic Name");
            return;
        }
        enabledCosmetic = go;
        enabledCosmetic.SetActive(true);
    }

    public void DisableAllCosmetics()
    {
        enabledCosmetic?.SetActive(false);
        enabledCosmetic = null;
    }
}


