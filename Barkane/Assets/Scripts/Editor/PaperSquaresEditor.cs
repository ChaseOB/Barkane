using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PaperSquares))]
public class PaperSquaresEditor : Editor
{
    private PaperSquares _target;

    private void OnEnable()
    {
        _target = (PaperSquares) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //L: Don't use these buttons

        //if (GUILayout.Button("CLEAR SQAURE REFERENCES"))
        //{
        //    _target.RemoveAllReferences();
        //}

        //if (GUILayout.Button("SYNC SQAURE REFERENCES TO SCENE"))
        //{
        //    Debug.LogError("Syncing of References Not Implemented Yet (we need to do this at some point)");
        //}
    }
}
