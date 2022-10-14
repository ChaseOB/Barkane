using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldablePaper : MonoBehaviour
{
    [SerializeField] private PaperSquare[] paperSquares;
    public PaperSquare[] PaperSquares => paperSquares;
    [SerializeField] private PaperJoint[] paperJoints;
    private List<PaperSquareStack> paperSquareStacks = new List<PaperSquareStack>();
    public List<PaperSquareStack> PaperSquareStacks => paperSquareStacks;
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

    private void Awake() 
    {
        paperSquares = GetComponentsInChildren<PaperSquare>();   
        paperJoints = GetComponentsInChildren<PaperJoint>(); 
        foldAnimator = FindObjectOfType<FoldAnimator>();
        UpdateAdjList();
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

    //C: Physics calculations should always be in FixedUpdate()
    private void FixedUpdate() 
    {
       // CheckFoldCollisions();
    }


    //C: Given the currently selected fold objects, uses collision to check the directions that they can rotate in
    private void CheckFoldCollisions()
    {
        int numChecks = 5;
        if(foldJoint == null) return;
        GameObject parent = new GameObject();
        foreach(GameObject go in foldObjects.foldSquares)
        {
            GameObject newSquare = Instantiate(SquareCollider, go.transform.position, go.transform.rotation);
            newSquare.transform.parent = parent.transform;
        }
        
        //Ideally we should check every point along the rotation axis, but this is impracticle. 
        for(int i = 1; i <= numChecks; i++) {

        }

        //First check obstacles


        //Then check for intesecting squares



        Destroy(parent);
    }




    //C: Uses a modified DFS to determine which objects should be folded
    public void FindFoldObjects()
    {
        visitedJoints.Clear();
        visitedSquares.Clear();

        playerSide = new FoldObjects();
        foldObjects = new FoldObjects(paperSquares[0].transform.parent, paperJoints[0].transform.parent);

        PaperSquare playerSquare = null;
        foreach(PaperSquare ps in paperSquares)
            if(ps.PlayerOccupied)
                playerSquare = ps;
        DFSHelperSquare(playerSquare, true);

        playerSide.OnFoldHighlight(false);
        foldObjects.OnFoldHighlight(true);
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

    public void TestFold(float degrees)
    {
        FindFoldObjects();
        if(!isComplete && foldJoint != null && foldJoint.canFold)
            foldAnimator.Fold(foldJoint, foldObjects, foldJoint.transform.position, foldJoint.transform.rotation * Vector3.right, degrees);
    }


    public List<List<PaperSquare>> FindOverlappingSquares()
    {
        List<List<PaperSquare>> overlapList = new List<List<PaperSquare>>();

        Dictionary<Vector3, List<PaperSquare>> dict = new Dictionary<Vector3, List<PaperSquare>>();

        foreach(PaperSquare ps in paperSquares) {
            bool didAdd = false;
            foreach(Vector3 key in dict.Keys){
                if (Vector3.Magnitude(key - ps.transform.position) < 0.001f) {
                    dict[key].Add(ps);
                    didAdd = true;
                }
            }
            if(!didAdd)
            {
                List<PaperSquare> list = new List<PaperSquare>();
                list.Add(ps);
                dict.Add(ps.transform.position, list);
            }
        }

        foreach (List<PaperSquare> list in dict.Values){
            if(list.Count > 1)
                overlapList.Add(list);
        }
        return overlapList;
    }


    
    //C: looks through the PSSes to see if this square is in a stack. If it is, remove it from the stack and update stack visuals.
    //If there is only one square left in that PSS, the PSS is destroyed and removed.
    public void TryRemoveSquare(PaperSquare ps)
    {
        foreach(PaperSquareStack pss in paperSquareStacks)
        {
            pss.TryRemoveSquare(ps);
            if(pss.destroy)
            {
                paperSquareStacks.Remove(pss);
                Destroy(pss);
            }
        }
    }

    public PaperSquareStack GetStackWith(PaperSquare ps)
    {
        foreach(PaperSquareStack pss in paperSquareStacks)
            if (pss.Contains(ps))
                return pss;
        return null;
    }
}

public class FoldObjects {
    public List<GameObject> foldSquares;
    public List<GameObject> foldJoints;
    public Transform squareParent;
    public Transform jointParent;

    public FoldObjects() {
        foldSquares = new List<GameObject>();
        foldJoints = new List<GameObject>();
    }

    public FoldObjects(Transform sp, Transform jp) {
        foldSquares = new List<GameObject>();
        foldJoints = new List<GameObject>();
        squareParent = sp;
        jointParent = jp;
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

    public void UpdateSquarePriority(int priority)
    {
        foreach(GameObject go in foldSquares)
        {
            go.GetComponent<PaperSquare>().UpdateSquarePriority(priority);
        }
    }
}