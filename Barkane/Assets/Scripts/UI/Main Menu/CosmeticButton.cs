using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticButton : MonoBehaviour
{
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject button;
    [SerializeField] private GameObject selection;
    public string cosmeticName;
    public bool unlockDefault;

    private void Start() {
        if(unlockDefault)
            UnlockCosmetic(true);
    }

    public void UnlockCosmetic(bool unlock)
    {
        lockIcon.SetActive(!unlock);
        button.SetActive(unlock);
    }

    public void ToggleSelectionImage(bool toggle)
    {
        selection.SetActive(toggle);
    }
}
