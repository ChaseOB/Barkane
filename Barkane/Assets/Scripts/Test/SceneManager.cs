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

    private BoxCollider collider;
    [SerializeField] private int axisPos;

    // [SerializeField] private Vector3 orientation;
    public readonly Vector3 XY = new Vector3(0, 0, 0);
    public readonly Vector3 XZ = new Vector3(90, 0, 0);
    public readonly Vector3 YZ = new Vector3(0, 90, 0);

    public Vector3? GetPlanePosition(Ray mouseRay)
    {
        // Bit mask for layer 6 (paper layer)
        int paperMask = 1 << 6;
        RaycastHit hit;

        if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, paperMask))
        {
            Vector3 hitPoint = hit.point;
            if (collider.bounds.Contains(hitPoint))
            {
                return hitPoint;
            }
        }
        return null;
    }

    private void Start()
    {
        collider = GetComponent<BoxCollider>();
    }
}
