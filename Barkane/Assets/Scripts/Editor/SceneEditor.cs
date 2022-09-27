using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
[CustomEditor(typeof(SceneManager))]
public class SceneEditor : Editor
{
    private SceneManager manager;

    // Reads input from scene when SceneManager is selected
    private void OnSceneGUI()
    {
        if (!manager.EditorOn)
        {
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            Vector3 mousePos = e.mousePosition;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePos);
            if (e.button == 0)
            {
                // Left mouse button
                manager.SelectSquare(mouseRay);
            }
            else if (e.button == 1)
            {
                // Right mouse button
                manager.RemoveSquare(mouseRay);
            }
            e.Use();
        }
    }

    private void OnEnable()
    {
        manager = target as SceneManager;
    }
}
