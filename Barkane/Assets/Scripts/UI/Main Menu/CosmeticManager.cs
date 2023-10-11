using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticManager : Singleton<CosmeticManager>
{
    [SerializeField] private List<CosmeticButton> buttons = new List<CosmeticButton>();

    public string cosmeticString;
    public PlayerCosmetics playerCosmetics;

    private void Awake() {
        InitializeSingleton();
    }

    public void SetCosmetics()
    {
        cosmeticString = SaveSystem.Current.GetCosmetic(); 
        Dictionary<string, bool> cosmeticDict = SaveSystem.Current.GetCosmeticsDictionary();
        foreach(CosmeticButton button in buttons) 
            if(cosmeticDict.GetValueOrDefault(button.cosmeticName, false))
                button.UnlockCosmetic();
    }

    public void SelectCosmetic(string cosmeticName)
    {
        if(cosmeticName == "" || cosmeticName == null)
            cosmeticName = "None";
        foreach(CosmeticButton button in buttons) {
            button.ToggleSelectionImage(button.cosmeticName.Equals(cosmeticName));
        }

        cosmeticString = cosmeticName;

        SaveSystem.Current.SetCosmetic(cosmeticName);
        SaveSystem.SaveGame();
        playerCosmetics.EnableCosmetic(cosmeticName);
    }

    public void OnOpen()
    {
        SelectCosmetic(SaveSystem.Current.GetCosmetic());
    }
}
