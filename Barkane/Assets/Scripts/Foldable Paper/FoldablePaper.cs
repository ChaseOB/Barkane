using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEngine;

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
    // private  FoldObjects playerSide;
    // private  FoldObjects foldObjects;
    private HashSet<PaperSquare> visitedSquares = new HashSet<PaperSquare>();
    private HashSet<PaperJoint> visitedJoints = new HashSet<PaperJoint>();
    public PaperJoint foldJoint;
    public GameObject SquareCollider;
    public Transform playerSpawn;
    public bool isComplete = false; //C: set to true when goal is reached
    public Vector3 centerPos;
    Dictionary<Vector3Int, List<PaperSquare>> squareLocs = new Dictionary<Vector3Int, List<PaperSquare>>();
    public PaperSquare playerSquare;

    // public OcclusionMap OcclusionMap => m_OcclusionMap;
    // OcclusionMap m_OcclusionMap = new OcclusionMap();

    // private List<FoldableObject> playerSideFoldableObjects = new();
    // private List<FoldableObject> foldSideFoldableObjects = new();
    private FoldObjects foldObjects = new();

    private Dictionary<PaperSquare, SquareData> squareDict = new();
    private Dictionary<PaperJoint, JointData> jointDict = new();

    private void Awake() 
    {
        CalculateCenter();
        paperSquares = GetComponentsInChildren<PaperSquare>();   
        paperJoints = GetComponentsInChildren<PaperJoint>(); 
        foldAnimator = FindObjectOfType<FoldAnimator>();
        //Build Paper State
        UpdateAdjList();
        IntializeSquarePosList();
    }

    private void Start()
    {
        BuildPaperState();
    }

    // private void OnEnable() {
    //     TileSelector.OnFoldSelect += OnFoldSelect; 
    // }

    // private void OnDisable() {
    //     TileSelector.OnFoldSelect -= OnFoldSelect; 
    // }

    // private void OnFoldSelect(object sender, bool value)
    // {
    //     FindFoldObjects();
    //     foldObjects.OnFoldHighlight(value);
    // }


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
            var rounded = Vector3Int.RoundToInt(ps.transform.position);
            List<PaperSquare> list = new List<PaperSquare>();
                list.Add(ps);
                squareLocs.Add(rounded, list);
        }
    }


    // //C: Uses a modified DFS to determine which objects should be folded
    // // Returns fold side objects first, player side objects second 
    // public (FoldObjects, FoldObjects) FindFoldObjects()
    // {
    //     visitedJoints.Clear();
    //     visitedSquares.Clear();

    //     playerSide = new FoldObjects(paperSquares[0].transform.parent, paperJoints[0].transform.parent);
    //     foldObjects = new FoldObjects(paperSquares[0].transform.parent, paperJoints[0].transform.parent);

    //     PaperSquare playerSquare = null;
    //     foreach(PaperSquare ps in paperSquares)
    //         if(ps.PlayerOccupied)
    //             playerSquare = ps;
    //     DFSHelperSquare(playerSquare, true);
    //     playerSide.MakeSideSet();
    //     foldObjects.MakeSideSet();
    //     return (playerSide, foldObjects);
    // }

    // private void DFSHelperSquare(PaperSquare ps, bool isPlayerSide)
    // {
    //     if(ps == null || visitedSquares.Contains(ps)) return;
    //     visitedSquares.Add(ps);
    //     if(isPlayerSide && ! playerSide.squareScripts.Contains(ps)) {
    //         playerSide.foldSquares.Add(ps.gameObject);
    //         playerSide.squareScripts.Add(ps);
    //     }
    //     else if (!foldObjects.squareScripts.Contains(ps)){
    //         foldObjects.foldSquares.Add(ps.gameObject);
    //         foldObjects.squareScripts.Add(ps);
    //     }
    //     foreach(PaperJoint adjJoint in adjListSquareToJoint[ps])
    //     {
    //         if(!visitedJoints.Contains(adjJoint))
    //             DFSHelperJoint(adjJoint, isPlayerSide);
    //     }
    // }

    // private void DFSHelperJoint(PaperJoint pj,  bool isPlayerSide)
    // {
    //     if(pj == null || visitedJoints.Contains(pj)) return;
    //     visitedJoints.Add(pj);
    //     isPlayerSide = pj.showLine ? !isPlayerSide : isPlayerSide; //C: if we cross the fold line, then this value changes. We're essentially slicing the graph into 2 parts
    //     if(pj.showLine)
    //         foldObjects.foldLineJoints.Add(pj.gameObject);
    //     if(isPlayerSide)
    //         playerSide.foldJoints.Add(pj.gameObject);
    //     else
    //         foldObjects.foldJoints.Add(pj.gameObject);
    //     foreach(PaperSquare adjSquare in adjListJointToSquare[pj])
    //     {
    //         if(!visitedSquares.Contains(adjSquare))
    //             DFSHelperSquare(adjSquare, isPlayerSide);
    //     }
    // }


    // public FoldData BuildFoldData(float degrees)
    // {
    //     FindFoldObjects();
    //     if(!isComplete && foldJoint != null && foldJoint.canFold) {
    //         List<PaperJoint> foldJoints = new List<PaperJoint>();
    //         foreach(PaperJoint pj in PaperJoints)
    //             if(pj.showLine)
    //                 foldJoints.Add(pj);
    //         FoldData fd = new FoldData(foldJoints, foldObjects, playerSide, foldJoint.transform.position, foldJoint.transform.rotation * Vector3.right, degrees);
    //         return fd;
    //     }
    //     return null;
    // }

    public void BuildPaperState()
    {
        PaperState state = new();
        foreach (PaperSquare paperSquare in paperSquares)
        {
            SquareStack stack = new(paperSquare);
            state.squareStacks.Add(stack);
            SquareData s = stack.squarelist.First();
            squareDict.Add(paperSquare, s); 
        }
        foreach (PaperJoint paperJoint in paperJoints)
        {
            JointStack stack = new(paperJoint);
            state.jointStacks.Add(stack);
            JointData s = stack.jointList.First();
            jointDict.Add(paperJoint, s); 
        }
        PaperStateManager.Instance.squareDict = squareDict;
        PaperStateManager.Instance.jointDict = jointDict;

        PaperStateManager.Instance.SetPaperState(state);
    }

    public FoldObjects FindFoldObjects()
    {
        visitedJoints.Clear();
        visitedSquares.Clear();
        foldObjects = new();

        PaperSquare playerSquare = null;
        foreach(PaperSquare ps in paperSquares)
            if(ps.PlayerOccupied)
                playerSquare = ps;
        DFSHelperSquare(playerSquare, true);
        return foldObjects;
    }

    private void DFSHelperSquare(PaperSquare ps, bool isPlayerSide)
    {
        if(ps == null || visitedSquares.Contains(ps)) return;
        visitedSquares.Add(ps);
        //if first time, build new stack with only this square. else, get existing square data.
        SquareData s;
        if(squareDict.ContainsKey(ps))
        {
            s = squareDict[ps];
        }
        else
        {
            Debug.Log("square key not found (bad)");
        //     SquareStack stack = new(ps);
        //   //  PaperStateManager.PaperState.squareStacks.Add(stack);
        //     s = stack.squarelist.First();
        //     squareDict.Add(ps, s);
        s = null;
        }
        if(isPlayerSide) { 
            foldObjects.playerSideObjects.Add(s);
        }
        else{
            foldObjects.foldSideObjects.Add(s);
        }
        foreach(PaperJoint adjJoint in adjListSquareToJoint[ps])
        {
            if(!visitedJoints.Contains(adjJoint))
                DFSHelperJoint(adjJoint, isPlayerSide);
        }
    }

    private void DFSHelperJoint(PaperJoint pj,  bool isPlayerSide)
    {
        if(pj == null || visitedJoints.Contains(pj)) return;
        visitedJoints.Add(pj);
        isPlayerSide = pj.showLine ? !isPlayerSide : isPlayerSide; //C: if we cross the fold line, then this value changes. We're essentially slicing the graph into 2 parts
        //if first time, build new stack with only this square. else, get existing square data.
        JointData j;
        if(jointDict.ContainsKey(pj))
        {
            j = jointDict[pj];
        }
        else
        {
                        Debug.Log("joint key not found (bad)");
            j = null;
        //     JointStack stack = new(pj);
        //    //PaperStateManager.PaperState.jointStacks.Add(stack);
        //     j = stack.jointList.First();
        //     jointDict.Add(pj, j);
        }
        if(pj.showLine)
            foldObjects.axisJoints.Add(pj);
        if(isPlayerSide)
           foldObjects.playerSideObjects.Add(j);
        else
            foldObjects.foldSideObjects.Add(j);
        foreach(PaperSquare adjSquare in adjListJointToSquare[pj])
        {
            if(!visitedSquares.Contains(adjSquare))
                DFSHelperSquare(adjSquare, isPlayerSide);
        }
    }

    public FoldData BuildFoldData(bool invert)
    {
        FindFoldObjects();
        // List<FoldableObject> playerStacks = new();
        // List<FoldableObject> foldStacks = new();
        if(!isComplete && foldJoint != null && foldJoint.canFold) {
            // List<PaperJoint> foldJoints = new List<PaperJoint>();
            // foreach(PaperJoint pj in PaperJoints)
            //     if(pj.showLine)
            //         foldJoints.Add(pj);

            // foreach(PaperSquare ps in playerSide.squareScripts)
            // {
            //     SquareStack s = new(Vector3Int.RoundToInt(ps.transform.position));
            //     s.squares.AddFirst(ps);
            //     playerStacks.Add(s);
            // }
            // foreach(PaperSquare ps in foldObjects.squareScripts)
            // {
            //     SquareStack s = new(Vector3Int.RoundToInt(ps.transform.position));
            //     s.squares.AddFirst(ps);
            //     foldStacks.Add(s);
            // }
            //FoldData fd = new FoldData(foldJoints, foldObjects.foldSideObjects, foldObjects, Vector3Int.RoundToInt(foldJoint.transform.position), Vector3Int.RoundToInt(foldJoint.transform.rotation * Vector3.right), (int) degrees);
            // foreach(FoldableObject fo in foldObjects.foldSideObjects)
            // {
            //     print(fo);
            // }
            Vector3Int position = Vector3Int.RoundToInt(foldJoint.transform.position);
            FoldData fd = new(
                foldObjects,
                position,
                GetFoldAxis(position, invert)
            );
            return fd;
        }
        return null;
    }

    private Vector3Int GetFoldAxis(Vector3Int postion, bool invert)
    {
        Vector3Int ret = Vector3Int.forward;
        if(postion.x % 2 == 0) ret = Vector3Int.right;
        if(postion.y % 2 == 0) ret = Vector3Int.up;
        ret = invert? -1 * ret : ret;
        return ret;
    }


    // public List<List<PaperSquare>> FindOverlappingSquares()
    // {
    //     List<List<PaperSquare>> overlapList = new List<List<PaperSquare>>();

    //     Dictionary<Vector3, List<PaperSquare>> dict = new Dictionary<Vector3, List<PaperSquare>>();

    //     foreach(PaperSquare ps in paperSquares) {
    //         bool didAdd = false;
    //         foreach(Vector3 key in dict.Keys){
    //             if (Vector3.Magnitude(key - ps.transform.position) < 0.0001f) {
    //                 dict[key].Add(ps);
    //                 didAdd = true;
    //             }
    //         }
    //         if(!didAdd)
    //         {
    //             List<PaperSquare> list = new List<PaperSquare>();
    //             list.Add(ps);
    //             dict.Add(ps.transform.position, list);
    //         }
    //     }

    //     foreach (List<PaperSquare> list in dict.Values)
    //         overlapList.Add(list);
    //     return overlapList;
    // } 

    // public void PopulateOcclusionMap()
    // {
    //     foreach(var ps in paperSquares)
    //     {
    //         var rounded = Vector3Int.RoundToInt(ps.transform.position);

    //         if (m_OcclusionMap.ContainsKey(rounded))
    //         {
    //             m_OcclusionMap[rounded].Enqueue(ps);
    //             m_OcclusionMap[rounded].UseAsGlobal();
    //         }
    //         else
    //         {
    //             var q = OcclusionQueue.MakeOcclusionQueue(rounded, OcclusionQueue.IdentityEncoder);

    //             if (q != null)
    //             {
    //                 q.Enqueue(ps);
    //                 m_OcclusionMap[rounded] = q;
    //             }
    //             else
    //             {
    //                 throw new UnityException("Occlusion queue could not be created");
    //             }
    //             m_OcclusionMap[rounded].UseAsGlobal();
    //         }

    //         // Debug.DrawRay(m_OcclusionMap[rounded].Offset, m_OcclusionMap[rounded].upwards, Color.yellow, 3);
    //     }

    //     // Debug.Log(OcclusionMap);
    // }
}
