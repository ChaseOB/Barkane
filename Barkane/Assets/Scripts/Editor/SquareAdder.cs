using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Add & Delete Squares", typeof(LevelEditorManager))]
public class SquareAdder : EditorTool
{
    LevelEditorManager manager;
    // TODO: Use PrefabUtility.InstatitatePrefab to instantiate new PaperSquares
    [SerializeField] PaperSqaure squarePrefab;

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
            if (!manager.GetPlanePosition(mouseRay, out hitPoint))
            {
                return;
            }

            Vector3Int relPos = manager.GetNearestSquarePos(hitPoint);

            // Left click start
            if (e.type == EventType.MouseDown)
            {
                GUIUtility.hotControl = id;
                if (manager.AddSquare(relPos))
                {
                    EditorUtility.SetDirty(manager.gameObject);
                }
                e.Use();
            }

            // Left click drag
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id)
            {
                if (manager.AddSquare(relPos))
                {
                    EditorUtility.SetDirty(manager.gameObject);
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

            if (!manager.GetSquarePosition(mouseRay, out hitPoint, out orientation))
            {
                return;
            }

            Vector3Int relPos = manager.GetNearestSquarePos(hitPoint, orientation);

            // Right click start
            if (e.type == EventType.MouseDown)
            {
                GUIUtility.hotControl = id;
                if (manager.RemoveSquare(relPos))
                {
                    EditorUtility.SetDirty(manager.gameObject);
                }
                e.Use();
            }

            // Right click drag
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id)
            {
                if (manager.RemoveSquare(relPos))
                {
                    EditorUtility.SetDirty(manager.gameObject);
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

    private void OnEnable()
    {
        manager = target as LevelEditorManager;
    }

    public override void OnActivated()
    {
        if (!Application.isPlaying)
        {
            manager.ShowPlane();
        }
    }

    public override void OnWillBeDeactivated()
    {
        if (!Application.isPlaying)
        {
            manager.HidePlane();
        }
    }
}

