using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

public class FaceInspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<FaceInspectorView, VisualElement.UxmlTraits> { }

    Editor editor;
    PaperSqaureFace currFace;

    public FaceInspectorView()
    {

    }

    public void UpdateSelection(PaperSqaureFace face, bool isTopHalf)
    {
        Debug.Log($"Update Selection of Face: {face.gameObject.name}");
        ClearSelection();
        this.currFace = face;
        if (face != null)
        {
            editor = Editor.CreateEditor(face);
            IMGUIContainer container = new IMGUIContainer(() => editor.OnInspectorGUI());
            SerializedProperty faceTypeProperty = editor.serializedObject.FindProperty("faceType");
            Add(container);
        }
    }

    public void ClearSelection()
    {
        Clear();
        this.Unbind();
        UnityEngine.Object.DestroyImmediate(editor);
    }

    //Called when user changes a property in the inspector.
    private void OnPropertyChanged(SerializedProperty property)
    {

    }
}