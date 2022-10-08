using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[ExecuteAlways]
public class LevelEditorManager : MonoBehaviour
{
    private const int PAPER_LAYER = 6;

    [SerializeField] private PaperSquares squares;
    public PaperSquares Squares => squares;
    [SerializeField] private PaperSqaure copyFrom;

    private BoxCollider meshCollider;
    [SerializeField] private GameObject plane;

    [SerializeField] private int axisPos = -1;
    [SerializeField] private Orientation orientation;

    ///Summary <summary>
    /// Output the position on the edit plane that was clicked
    /// </summary>
    /// <param name="mouseRay">Ray going inward from part of the screen that was clicked</param>
    /// <param name="hitPoint">The point on the plane that was clicked</param>
    /// <returns>Returns true if and only if the area clicked is on the edit plane</returns>    
    public bool GetPlanePosition(Ray mouseRay, out Vector3 hitPoint)
    {
        //Check that the user clicked on the edit plane
        RaycastHit hitPlane;
        if (!meshCollider.Raycast(mouseRay, out hitPlane, Mathf.Infinity))
        {
            // The mouseRay does not collide with the edit plane.
            hitPoint = Vector3.zero;
            return false;
        }


        //Check that the clicked part does not hit a square that is in front of the edit plane.
        int paperMask = 1 << PAPER_LAYER;     // Bit mask for layer 6 (paper layer)
        RaycastHit hitSquare;
        if (Physics.Raycast(mouseRay, out hitSquare, Mathf.Infinity, paperMask) && hitPlane.distance > hitSquare.distance)
        {
            hitPoint = GetPosOnPlane(hitSquare.point);
            return false;
        }

        hitPoint = GetPosOnPlane(hitPlane.point);
        return true;
    }

    ///Summary <summary>
    /// Output the position on a square
    /// </summary>
    /// <param name="mouseRay">Ray going inward from part of the screen that was clicked</param>
    /// <param name="hitPoint">The point on the plane that was clicked</param>
    /// <returns>Returns the square clicked on</returns>    
    public PaperSqaure GetSquareClicked(Ray mouseRay)
    {
        Debug.Log("Getting Square Clicked");
        int paperMask = 1 << PAPER_LAYER;     // Bit mask for layer 6 (paper layer)
        RaycastHit hitSquare;
        if (Physics.Raycast(mouseRay, out hitSquare, Mathf.Infinity, paperMask))
        {
            Debug.Log($"Found Square: {hitSquare.transform.gameObject.name}");
            PaperSqaure squareClicked = hitSquare.transform.GetComponent<PaperSqaure>();

            return squareClicked;
        }

        return null;
    }

    public bool AddSquare(Vector3Int relPos)
    {
        PaperSqaure square = squares.GetSquareAt(relPos);

        if (square != null)
        {
            return false;
        }

        Vector3 squareCenter = squares.GetAbsolutePosition(relPos);
        Quaternion rotation = Quaternion.Euler(OrientationExtension.GetEulerAngle(orientation));
        square = Instantiate(copyFrom, squareCenter, rotation, squares.squareList.transform);
        squares.SetSquareAt(relPos, square);
        return true;
    }

    public bool RemoveSquare(Vector3Int relPos)
    {
        PaperSqaure square = squares.GetSquareAt(relPos);

        if (square == null || square == squares.GetCenter())
        {
            return false;
        }

        squares.RemoveReference(relPos);
        if (square == copyFrom)
        {
            copyFrom = squares.GetCenter();
        }
        DestroyImmediate(square.gameObject);
        return true;
    }

    public Vector3Int GetNearestSquarePos(Vector3 absPos)
    {
        return squares.GetNearestSquarePos(absPos, orientation);
    }

    private void Start()
    {
        meshCollider = plane.GetComponent<BoxCollider>();
        orientation = OrientationExtension.GetOrientation(plane.transform.eulerAngles);
        if (orientation == Orientation.XZ)
        {
            plane.transform.eulerAngles = OrientationExtension.XZ;
        }
        plane.transform.position = GetPosOnPlane(Vector3.zero);
        copyFrom = squares.GetCenter();
    }

    private void OnValidate()
    {
        axisPos = Mathf.Clamp(Mathf.RoundToInt(axisPos + 0.5f) - 1, -PaperSquares.SIZE / 2, PaperSquares.SIZE / 2);
        plane.transform.eulerAngles = OrientationExtension.GetEulerAngle(orientation);
        plane.transform.position = GetPosOnPlane(Vector3.zero);
    }

    private void OnDisable()
    {
        plane.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        plane.gameObject.SetActive(true);
    }

    private Vector3 GetPosOnPlane(Vector3 approxPos)
    {
        Vector3 newPos = new Vector3(approxPos.x, approxPos.y, approxPos.z);
        switch (orientation)
        {
            case Orientation.YZ:
                newPos.x = axisPos;
                break;
            case Orientation.XZ:
                newPos.y = axisPos;
                break;
            case Orientation.XY:
                newPos.z = axisPos;
                break;
        }
        return newPos;
    }
}
