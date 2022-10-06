using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[ExecuteInEditMode]
public class SceneManager : MonoBehaviour
{
    [SerializeField] private PaperSquares squares;
    public PaperSquares Squares => squares;
    [SerializeField] private PaperSqaure copyFrom;

    private BoxCollider meshCollider;

    [SerializeField] private int axisPos = -1;
    [SerializeField] Orientation orientation;

    public bool GetPlanePosition(Ray mouseRay, out Vector3 hitPoint)
    {
        // Bit mask for layer 6 (paper layer)
        int paperMask = 1 << 6;
        RaycastHit hit;

        if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, paperMask))
        {
            if (meshCollider.bounds.Contains(hit.point))
            {
                hitPoint = GetPosOnPlane(hit.point);
                return true;
            }
        }
        hitPoint = Vector3.zero;
        return false;
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
        meshCollider = GetComponent<BoxCollider>();
        orientation = OrientationExtension.GetOrientation(transform.eulerAngles);
        if (orientation == Orientation.XZ)
        {
            transform.eulerAngles = OrientationExtension.XZ;
        }
        transform.position = GetPosOnPlane(Vector3.zero);
        copyFrom = squares.GetCenter();
    }

    private void OnValidate()
    {
        transform.eulerAngles = OrientationExtension.GetEulerAngle(orientation);
        transform.position = GetPosOnPlane(Vector3.zero);
    }

    // Helper methods

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
