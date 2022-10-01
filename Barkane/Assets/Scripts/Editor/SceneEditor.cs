using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

[ExecuteAlways]
[CustomEditor(typeof(SceneManager))]
public class SceneEditor : Editor
{
    private SceneManager manager;
    private PaperSqaure moveSquare;
    private DragType curDrag;

    private enum DragType
    {
        MoveDrag,
        CreateDrag,
        DeleteDrag
    }

    // Reads input from scene when SceneManager is selected
    private void OnSceneGUI()
    {
        if (!manager.EditorOn)
        {
            return;
        }

        Event e = Event.current;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (!e.isMouse)
        {
            return;
        }

        Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3? selectPos_ = manager.GetPlanePosition(mouseRay);
        if (selectPos_ == null)
        {
            return;
        }
        Vector3 selectPos = (Vector3)selectPos_;

        if (e.button == 0)
        {
            // Left click handlers

            // Left click start
            if (e.type == EventType.MouseDown)
            {
                curDrag = manager.Squares.SelectSquare(selectPos, out moveSquare) ? DragType.CreateDrag : DragType.MoveDrag;

            }

            // Left click drag
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == controlId)
            {

            }

            // Left click end
            else if (e.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
            {

            }
        } else if (e.button == 1)
        {
            // Right click handlers

            // Right click start
            if (e.type == EventType.MouseDown)
            {

            }

            // Right click drag
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == controlId)
            {

            }

            // Right click end
            else if (e.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
            {

            }
        }

        /*
        if (e.type == EventType.MouseDown)
        {


            if (e.button == 0)
            {
                Debug.Log("add");
                // Left mouse button; selects PaperSquare
                Undo.IncrementCurrentGroup();
                Undo.RegisterCompleteObjectUndo(manager.Squares, "Updating PaperSquares");
                PaperSqaure newSquare = manager.Squares.SelectSquare(selectPos);
                Undo.FlushUndoRecordObjects();

                if (newSquare != null)
                {
                    Undo.RegisterCreatedObjectUndo(newSquare.gameObject, "Created new PaperSquare object");
                }
                Undo.SetCurrentGroupName("Selected PaperSquare in scene");
            }
            else if (e.button == 1)
            {
                Debug.Log("remove");
                // Right mouse button; removes PaperSquare
                Undo.IncrementCurrentGroup();
                Undo.RegisterCompleteObjectUndo(manager.Squares, "Updating PaperSquares");
                PaperSqaure delSquare = manager.Squares.RemoveSquare(selectPos);

                if (delSquare != null)
                {
                    Undo.DestroyObjectImmediate(delSquare.gameObject);
                }
                Undo.SetCurrentGroupName("Tried deleting PaperSquare");
            }
            e.Use();
        }
        */
    }

    private void OnEnable()
    {
        manager = target as SceneManager;
        Undo.undoRedoPerformed += MyUndoCallback;
    }

    /*
    public override void OnInspectorGUI()
    {

    }
    */

    void MyUndoCallback()
    {
        Debug.Log("Redo performed");
    }
}
