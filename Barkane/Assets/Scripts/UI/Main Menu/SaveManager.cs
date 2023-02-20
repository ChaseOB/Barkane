using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{

    //responsible for dealing with creating and loading saves
    private void Awake() {
        SaveSystem s = new SaveSystem(); //load save systme on game startup
    }

    public void SortSaveProfilesByTime() {
        SaveProfile[] profiles = new SaveProfile[SaveSystem.maxSaves];
        Array.Copy(SaveSystem.GetProfiles(), profiles, SaveSystem.maxSaves);
        printProfiles(profiles);
        Array.Sort(profiles, SaveProfile.SortMostRecent());
        printProfiles(profiles);
    }

    private void printProfiles(SaveProfile[] profiles)
    {
        foreach(SaveProfile s in profiles) {
            if(s == null)
                print("null");
            else
                print(s.GetProfileName());
        }
    }

}
