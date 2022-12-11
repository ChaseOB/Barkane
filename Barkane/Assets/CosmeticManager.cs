using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticManager : Singleton<CosmeticManager>
{
    [SerializeField] private List<CosmeticButton> buttons = new List<CosmeticButton>();

    public string cosmeticString;

    private void Awake() {
        cosmeticString = PlayerPrefs.GetString("CurrentCosmetic", "None");
        foreach(CosmeticButton button in buttons) {
            int unlock = PlayerPrefs.GetInt($"CosmeticUnlock{button.cosmeticName}");
            if(unlock == 1 || button.cosmeticName == "None")
                button.UnlockCosmetic();
        }
    }

    public void SelectCosmetic(string cosmeticName)
    {
        foreach(CosmeticButton button in buttons) {
            button.ToggleSelectionImage(button.cosmeticName == cosmeticName);
        }

        cosmeticString = cosmeticName;

        PlayerPrefs.SetString("CurrentCosmetic", cosmeticName);
    }
}
