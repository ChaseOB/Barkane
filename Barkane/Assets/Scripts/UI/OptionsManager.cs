using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public void SetMusicVolume(float val)
    {
        AudioManager.SetPlayerMusicVolume(val);
    }

    public void SetSFXVolume(float val)
    {
        AudioManager.SetPlayerSFXVolume(val);
    }

    public void SetSensitivity()
    {
        if(CameraOrbit.Instance != null)
            CameraOrbit.Instance.UpdateSensitivity();
    }
}
