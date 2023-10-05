using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public void SetMusicVolume(int val)
    {
        AudioManager.SetPlayerMusicVolume(val);
    }

    public void SetSFXVolume(int val)
    {
        AudioManager.SetPlayerSFXVolume(val);
    }

    public void SetSensitivity()
    {
        if(CameraOrbit.Instance != null)
            CameraOrbit.Instance.UpdateSensitivity();
    }
}
