using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticManager : Singleton<CosmeticManager>
{
    [SerializeField] private List<CosmeticButton> buttons = new List<CosmeticButton>();

    public string cosmeticString;

    public void SetCosmetics()
    {
        cosmeticString = SaveSystem.Current.GetCosmetic(); 
        Dictionary<string, bool> cosmeticDict = SaveSystem.Current.GetCosmeticsDictionary();
        foreach(CosmeticButton button in buttons) 
            if(cosmeticDict.GetValueOrDefault(button.cosmeticName))
                button.UnlockCosmetic();
    }
    
    public void SelectCosmetic(string cosmeticName)
    {
        foreach(CosmeticButton button in buttons) {
            button.ToggleSelectionImage(button.cosmeticName == cosmeticName);
        }

        cosmeticString = cosmeticName;

        SaveSystem.Current.SetCosmetic(cosmeticName);
    }
}
