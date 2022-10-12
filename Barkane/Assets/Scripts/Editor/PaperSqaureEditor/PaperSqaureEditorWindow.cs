using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaperSqaureEditorWindow : EditorWindow
{
    //private RadioButtonGroup typeButtonGroup;
    //private Dictionary<TileType, RadioButton> tileTypeButtons;

    private FaceInspectorView _topInspector;
    private FaceInspectorView _bottomInspector;

    private void OnEnable()
    {
        SquareSelector.onSquareSelected += UpdateSqaureSelection;
    }

    private void OnDisable()
    {
        SquareSelector.onSquareSelected -= UpdateSqaureSelection;
    }

    [MenuItem("Tools/Paper Sqaure Editor")]
    public static void OpenWindow()
    {
        PaperSqaureEditorWindow wnd = GetWindow<PaperSqaureEditorWindow>();
        wnd.titleContent = new GUIContent("Paper Sqaure Editor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Import and build the editor from the UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/PaperSqaureEditor/PaperSqaureEditorWindow.uxml");
        visualTree.CloneTree(root);

        //typeButtonGroup = root.Query<RadioButtonGroup>("TileTypes");
        //typeButtonGroup.RegisterValueChangedCallback(RadioButtonGroupCallback);

        _topInspector = root.Query<FaceInspectorView>("TopInspector");
        _bottomInspector = root.Query<FaceInspectorView>("BottomInspector");

        UpdateSqaureSelection(SquareSelector.SelectedSquare);

        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/PaperSqaureEditor/PaperSqaureEditorWindow.uss");
        root.styleSheets.Add(styleSheet);
    }

    void UpdateSqaureSelection(PaperSqaure sqaure)
    {
        //string sqaureName = sqaure == null ? "null" : sqaure.gameObject.name;
        //Debug.Log($"Updated Sqaure Selection to {sqaureName} ");

        if (sqaure != null)
        {
            _topInspector.UpdateSelection(sqaure.TopHalf.GetComponent<PaperSqaureFace>());
            _bottomInspector.UpdateSelection(sqaure.BottomHalf.GetComponent<PaperSqaureFace>());
        } else
        {
            _topInspector.UpdateSelection(null);
            _bottomInspector.UpdateSelection(null);
        }

    }

    //private void RadioButtonGroupCallback(ChangeEvent<int> e)
    //{
    //    Debug.Log($"Clicked Radio Button: {(FaceType) e.newValue}");
    //    SelectTileType( (FaceType) e.newValue);
    //}

    //private void SelectTileType(FaceType tileType)
    //{
    //    SquareSelector.ChangeSelectedTileType(tileType);
    //}
}