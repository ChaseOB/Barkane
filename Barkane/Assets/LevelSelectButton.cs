using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    public GameObject button;
    public GameObject lockImage;
    public GameObject levelText;
    public string levelName;

    private void OnEnable() {
        CheckUnlock();
    }

    public void CheckUnlock() {
        if(SaveSystem.Current == null) 
            return;
        if(SaveSystem.Current.GetLevelUnlocksDictionary().GetValueOrDefault(levelName, false)) {
            print($"level {levelName} unlocked");
            SetLevelUnlocked(true);
            return;
        }
        print("Level not unlocked");
        SetLevelUnlocked(false);
    }

    public void SetLevelUnlocked(bool val) {
        button.SetActive(val);
        lockImage.SetActive(!val);
        levelText.SetActive(val);
    }

    public void LoadLevel() {
        LevelManager.Instance.LoadLevel(levelName);
    }
}
