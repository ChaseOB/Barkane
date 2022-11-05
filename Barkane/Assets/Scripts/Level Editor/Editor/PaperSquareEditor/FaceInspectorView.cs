using System;
using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

public class FaceInspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<FaceInspectorView, VisualElement.UxmlTraits> { }

    Editor editor;
    PaperSquareFace currFace;

    public FaceInspectorView()
    {

    }

    public void UpdateSelection(PaperSquareFace face)
    {
        ClearSelection();
        this.currFace = face;
        if (face != null)
        {
            editor = Editor.CreateEditor(face);
            Debug.Log(editor.GetType());
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