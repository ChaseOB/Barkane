using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PaperSquareEditorWindow : EditorWindow
{
    //private RadioButtonGroup typeButtonGroup;
    //private Dictionary<TileType, RadioButton> tileTypeButtons;

    private FaceInspectorView _topInspector;
    private FaceInspectorView _bottomInspector;

    private void OnEnable()
    {
        SquareSelector.onSquareSelected += UpdateSquareSelection;
    }

    private void OnDisable()
    {
        SquareSelector.onSquareSelected -= UpdateSquareSelection;
    }

    [MenuItem("Tools/Paper Square Editor")]
    public static void OpenWindow()
    {
        PaperSquareEditorWindow wnd = GetWindow<PaperSquareEditorWindow>();
        wnd.titleContent = new GUIContent("Paper Square Editor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Import and build the editor from the UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/PaperSquareEditor/PaperSquareEditorWindow.uxml");
        visualTree.CloneTree(root);

        //typeButtonGroup = root.Query<RadioButtonGroup>("TileTypes");
        //typeButtonGroup.RegisterValueChangedCallback(RadioButtonGroupCallback);

        _topInspector = root.Query<FaceInspectorView>("TopInspector");
        _bottomInspector = root.Query<FaceInspectorView>("BottomInspector");

        UpdateSquareSelection(SquareSelector.SelectedSquare);

        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/PaperSquareEditor/PaperSquareEditorWindow.uss");
        root.styleSheets.Add(styleSheet);
    }

    void UpdateSquareSelection(PaperSquare square)
    {
        //string squareName = square == null ? "null" : square.gameObject.name;
        //Debug.Log($"Updated Square Selection to {squareName} ");

        if (square != null)
        {
            _topInspector.UpdateSelection(square.TopHalf.GetComponent<PaperSquareFace>());
            _bottomInspector.UpdateSelection(square.BottomHalf.GetComponent<PaperSquareFace>());
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