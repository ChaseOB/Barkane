using UnityEditor;
using UnityEngine;
using BarkaneEditor;

[ExecuteAlways]
public class LevelEditorManager : MonoBehaviour
{
    private const int PAPER_LAYER = 6;
    private const int PaperMask = 1 << PAPER_LAYER;

    [SerializeField] private PaperSquares squares;
    public PaperSquares Squares => squares;
    [SerializeField] private PaperJoints joints;

    [SerializeField] private Object squarePrefab;
    [SerializeField] private Object jointPrefab;

    [SerializeField] private GameObject plane;

    [SerializeField] private int axisPos = -1;
    [SerializeField] private Orientation orientation;

    private BoxCollider meshCollider;

    public static LevelEditorManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        if (jointPrefab == null)
        {
            Debug.LogWarning("Joint prefab is null");
        }
        meshCollider = plane.GetComponent<BoxCollider>();
    }

    private void Start()
    {
        orientation = OrientationExtension.GetOrientation(plane.transform.eulerAngles);
        if (orientation == Orientation.XZ)
        {
            plane.transform.eulerAngles = OrientationExtension.XZ;
        }
        plane.transform.position = GetPosOnPlane(Vector3.zero);
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
   
    public PaperSquare GetSquareClicked(Ray mouseRay)
    {
        RaycastHit hitSquare;
        if (Physics.Raycast(mouseRay, out hitSquare, Mathf.Infinity, PaperMask))
        {
            //Debug.Log($"Found Square: {hitSquare.transform.gameObject.name}");
            PaperSquare squareClicked = hitSquare.transform.GetComponent<PaperSquare>();

            return squareClicked;
        }

        return null;
    }

    public bool AddSquare(Vector3Int relPos)
    {
        squares.RefreshSquares();   //L: We might not want to do this every time for performance, but for now it works.

        if (squares.GetSquareAt(relPos) != null)
        {
            return false;
        }

        Vector3 squareCenter = squares.GetAbsolutePosition(relPos);
        Quaternion rotation = Quaternion.Euler(OrientationExtension.GetEulerAngle(orientation));

        GameObject squareObj = InstantiationExtension.InstantiateKeepPrefab(squarePrefab);
        Debug.Log($"Instantiated Prefab: {squareObj}");
        squareObj.transform.parent = squares.gameObject.transform;
        squareObj.transform.position = squareCenter;
        squareObj.transform.rotation = rotation;
        squareObj.name = $"Square {squares.numSquares}";

        PaperSquare square = squareObj.GetComponent<PaperSquare>();
        square.orientation = orientation;
        squares.SetSquareAt(relPos, square);
        Debug.Log($"Added Square at {relPos}");

        square.adjacentJoints.Clear();
        AddJointsTo(relPos, square);

        VFXManager.Instance?.Refresh();
        return true;
    }

    private void AddJointsTo(Vector3Int squareRelPos, PaperSquare square)
    {
        Orientation orient = square.orientation;

        //Still need to take care of case where two squares meet with different orientations.
        Vector3Int[] tangents = OrientationExtension.GetTangentDirs(orient);
        Vector3Int normal = OrientationExtension.GetNormalDir(orient);

        foreach (Vector3Int tangent in tangents)
        {
            Vector3Int[] neighborPositions = new Vector3Int[3] {    squareRelPos + 2 * tangent, 
                                                                    squareRelPos + tangent + normal, 
                                                                    squareRelPos + tangent - normal
            };

            foreach (Vector3Int nPos in neighborPositions)
            {
                //Debug.Log($"Checking Position: {neighbor}");
                PaperSquare nSquare = squares.GetSquareAt(nPos);
                if (nSquare != null)
                {
                    //Debug.Log($"Neighbor detected at {nPos}");
                    AddJoint(squareRelPos, square, nSquare, tangent);
                }
            }
        }
    }

    private void AddJoint(Vector3Int currSquareRelPos, PaperSquare currSquare, PaperSquare neighborSquare, Vector3Int jointOffset)
    {
        PaperJoint joint = InstantiateJoint(currSquareRelPos, currSquare.orientation, jointOffset);
        joint.name = $"Joint {currSquare.name} {neighborSquare.name}";

        //Update the joint's adjacent squares and the adjacent squares' joints.
        joint.PaperSquares.Clear();        joint.PaperSquares.Add(currSquare);
        joint.PaperSquares.Add(neighborSquare);

        currSquare.adjacentJoints.Add(joint);
        neighborSquare.adjacentJoints.Add(joint);
    }

    private PaperJoint InstantiateJoint(Vector3Int squareRelPos, Orientation squareOrient, Vector3Int jointOffset)
    {
        Vector3 sqAbsPos = squares.GetAbsolutePosition(squareRelPos);

        //Determine the position of the joint
        Vector3 jointCenter = sqAbsPos + jointOffset;

        //Set the direction of the capsule
       // var capsule = jointPrefab.GetComponent<CapsuleCollider>();
        //capsule.direction = GetJointCapsuleDirection(squareOrient, jointOffset);

        Vector3 rot = GetJointDirection(squareOrient, jointOffset);
        GameObject jointObj = InstantiationExtension.InstantiateKeepPrefab(jointPrefab);
        jointObj.transform.position = jointCenter;
        jointObj.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
        jointObj.transform.parent = joints.transform;
        return jointObj.GetComponent<PaperJoint>();
    }
    
    private Vector3 GetJointDirection(Orientation squareOrientation, Vector3Int jointOffset)
    {
        //C: need to rotate entire joint, not just capsule

        //0 X -> 0, 0, 0 
        //1 Y -> 0 0 90
        //2 Z -> 0 90 0
        switch (squareOrientation)
        {
            case Orientation.XZ:
                if (jointOffset == Vector3.left || jointOffset == Vector3.right)
                {
                    return new Vector3(0, 90, 0);  //Z-axis
                }
                else
                {
                    return Vector3.zero;  //X-axis
                }
            case Orientation.XY:
                if (jointOffset == Vector3.left || jointOffset == Vector3.right)
                {
                    return new Vector3(0, 0, 90);  //Y-axis
                }
                else
                {
                    return Vector3.zero;  //X-axis
                }
            case Orientation.YZ:
            default:
                if (jointOffset == Vector3.up || jointOffset == Vector3.down)
                {
                    return new Vector3(0, 90, 0);  //Z-axis
                }
                else
                {
                    return new Vector3(0, 0, 90);  //Y-axis
                }
        }
    }

    private int GetJointCapsuleDirection(Orientation squareOrientation, Vector3Int jointOffset)
    {
        //L: Boiler Plate FTW
        switch (squareOrientation)
        {
            case Orientation.XZ:
                if (jointOffset == Vector3.left || jointOffset == Vector3.right)
                {
                    return 2;  //Z-axis
                }
                else
                {
                    return 0;  //X-axis
                }
            case Orientation.XY:
                if (jointOffset == Vector3.left || jointOffset == Vector3.right)
                {
                    return 1;  //Y-axis
                }
                else
                {
                    return 0;  //X-axis
                }
            case Orientation.YZ:
            default:
                if (jointOffset == Vector3.up || jointOffset == Vector3.down)
                {
                    return 2;  //Z-axis
                }
                else
                {
                    return 1;  //Y-axis
                }
        }
    }

    public bool RemoveSquare(Vector3Int relPos)
    {
        squares.RefreshSquares();   //L: We might not want to do this every time for performance, but for now it works.
        Debug.Log($"Removing Square at {relPos}");
        PaperSquare square = squares.GetSquareAt(relPos);
        //Debug.Log($"square is {square}");

        if (square == null || square == squares.GetCenter())
        {
            return false;
        }
        //Debug.Log("Square is a square");

        squares.RemoveReference(relPos);
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
