using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempLoader : MonoBehaviour
{
    public string profileName;
    public int loadIndex;

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
