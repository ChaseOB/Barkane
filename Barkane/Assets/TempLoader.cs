using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempLoader : MonoBehaviour
{
    public string profileName;
    public int loadIndex;

    private void Awake() {
        SaveSystem s = new SaveSystem();
    }
    public void MakeProfile()
    {
        int ind = SaveSystem.CreateNewProfile(profileName);
        SaveSystem.SetMostRecentProfile();
    }

    public void LoadProfile()
    {
        SaveSystem.SetCurrentProfile(loadIndex);
    }

}
