using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;

[RequireComponent(typeof(VFXThemeAdapter))]
public class FoldablePaper : MonoBehaviour
{
    [SerializeField] private PaperSquare[] paperSquares;
    public PaperSquare[] PaperSquares => paperSquares;
    [SerializeField] private PaperJoint[] paperJoints;
    public PaperJoint[] PaperJoints => paperJoints;
    private Dictionary<PaperSquare,  List<PaperJoint>> adjListSquareToJoint;
    private Dictionary<PaperJoint,  List<PaperSquare>> adjListJointToSquare;
    public FoldAnimator foldAnimator;
    private  FoldObjects playerSide;
    private  FoldObjects foldObjects;
    private HashSet<PaperSquare> visitedSquares = new HashSet<PaperSquare>();
    private HashSet<PaperJoint> visitedJoints = new HashSet<PaperJoint>();
    public PaperJoint foldJoint;
    public GameObject SquareCollider;
    public Transform playerSpawn;
    public bool isComplete = false; //C: set to true when goal is reached
    public Vector3 centerPos;
    Dictionary<Vector3Int, List<PaperSquare>> squareLocs = new Dictionary<Vector3Int, List<PaperSquare>>();
    public PaperSquare playerSquare;

    private void Awake() 
    {
        CalculateCenter();
        paperSquares = GetComponentsInChildren<PaperSquare>();   
        paperJoints = GetComponentsInChildren<PaperJoint>(); 
        foldAnimator = FindObjectOfType<FoldAnimator>();
        UpdateAdjList();
        IntializeSquarePosList();
    }


    private void UpdateAdjList()
    {
        adjListSquareToJoint = new Dictionary<PaperSquare,  List<PaperJoint>>();
        adjListJointToSquare = new Dictionary<PaperJoint,  List<PaperSquare>>();
        foreach(PaperSquare ps in paperSquares)
        {
            List<PaperJoint> adj = new List<PaperJoint>();
            foreach(PaperJoint pj in paperJoints)
            {
                if(pj.PaperSquares.Contains(ps))
                {
                    adj.Add(pj);
                }
            }
            adjListSquareToJoint[ps] = adj;
        }

        foreach(PaperJoint pj in paperJoints)
        {
            adjListJointToSquare[pj] = pj.PaperSquares;
        }
    }

    private void Update() {
        CalculateCenter();
    }
  
    private void CalculateCenter() {
        List<Vector3> vectors = new List<Vector3>();
         foreach(PaperSquare ps in paperSquares){
            vectors.Add(ps.transform.localPosition);
        }
        CoordUtils.CalculateCenter(vectors);
    }

    private void IntializeSquarePosList()
    {
        foreach(PaperSquare ps in paperSquares){
            List<PaperSquare> list = new List<PaperSquare>();
                list.Add(ps);
                squareLocs.Add(Vector3Int.RoundToInt(ps.transform.position), list);
        }
    }


    //C: Uses a modified DFS to determine which objects should be folded
    // Returns fold side objects first, player side objects second (we need player side objects for
    // testing collision)
    public FoldObjects[] FindFoldObjects()
    {
        visitedJoints.Clear();
        visitedSquares.Clear();

        playerSide = new FoldObjects(paperSquares[0].transform.parent, paperJoints[0].transform.parent);
        foldObjects = new FoldObjects(paperSquares[0].transform.parent, paperJoints[0].transform.parent);

        PaperSquare playerSquare = null;
        foreach(PaperSquare ps in paperSquares)
            if(ps.PlayerOccupied)
                playerSquare = ps;
        DFSHelperSquare(playerSquare, true);

        playerSide.OnFoldHighlight(false);
        foldObjects.OnFoldHighlight(true);
        FoldObjects[] returnArr = new FoldObjects[2];
        returnArr[0] = playerSide;
        returnArr[1] = foldObjects;
        return returnArr;
    }

    private void DFSHelperSquare(PaperSquare ps, bool isPlayerSide)
    {
        if(ps == null) return;
        visitedSquares.Add(ps);
        if(isPlayerSide)
            playerSide.foldSquares.Add(ps.gameObject);
        else
            foldObjects.foldSquares.Add(ps.gameObject);
        foreach(PaperJoint adjJoint in adjListSquareToJoint[ps])
        {
            if(!visitedJoints.Contains(adjJoint))
                DFSHelperJoint(adjJoint, isPlayerSide);
        }
    }

