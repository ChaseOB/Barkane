using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfileScreenManager : MonoBehaviour
{
    public GameObject profileMenu;
    public GameObject levelSelect;
    public GameObject cosmetics;
    public GameObject deleteMenu;


    public void ToggleLevelSelect(bool val)
    {
        levelSelect.SetActive(val);
        profileMenu.SetActive(!val);
    }

    public void ToggleDeleteConfirm(bool val)
    {
        deleteMenu.SetActive(val);
        profileMenu.SetActive(!val);
    }
}
