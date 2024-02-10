using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSFX : MonoBehaviour
{
    public void OnHover(string value)
    {
        AudioManager.Instance.Play(value != "" ? value : "Button Hover");
    }

    public void OnClick(string value)
    {
        AudioManager.Instance.Play(value != "" ? value : "Button Click");
    }

    public void OnLeave(string value)
    {
        AudioManager.Instance.Play(value != "" ? value : "Button Leave");
    }
}
