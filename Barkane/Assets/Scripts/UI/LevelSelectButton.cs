using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    public GameObject button;
    public GameObject lockImage;
    public GameObject levelText;
    public Level level;

    public StarsUI starsUI;

    private bool adminMode = true;

    private void OnEnable() {
        CheckUnlock();
        CheckStars();
    }

    public void CheckUnlock() {
        if(SaveSystem.Current == null) 
            return;
        if(adminMode || SaveSystem.Current.GetLevelUnlocksDictionary().GetValueOrDefault(level.levelName, false)) {
            SetLevelUnlocked(true);
            return;
        }
        SetLevelUnlocked(false);
    }

    public void SetLevelUnlocked(bool val) {
        button.SetActive(val);
        lockImage.SetActive(!val);
        levelText.SetActive(val);
    }
    
    public void CheckStars()
    {
        int numFolds = SaveSystem.Current.GetFolds(level.levelName);
        print(level.levelName + " " + numFolds);
        starsUI.DisplayStars(level, numFolds);
    }

    public void LoadLevel() {
        LevelManager.Instance.LoadLevel(level.levelName);
    }
}
