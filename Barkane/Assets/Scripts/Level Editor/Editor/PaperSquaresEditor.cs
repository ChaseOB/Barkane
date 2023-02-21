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
}