    private void DFSHelperJoint(PaperJoint pj,  bool isPlayerSide)
    {
        if(pj == null) return;
        visitedJoints.Add(pj);
        isPlayerSide = pj.showLine ? !isPlayerSide : isPlayerSide; //C: if we cross the fold line, then this value changes. We're essentially slicing the graph into 2 parts
        if(pj.showLine)
            foldObjects.foldLineJoints.Add(pj.gameObject);
        if(isPlayerSide)
            playerSide.foldJoints.Add(pj.gameObject);
        else
            foldObjects.foldJoints.Add(pj.gameObject);
        foreach(PaperSquare adjSquare in adjListJointToSquare[pj])
        {
            if(!visitedSquares.Contains(adjSquare))
                DFSHelperSquare(adjSquare, isPlayerSide);
        }
    }

    public void TryFold(float degrees)
    {
        FindFoldObjects();
        if(!isComplete && foldJoint != null && foldJoint.canFold) {
            List<PaperJoint> foldJoints = new List<PaperJoint>();
            foreach(PaperJoint pj in PaperJoints)
                if(pj.showLine)
                    foldJoints.Add(pj);
            FoldData fd = new FoldData(foldJoint, foldJoints, foldObjects, playerSide, foldJoint.transform.position, foldJoint.transform.rotation * Vector3.right, degrees);
            foldAnimator.TryFold(fd);
        }
    }


    public List<List<PaperSquare>> FindOverlappingSquares()
    {
        List<List<PaperSquare>> overlapList = new List<List<PaperSquare>>();

        Dictionary<Vector3, List<PaperSquare>> dict = new Dictionary<Vector3, List<PaperSquare>>();

        foreach(PaperSquare ps in paperSquares) {
            bool didAdd = false;
            foreach(Vector3 key in dict.Keys){
                if (Vector3.Magnitude(key - ps.transform.position) < 0.0001f) {
                    dict[key].Add(ps);
                    didAdd = true;
                }
            }
            if(!didAdd)
            {
                List<PaperSquare> list = new List<PaperSquare>();
                list.Add(ps);
                dict.Add(ps.transform.position, list);
//                Debug.Log(ps.transform.position);
            }
        }

        foreach (List<PaperSquare> list in dict.Values){
                overlapList.Add(list);
        }
        return overlapList;
    }
}


public class FoldObjects {
    public List<GameObject> foldSquares; //C: every square being folded
    public List<GameObject> foldJoints; //C: the non-line joints being folded
    public List<GameObject> foldLineJoints; //C: joints along the fold line
    public Transform squareParent;
    public Transform jointParent;

    public FoldObjects() {
        foldSquares = new List<GameObject>();
        foldJoints = new List<GameObject>();
        foldLineJoints = new List<GameObject>();
    }

    public FoldObjects(Transform sp, Transform jp) {
        foldSquares = new List<GameObject>();
        foldJoints = new List<GameObject>();
        foldLineJoints = new List<GameObject>();
        squareParent = sp;
        jointParent = jp;
    }

    public void EnableJointMeshes()
    {
        foreach(GameObject go in foldLineJoints)
        {
            PaperJoint pj = go.GetComponent<PaperJoint>();
            JointRenderer jr = pj?.JointRenderer;
            jr?.EnableMeshAction();
            jr?.ShowLine(false, true);
        }
    }

    public void DisableJointMeshes()
    {
        foreach(GameObject go in foldLineJoints)
        {
            PaperJoint pj = go.GetComponent<PaperJoint>();
            JointRenderer jr = pj?.JointRenderer;
            jr?.DisableMeshAction();
            jr?.ShowLine(true);
        }
    }

    public void OnFoldHighlight(bool select)
    {
        foreach (GameObject go in foldSquares)
            go.GetComponent<PaperSquare>().OnFoldHighlight(select);
    }

    //foldStart is true when starting a fold and false when ending a fold
    public void OnFold(bool foldStart)
    {
        
    }

    public Vector3 CalculateCenter()
    {
        List<Vector3> vectors = new List<Vector3>();
        foreach(GameObject ps in foldSquares){
            vectors.Add(ps.transform.position);
        }
        return CoordUtils.CalculateCenter(vectors);
    }

}