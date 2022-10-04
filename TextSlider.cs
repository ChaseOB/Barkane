using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextSlider : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    private Slider slider;

    void Start() {
        slider = GetComponent<Slider>();
        setNumberText(slider.value);
    }

    public void setNumberText(float value) {
        numberText.text = value.ToString();
    }
}
