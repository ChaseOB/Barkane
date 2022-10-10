using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[ExecuteAlways]
public class LevelEditorManager : MonoBehaviour
{
    private const int PAPER_LAYER = 6;
    private const int PaperMask = 1 << PAPER_LAYER;

    [SerializeField] private PaperSquares squares;
    public PaperSquares Squares => squares;
    [SerializeField] private PaperJoints joints;

    [SerializeField] private PaperSqaure SqaureCopy;
    [SerializeField] private GameObject jointPrefab;

    [SerializeField] private GameObject plane;

    [SerializeField] private int axisPos = -1;
    [SerializeField] private Orientation orientation;

    private BoxCollider meshCollider;

    private void Awake()
    {
        if (jointPrefab == null)
        {
            Debug.LogWarning("Joint prefab is null");
        }
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
        SqaureCopy = squares.GetCenter();
        HidePlane();
    }

    private void OnValidate()
    {
        axisPos = Mathf.Clamp(Mathf.RoundToInt(axisPos + 0.5f) - 1, -PaperSquares.SIZE / 2, PaperSquares.SIZE / 2);
        plane.transform.eulerAngles = OrientationExtension.GetEulerAngle(orientation);
        plane.transform.position = GetPosOnPlane(Vector3.zero);
    }

    public bool GetSquarePosition(Ray mouseRay, out Vector3 hitPoint, out Orientation orientation)
    {
        RaycastHit hitSquare;
        if (!GetSquarePosition(mouseRay, out hitSquare))
        {
            hitPoint = Vector3.zero;
            orientation = Orientation.XZ;
            return false;
        }
        hitPoint = hitSquare.collider.transform.position;
        orientation = OrientationExtension.GetOrientation(hitSquare.collider.transform.eulerAngles);
        return true;
    }

    private bool GetSquarePosition(Ray mouseRay, out RaycastHit hitSquare)
    {
        if (Physics.Raycast(mouseRay, out hitSquare, Mathf.Infinity, PaperMask))
        {
            return true;
        }
        return false;
    }

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

        RaycastHit hitSquare;
        if (GetSquarePosition(mouseRay, out hitSquare) && hitPlane.distance > hitSquare.distance)
        {
            // A PaperSquare blocks the Raycast to the plane.
            hitPoint = hitSquare.collider.transform.position;
            return false;
        }

        hitPoint = GetPosOnPlane(hitPlane.point);
        return true;
    }
   
    public PaperSqaure GetSquareClicked(Ray mouseRay)
    {
        RaycastHit hitSquare;
        if (Physics.Raycast(mouseRay, out hitSquare, Mathf.Infinity, PaperMask))
        {
            //Debug.Log($"Found Square: {hitSquare.transform.gameObject.name}");
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
        square = Instantiate(SqaureCopy, squareCenter, rotation, squares.squareList.transform);
        square.orient = orientation;
        squares.SetSquareAt(relPos, square);

        AddJointsTo(relPos, square);

        return true;
    }

    private void AddJointsTo(Vector3Int relPos, PaperSqaure square)
    {
        ForEachAdjacentSqaure(relPos, square.orient, (nRelPos, nSquare) =>
        {
            AddJoint(square, nSquare, relPos, nRelPos);
        });
    }

    private void ForEachAdjacentSqaure(Vector3Int relPos, Orientation orient, System.Action<Vector3Int, PaperSqaure> callback)
    {
        //Still need to take care of case where two sqaures meet with different orientations.
        Vector3Int[] dirs = DirectionsToCheck(orient);

        foreach (Vector3Int dir in dirs)
        {
            Vector3Int neighborRelPos = relPos + 2 * dir;
            Debug.Log($"Checking Position: {neighborRelPos}");
            PaperSqaure neighborSq = squares.GetSquareAt(neighborRelPos);
            if (neighborSq != null)
            {
                Debug.Log($"Neighbor detected at {neighborRelPos}");
                callback(neighborRelPos, neighborSq);
            }
        }
    }

    private Vector3Int[] DirectionsToCheck(Orientation orient)
    {
        Vector3Int[] dirs = new Vector3Int[4];

        //4 sides based on orientation
        switch (orient)
        {
            case Orientation.XY:
                dirs = new Vector3Int[4] { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };
                break;
            case Orientation.YZ:
                dirs = new Vector3Int[4] { Vector3Int.up, Vector3Int.down, Vector3Int.forward, Vector3Int.back };
                break;
            case Orientation.XZ:
                dirs = new Vector3Int[4] { Vector3Int.left, Vector3Int.forward, Vector3Int.right, Vector3Int.back };
                break;
        }

        return dirs;
    }

    private void AddJoint(PaperSqaure sq1, PaperSqaure sq2, Vector3Int relPos1, Vector3Int relPos2)
    {
        Vector3 absPos1 = squares.GetAbsolutePosition(relPos1);
        Vector3 absPos2 = squares.GetAbsolutePosition(relPos2);
        Vector3 dPos = (absPos2 - absPos1);
        Vector3 dPosNormal = dPos.normalized;

        //Determine the position of the joint
        Vector3 jointCenter = absPos1 + dPos / 2;

        //Determine the orientation of the joint
        Vector3 sqNormal = OrientationExtension.GetRotationAxis(sq1.orient);
        var capsule = jointPrefab.GetComponent<CapsuleCollider>();

        //L: Boiler Plate FTW
        switch (sq1.orient)
        {
            case Orientation.XZ:
                if (dPosNormal == Vector3.left || dPosNormal == Vector3.right)
                {
                    capsule.direction = 2;  //Z-axis
                } else
                {
                    capsule.direction = 0;  //X-axis
                }
                break;
            case Orientation.XY:
                if (dPosNormal == Vector3.left || dPosNormal == Vector3.right)
                {
                    capsule.direction = 1;  //Y-axis
                }
                else
                {
                    capsule.direction = 0;  //X-axis
                }
                break;
            case Orientation.YZ:
                if (dPosNormal == Vector3.up || dPosNormal == Vector3.down)
                {
                    capsule.direction = 2;  //Z-axis
                }
                else
                {
                    capsule.direction = 1;  //Y-axis
                }
                break;
        }

        PaperJoint joint = Instantiate(jointPrefab, jointCenter, Quaternion.identity, joints.transform).GetComponent<PaperJoint>();

        //Update the joint's adjacent squares and the adjacent squares' joints.
        joint.PaperSqaures.Clear();
        joint.PaperSqaures.Add(sq1);
        joint.PaperSqaures.Add(sq2);
        sq1.adjacentJoints.Add(joint);
        sq2.adjacentJoints.Add(joint);
    }

    public bool RemoveSquare(Vector3Int relPos)
    {
        PaperSqaure square = squares.GetSquareAt(relPos);

        if (square == null || square == squares.GetCenter())
        {
            return false;
        }

        squares.RemoveReference(relPos);
        if (square == SqaureCopy)
        {
            SqaureCopy = squares.GetCenter();
        }

        square.RemoveAdjacentJoints();
        DestroyImmediate(square.gameObject);
        return true;
    }

    public Vector3Int GetNearestSquarePos(Vector3 absPos)
    {
        return GetNearestSquarePos(absPos, orientation);
    }

    public Vector3Int GetNearestSquarePos(Vector3 absPos, Orientation orientation)
    {
        return squares.GetNearestSquarePos(absPos, orientation);
    }

    public void ShowPlane()
    {
        plane.gameObject.SetActive(true);
    }

    public void HidePlane()
    {
        plane.gameObject.SetActive(false);
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
