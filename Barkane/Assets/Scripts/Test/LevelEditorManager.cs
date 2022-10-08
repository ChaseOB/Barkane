using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[ExecuteAlways]
public class LevelEditorManager : MonoBehaviour
{
    [SerializeField] private PaperSquares squares;
    public PaperSquares Squares => squares;
    [SerializeField] private PaperSqaure copyFrom;

    private BoxCollider meshCollider;
    [SerializeField] private GameObject plane;

    [SerializeField] private int axisPos = -1;
    [SerializeField] private Orientation orientation;

    public bool GetPlanePosition(Ray mouseRay, out Vector3 hitPoint)
    {
        RaycastHit hitPlane;
        if (!meshCollider.Raycast(mouseRay, out hitPlane, Mathf.Infinity))
        {
            // The mouseRay does not collide with the edit plane.
            hitPoint = Vector3.zero;
            return false;
        }

        // Bit mask for layer 6 (paper layer)
        int paperMask = 1 << 6;
        RaycastHit hitSquare;

        if (Physics.Raycast(mouseRay, out hitSquare, Mathf.Infinity, paperMask) && hitPlane.distance > hitSquare.distance)
        {
            hitPoint = GetPosOnPlane(hitSquare.point);
            return false;
        }

        hitPoint = GetPosOnPlane(hitPlane.point);
        return true;
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
        this.enabled = false;
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

    private void OnDestroy()
    {
        this.enabled = true;
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
