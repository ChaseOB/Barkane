using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class JointRenderer : MonoBehaviour
{
    [SerializeField] SquareRenderSettings squareRenderSettings; // for referencing the margin property

    [SerializeField] MeshFilter filterA1, filterA2, filterB1, filterB2;
    [SerializeField] MeshRenderer rendererA1, rendererA2, rendererB1, rendererB2;

    /// <summary>
    /// Can be called manually in inspector or automatically by other scene editor utilities.
    /// This should NOT be preferred over calling either UpdateReferences or UpdateGeometry    
    /// unless absolutely necessary
    /// </summary>
    /// <exception cref="UnityException"></exception>
    public void Refresh()
    {
        var parent = transform.parent.GetComponent<PaperJoint>();
        if (parent.PaperSqaures.Count < 2)
        {
            throw new UnityException($"Cannot refresh joints without enough adjacent squares: {parent.PaperSqaures.Count}");
        }

        var aTop = parent.PaperSqaures[0].TopHalf;
        var aBottom = parent.PaperSqaures[0].BottomHalf;
        var bTop = parent.PaperSqaures[1].TopHalf;
        var bBottom = parent.PaperSqaures[1].BottomHalf;

        // if (aTop)

        // UpdateReferences(a, b);
        // UpdateGeometry(a, b);
    }

    /// <summary>
    /// Update Joint appearance when the type/state of either side changes
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public void UpdateReferences(PaperSqaure a, PaperSqaure b)
    {
    }

    /// <summary>
    /// Update Joint appearance when the physical location/orientation of either side changes
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public void UpdateGeometry(PaperSqaure a, PaperSqaure b)
    {
        if (CoordUtils.Coplanar(a, b))
        {
            var normal = a.transform.up;

            var center = CoordUtils.AsV(CoordUtils.Average(a, b));

            var toA = (a.transform.position - center).normalized;
            var toB = (b.transform.position - center).normalized;

            // create 4 rectangles (8 tris), 2 for each side
            var verts = new Vector3[]
            {

            };
            var tris = new List<int>();
            for (int i = 0; i < verts.Length; i += 3)
            {

            }
        }
        else
        {

        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(JointRenderer))]
public class JointRendererEditor: Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Refresh"))
        {
            (target as JointRenderer).Refresh();
        }
    }
}

#endif