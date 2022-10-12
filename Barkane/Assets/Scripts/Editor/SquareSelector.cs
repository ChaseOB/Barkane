using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Select Square", typeof(LevelEditorManager))]
public class SquareSelector : EditorTool
{
    [SerializeField] private Texture2D toolIcon;    //L: You can't even assign this lmao.

    //static bc I want to access it from other things
    public static PaperSquare SelectedSquare { get; private set; }

    private LevelEditorManager levelEditor;

    private GUIContent _toolbarIcon;
    public override GUIContent toolbarIcon => _toolbarIcon;

    public delegate void OnSquareSelected(PaperSquare squareSelected);
    public static event OnSquareSelected onSquareSelected;

    private void Awake()
    {
        SelectedSquare = null;
    }

    private void OnEnable()
    {
        _toolbarIcon = new GUIContent()
        {
            image = toolIcon,
            text = "Select Square",
            tooltip = "Select Squares to change its type or add joints."
        };

        levelEditor = target as LevelEditorManager;
        SelectedSquare = null;
    }

    public override void OnWillBeDeactivated()
    {
        base.OnWillBeDeactivated();
        SelectedSquare = null;
    }

    // TODO: Use Handles to deal with UnityGUI events
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
        {
            // Current window is not a SceneView.
            return;
        }

        DrawHandles();

        Event e = Event.current;

        if (!e.isMouse)
        {
            // Not a mouse event.
            return;
        }

        int id = GUIUtility.GetControlID(FocusType.Passive);
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (e.button == 0)
        {
            HandleLeftClick(e, id, mouseRay);
        }
        else if (e.button == 1)
        {
            HandleRightClick(e, id, mouseRay);
        }
    }

    private void DrawHandles()
    {
        if (SelectedSquare != null)
        {
            Vector3 pos = SelectedSquare.transform.position;
            Vector3 size = new Vector3(SelectedSquare.paperLength, SelectedSquare.paperThickness, SelectedSquare.paperLength);
            Handles.color = Color.black;
            Handles.DrawWireCube(pos, size);
        }
    }

    private void HandleLeftClick(Event e, int id, Ray mouseRay)
    {
        // Left click start
        if (e.type == EventType.MouseDown)
        {
            GUIUtility.hotControl = id;
            SelectedSquare = levelEditor.GetSquareClicked(mouseRay);
            Debug.Log(SelectedSquare == null ? $"DESELECTED SQUARE" : $"SELECTED SQUARE: {SelectedSquare}");

            onSquareSelected?.Invoke(SelectedSquare);

            e.Use();
        }

        // Left click drag
        else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id)
        {
            e.Use();
        }

        // Left click end
        else if (e.type == EventType.MouseUp && GUIUtility.hotControl == id)
        {
            GUIUtility.hotControl = 0;
            e.Use();
        }
    }

    private void HandleRightClick(Event e, int id, Ray mouseRay)
    {
        // Right click start
        if (e.type == EventType.MouseDown)
        {
            GUIUtility.hotControl = id;
            e.Use();
        }

        // Right click drag
        else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id)
        {
            e.Use();
        }

        // Right click end
        else if (e.type == EventType.MouseUp && GUIUtility.hotControl == id)
        {
            GUIUtility.hotControl = 0;
            e.Use();
        }
    }
}