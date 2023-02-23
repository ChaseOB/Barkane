using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Auditor : Singleton<Auditor>
{
    public GameObject auditPanel;
    public TextMeshProUGUI auditText;

    private void Awake() {
        InitializeSingleton();
    }
    public string Audit()
    {
        SaveProfile[] profiles = new SaveProfile[SaveSystem.maxSaves];
        Array.Copy(SaveSystem.GetProfiles(), profiles, SaveSystem.maxSaves);
        Array.Sort(profiles, SaveProfile.SortMostLevelsFewestFolds());
        
        string ret = "Audit \n Winner: \n";

        foreach (SaveProfile s in profiles)
        {
            if(s!= null) {
                ret += s.GetProfileInfo();
                ret += "\n";
            }
        }

        Debug.Log(ret);
        auditText.text = ret;
        auditPanel.SetActive(true);
    
        return ret;
    }

    public void HidePanel() {
        auditPanel.SetActive(false);
    }
}
