using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldablePaper : MonoBehaviour
{
    [SerializeField] private PaperSqaure[] paperSqaures;
    public PaperSqaure[] PaperSqaures => paperSqaures;
    [SerializeField] private PaperJoint[] paperJoints;
    private List<PaperSquareStack> paperSquareStacks = new List<PaperSquareStack>();
    public List<PaperSquareStack> PaperSquareStacks => paperSquareStacks;
    private Dictionary<PaperSqaure,  List<PaperJoint>> adjListSquareToJoint;
    private Dictionary<PaperJoint,  List<PaperSqaure>> adjListJointToSquare;

    public FoldAnimator foldAnimator;


    private  List<GameObject> playerSide = new List<GameObject>();

    private  List<GameObject> foldSide = new List<GameObject>();

    private HashSet<PaperSqaure> visitedSquares = new HashSet<PaperSqaure>();
    private HashSet<PaperJoint> visitedJoints = new HashSet<PaperJoint>();


    public PaperJoint foldJoint;


    private void Awake() 
    {
        paperSqaures = GetComponentsInChildren<PaperSqaure>();   
        paperJoints = GetComponentsInChildren<PaperJoint>(); 
        UpdateAdjList();
    }

    private void UpdateAdjList()
    {
        adjListSquareToJoint = new Dictionary<PaperSqaure,  List<PaperJoint>>();
        adjListJointToSquare = new Dictionary<PaperJoint,  List<PaperSqaure>>();
        foreach(PaperSqaure ps in paperSqaures)
        {
            List<PaperJoint> adj = new List<PaperJoint>();
            foreach(PaperJoint pj in paperJoints)
            {
                if(pj.PaperSqaures.Contains(ps))
                {
                    adj.Add(pj);
                }
            }
            adjListSquareToJoint[ps] = adj;
        }

        foreach(PaperJoint pj in paperJoints)
        {
            adjListJointToSquare[pj] = pj.PaperSqaures;
        }
    }

    //C: Uses a modified DFS to determine which objects should be folded
    public void FindFoldObjects()
    {
        visitedJoints.Clear();
        visitedSquares.Clear();
        playerSide.Clear();
        foldSide.Clear();

        PaperSqaure playerSquare = null;
        foreach(PaperSqaure ps in paperSqaures)
            if(ps.PlayerOccupied)
                playerSquare = ps;
        DFSHelperSquare(playerSquare, true);
    }

    private void DFSHelperSquare(PaperSqaure ps, bool isPlayerSide)
    {
        if(ps == null) return;
        visitedSquares.Add(ps);
        if(isPlayerSide)
            playerSide.Add(ps.gameObject);
        else
            foldSide.Add(ps.gameObject);
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
            playerSide.Add(pj.gameObject);
        else
            foldSide.Add(pj.gameObject);
        foreach(PaperSqaure adjSquare in adjListJointToSquare[pj])
        {
            if(!visitedSquares.Contains(adjSquare))
                DFSHelperSquare(adjSquare, isPlayerSide);
        }
    }

    public void TestFold(float degrees)
    {
        FindFoldObjects();
        if(foldJoint.canFold)
            foldAnimator.Fold(foldSide, foldJoint.transform.position, foldJoint.transform.rotation * Vector3.right, degrees);
    }


    
    //C: looks through the PSSes to see if this square is in a stack. If it is, remove it from the stack and update stack visuals.
    //If there is only one square left in that PSS, the PSS is destroyed and removed.
    public void TryRemoveSquare(PaperSqaure ps)
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

    public PaperSquareStack GetStackWith(PaperSqaure ps)
    {
        foreach(PaperSquareStack pss in paperSquareStacks)
            if (pss.Contains(ps))
                return pss;
        return null;
    }

    public List<GameObject> getFoldSide() {
        return foldSide;
    }
}
