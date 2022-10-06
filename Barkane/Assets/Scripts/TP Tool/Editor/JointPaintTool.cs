using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Place Joints Tool")]
class JointPaintTool : EditorTool
{
    [SerializeField] private Texture2D toolIcon;

    private GUIContent _iconContent;

    private void OnEnable()
    {
        _iconContent = new GUIContent()
        {
            image = toolIcon,
            text = "Joint Paint Tool",
            tooltip = "Paint Joints Between Squares"
        };
    }
}

