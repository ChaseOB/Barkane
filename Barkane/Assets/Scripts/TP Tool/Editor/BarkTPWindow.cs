using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class BarkTPWindow : EditorWindow
{
    enum TPMode
    {
        PAINT,
        SELECT,
        JOINT,
    }
    private RadioButton paintTool;
    private RadioButton selectTool;
    private RadioButton jointTool;

    private TPMode mode;

    [MenuItem("Tools/BarkTP")]
    public static void OpenWindow()
    {
        BarkTPWindow wnd = GetWindow<BarkTPWindow>();
        wnd.titleContent = new GUIContent("BarkTPWindow");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import and build the editor from the UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/TP Tool/Editor/BarkTPWindow.uxml");
        visualTree.CloneTree(root);

        paintTool = root.Query<RadioButton>("PaintTool");
        selectTool = root.Query<RadioButton>("SelectTool");
        jointTool = root.Query<RadioButton>("JointTool");

        SelectMode(TPMode.PAINT);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/TP Tool/Editor/BarkTPWindow.uss");
        root.styleSheets.Add(styleSheet);
    }

    private void SelectMode(TPMode mode)
    {
        this.mode = mode;
        switch (mode)
        {
            case TPMode.PAINT:
                paintTool.SetSelected(true);
                break;
            case TPMode.SELECT:
                selectTool.SetSelected(true);
                break;
            case TPMode.JOINT:
                jointTool.SetSelected(true);
                break;
        }
    }
}