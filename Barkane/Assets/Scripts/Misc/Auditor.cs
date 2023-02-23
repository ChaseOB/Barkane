using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Auditor : Singleton<Auditor>
{
    private void Awake() {
        InitializeSingleton();
    }
    public string Audit()
    {
        SaveProfile[] profiles = new SaveProfile[SaveSystem.maxSaves];
        Array.Copy(SaveSystem.GetProfiles(), profiles, SaveSystem.maxSaves);
        Array.Sort(profiles, SaveProfile.SortMostLevelsFewestFolds());
        
        string ret = "";

        foreach (SaveProfile s in profiles)
        {
            ret += s.GetProfileInfo();
            ret += "\n";
        }

        Debug.Log(ret);

        MainMenuManager.Instance.ShowAudit(ret);
        return ret;
    }
}
