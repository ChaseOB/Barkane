using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Add & Delete Squares", typeof(LevelEditorManager))]
public class SquareAdder : EditorTool
{
    private LevelEditorManager levelEditor;
    // TODO: Use PrefabUtility.InstatitatePrefab to instantiate new PaperSquares
    [SerializeField] PaperSquare squarePrefab;

    private void OnEnable()
    {
        levelEditor = target as LevelEditorManager;
    }

    // TODO: Use Handles to deal with UnityGUI events
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
        {
            // Current window is not a SceneView.
            return;
        }

        Event e = Event.current;

        if (!e.isMouse)
        {
            // Not a mouse event.
            return;
        }

        int id = GUIUtility.GetControlID(FocusType.Passive);
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 hitPoint;

        if (e.button == 0)
        {
            // Left click handlers
            if (!levelEditor.GetPlanePosition(mouseRay, out hitPoint))
            {
                return;
            }

            Vector3Int relPos = levelEditor.GetNearestSquarePos(hitPoint);

            // Left click start
            if (e.type == EventType.MouseDown)
            {
                GUIUtility.hotControl = id;
                if (levelEditor.AddSquare(relPos))
                {
                    EditorUtility.SetDirty(levelEditor.gameObject);
                }
                e.Use();
            }

            // Left click drag
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id)
            {
                if (levelEditor.AddSquare(relPos))
                {
                    EditorUtility.SetDirty(levelEditor.gameObject);
                }
                e.Use();
            }

            // Left click end
            else if (e.type == EventType.MouseUp && GUIUtility.hotControl == id)
            {
                GUIUtility.hotControl = 0;
                e.Use();
            }
        }
        else if (e.button == 1)
        {
            // Right click handlers
            Orientation orientation;

            if (!levelEditor.GetSquarePosition(mouseRay, out hitPoint, out orientation))
            {
                return;
            }

            Vector3Int relPos = levelEditor.GetNearestSquarePos(hitPoint, orientation);

            // Right click start
            if (e.type == EventType.MouseDown)
            {
                GUIUtility.hotControl = id;
                if (levelEditor.RemoveSquare(relPos))
                {
                    EditorUtility.SetDirty(levelEditor.gameObject);
                }
                e.Use();
            }

            // Right click drag
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id)
            {
                if (levelEditor.RemoveSquare(relPos))
                {
                    EditorUtility.SetDirty(levelEditor.gameObject);
                }
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

    public override void OnActivated()
    {
        if (!Application.isPlaying)
        {
            levelEditor.ShowPlane();
        }
    }

    public override void OnWillBeDeactivated()
    {
        if (!Application.isPlaying)
        {
            levelEditor.HidePlane();
        }
    }
}

