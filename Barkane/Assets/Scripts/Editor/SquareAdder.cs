using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Add & Delete Squares", typeof(SceneManager))]
public class SquareAdder : EditorTool
{
    SceneManager manager;

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
        {
            // Current window is not a SceneView.
            return;
        }

        Event e = Event.current;

        int id = GUIUtility.GetControlID(FocusType.Passive);
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 hitPoint;

        if (!manager.GetPlanePosition(mouseRay, out hitPoint))
        {
            return;
        }

        Vector3Int relPos = manager.GetNearestSquarePos(hitPoint);

        //Handles.DrawWireDisc(hitPoint, Vector3.up, 0.5f);

        if (!e.isMouse)
        {
            // Not a mouse event.
            return;
        }

        if (e.button == 0)
        {
            // Left click handlers

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
        manager = target as SceneManager;
    }
}

