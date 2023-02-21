using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public List<GameObject> profileButtons;
    public GameObject newProfileButton;
    public GameObject saveProfileButton;

    public int firstVisIndex = 0;
    public int visibleProfiles = 3;
    public int moveAmount = 1;
    public Transform buttonParent;
    public List<Transform> profilePosTransforms;
    public GameObject cycleRightArrow;
    public GameObject cycleLeftArrow;
    private Dictionary<SaveProfile, int> profileToIndex = new Dictionary<SaveProfile, int>();

    //responsible for dealing with creating and loading saves
    private void Awake() {
        SaveSystem s = new SaveSystem(); //load save systme on game startup
    }

    private void Start() {
        CreateProfileButtons();
        ShowProfileButtons();
    }

    public void CreateProfileButtons() {
        SaveProfile[] profiles = SortSaveProfilesByTime();
        if(profiles[SaveSystem.maxSaves - 1] == null)
        {
            //if we have fewer than the max save profiles, add the "make new save profile" button
            GameObject button = Instantiate(newProfileButton, Vector3.zero, Quaternion.identity);
            button.SetActive(false);
            profileButtons.Add(button);
        }
        foreach(SaveProfile profile in profiles)
        {
            if(profile != null) {
                GameObject button = Instantiate(saveProfileButton, Vector3.zero, Quaternion.identity);
                button.transform.parent = buttonParent;
                button.GetComponent<SaveProfileButton>().SetProfile(profile, profileToIndex[profile]);
                button.SetActive(false);
                profileButtons.Add(button);
            }
        }
    }


    public SaveProfile[] SortSaveProfilesByTime() {
        SaveProfile[] profiles = new SaveProfile[SaveSystem.maxSaves];
        profileToIndex = new Dictionary<SaveProfile, int>();
        Array.Copy(SaveSystem.GetProfiles(), profiles, SaveSystem.maxSaves);
        for(int i = 0; i < profiles.Length; i++)
        {
            SaveProfile p = profiles[i];
            if(p != null)
                profileToIndex[p] = i;
        }
        Array.Sort(profiles, SaveProfile.SortMostRecent());
        return profiles;
    }

    public void CycleAndShowProfileButtons(int moveAmount)
    {
        CycleProfileButtons(moveAmount);
        cycleLeftArrow.SetActive(firstVisIndex > 0);
        cycleRightArrow.SetActive(firstVisIndex + visibleProfiles < SaveSystem.maxSaves);
        ShowProfileButtons();
    }

    private void CycleProfileButtons(int moveAmount)
    {
        int lastIndex = firstVisIndex + visibleProfiles - 1;
        firstVisIndex += moveAmount;
        if(firstVisIndex < 0)
            firstVisIndex = 0;
        if (firstVisIndex + visibleProfiles > SaveSystem.maxSaves)
            firstVisIndex = SaveSystem.maxSaves - visibleProfiles;
    }

    public void ShowProfileButtons()
    {
        foreach(GameObject button in profileButtons)
            button.SetActive(false);
        for(int i = firstVisIndex; i < firstVisIndex + visibleProfiles; i++) 
        {
            profileButtons[i].transform.position = profilePosTransforms[i - firstVisIndex].position;
            profileButtons[i].SetActive(true);
        }
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
