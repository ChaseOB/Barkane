using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using BarkaneEditor;

public class ColorCorrector : MonoBehaviour
{
    public Color Target;
    public Material materialTarget;
}

#if UNITY_EDITOR
[CustomEditor(typeof(ColorCorrector))]
public class GammaCorrectorEditor : Editor
{
    private Color sample;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = (target as ColorCorrector).Target;
        var mat = (target as ColorCorrector).materialTarget;

        if (GUILayout.Button("Reset to target"))
        {
            mat.SetColor("_Color", t);
            VFXManager.Instance.Refresh<SquareSide>();
        }
        GUILayout.Label("Sample the lightest color on the paper after refresh:");
        sample = EditorGUILayout.ColorField(sample);
        GUILayout.Label($"Error: {t.r - sample.r}, {t.g - sample.g}, {t.b - sample.b}");

        var corr = new Color(t.r * t.r / sample.r, t.g * t.g / sample.g, t.b * t.b / sample.b);
        GUILayout.Label("Corrected: ");
        EditorGUILayout.ColorField(corr);
        if (GUILayout.Button("Correct target color"))
        {
            mat.SetColor("_Color", corr);
            VFXManager.Instance.Refresh<SquareSide>();
        }
    }
}

#endif