using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class TileTypeSelectorWindow : EditorWindow
{
    private Dictionary<TileType, RadioButton> tileTypeButtons;

    [MenuItem("Tools/Tile Typer Selector")]
    public static void OpenWindow()
    {
        TileTypeSelectorWindow wnd = GetWindow<TileTypeSelectorWindow>();
        wnd.titleContent = new GUIContent("BarkTPWindow");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import and build the editor from the UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/TileTypeSelector/TileTypeSelectorWindow.uxml");
        visualTree.CloneTree(root);

        tileTypeButtons = new Dictionary<TileType, RadioButton>();
        tileTypeButtons[TileType.WALKABLE] = root.Query<RadioButton>("Walkable");
        tileTypeButtons[TileType.UNWALKABLE] = root.Query<RadioButton>("Unwalkable");

        SelectTileType(TileType.WALKABLE);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/TileTypeSelector/TileTypeSelectorWindow.uss");
        root.styleSheets.Add(styleSheet);

        //foreach (RadioButton button in tileTypeButtons.Values)
        //{
        //    button.RegisterCallback<ChangeEvent<bool>>(null);   //CHANGE
        //}
    }

    private void GetTypeOfButton(RadioButton button)
    {

    }

    private void SelectTileType(TileType tileType)
    {
        tileTypeButtons[tileType].SetSelected(true);
        if (SquareSelector.SelectedSquare != null)
        {
            SquareSelector.SelectedSquare.ChangeTileType(tileType);
        }
    }
}