using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(SceneManager))]
public class SceneEditor : Editor
{
    public VisualTreeAsset managerXML;

    // TODO: Finalize InspectorGUI to include tile walkability
    /*
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement customInspector = new VisualElement();
        managerXML.CloneTree(customInspector);
        return customInspector;
    }
    */
}
