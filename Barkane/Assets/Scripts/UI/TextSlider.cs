using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextSlider : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    private Slider slider;
    public string text;
    public string saveString;

    void Start() {
        slider = GetComponentInChildren<Slider>();
        slider.value = PlayerPrefs.HasKey(saveString) ? PlayerPrefs.GetInt(saveString) : 50;
        setNumberText((int) slider.value);
    }

    public void setNumberText(int value) {
        numberText.text = text + value.ToString() + "%";
        PlayerPrefs.SetInt(saveString, value);
    }
}
