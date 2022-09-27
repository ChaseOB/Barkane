using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SceneManager : MonoBehaviour
{
    [SerializeField] private bool editorOn = false;
    public bool EditorOn => editorOn;
    [SerializeField] private PaperSquares squares;
    public PaperSquares Squares => squares;
    [SerializeField] private Plane editPlane;

    // Start is called before the first frame update
    private void Start()
    {
        editPlane = new Plane(Vector3.up, Vector3.zero);
    }

    private Vector3? GetSelectPosition(Ray mouseRay)
    {
        float outDist = 0f;
        if (editPlane.Raycast(mouseRay, out outDist)) {
            return mouseRay.GetPoint(outDist);
        }
        // Ray does not intersect with plane
        return null;
    }

    public void SelectSquare(Ray mouseRay)
    {
        Vector3? selectPos = GetSelectPosition(mouseRay);
        if (selectPos == null)
        {
            return;
        }
        squares.SelectSquare((Vector3)selectPos);
    }

    public void RemoveSquare(Ray mouseRay)
    {
        Vector3? selectPos = GetSelectPosition(mouseRay);
        if (selectPos == null)
        {
            return;
        }
        squares.RemoveSquare((Vector3)selectPos);
    }

    private void OnMouseDown()
    {
        print("asdfasdf");
    }
}
