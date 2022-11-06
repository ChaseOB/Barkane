using System;
using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

public class FaceInspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<FaceInspectorView, VisualElement.UxmlTraits> { }

    Editor editor;

    public FaceInspectorView()
    {

    }

    public void UpdateSelection(PaperSquareFace face)
    {
        ClearSelection();
        if (face != null)
        {
            editor = Editor.CreateEditor(face);
            IMGUIContainer container = new IMGUIContainer(() => editor.OnInspectorGUI());
            Add(container);
        }
    }

    public void ClearSelection()
    {
        Clear();
        this.Unbind();
        UnityEngine.Object.DestroyImmediate(editor);
    }
}