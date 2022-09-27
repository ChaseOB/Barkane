using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
[CustomEditor(typeof(SceneManager))]
public class SceneEditor : Editor
{
    // Reads input from scene when SceneManager is selected
    void OnSceneGUI()
    {
        SceneManager manager = target as SceneManager;
        Event e = Event.current;

        switch (e.type)
        {
            /*
            case EventType.MouseMove:
                break;
            case EventType.Repaint:
                break;
            case EventType.Layout:
                break;
            case EventType.MouseLeaveWindow:
                break;
            case EventType.MouseEnterWindow:
                break;
            case EventType.Used:
                break;
            case EventType.ScrollWheel:
                break;
            */
            case EventType.MouseDown:
                Vector3 mousePos = e.mousePosition;
                Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePos);
                if (e.button == 0)
                {
                    // Left mouse button
                    manager.SelectSquare(mouseRay);
                } else if (e.button == 1)
                {
                    // Right mouse button
                    manager.RemoveSquare(mouseRay);
                }
                break;
            default:
                // Debug.Log(e);
                break;
        }
    }

    private void OnEnable()
    {

    }

}
