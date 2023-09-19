using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;
using System.Linq;

[ExecuteAlways]
public class PaperJoint : MonoBehaviour
{
    [SerializeField] private List<PaperSquare> paperSquares;
    public  List<PaperSquare> PaperSquares { get => paperSquares;}

    public bool isSelected = false; //true when this is the current selected fold
    public bool showLine = false; //true when this joint or any adjacent joins are selected. Used for showing visuals and partitioning graph


    [SerializeField] private List<PaperJoint> adjFoldJointsList = new List<PaperJoint>();
    private List<PaperJoint> allAdjJoints = new List<PaperJoint>();


    [SerializeField] private CapsuleCollider capsuleCollider;
    public bool canFold = true; //CO: Set to false to lock the current joint in position, as if the squares were glued together

    [SerializeField] private JointRenderer jointRenderer;
    public JointRenderer JointRenderer => jointRenderer;

    private void Start() {
        if(capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void OnDestroy()
    {
        Remove();
    }

    public void Select()
    {
        isSelected = true;
        ShowLine(true);
    }

    public void Deselect()
    {
        isSelected = false;
        ShowLine(false);
    }

    public void ToggleCollider(bool value)
    {
        capsuleCollider.enabled = value;
    }

    public void OnHoverEnter()
    {
        if(canFold)
            ShowLine(true);
    }

    public void OnHoverExit()
    {
        if(!isSelected)
            ShowLine(false);
    }

    private void ShowLine(bool value)
    {
        showLine = value;
        jointRenderer?.ShowLine(value);
        foreach(PaperJoint pj in adjFoldJointsList)
            if(pj.showLine != value)
                pj.ShowLine(value);
    }

    private void OnTriggerEnter(Collider other) {
        
        if(other.gameObject.layer == 7)
        {
            PaperJoint joint = other.GetComponent<PaperJoint>();
            allAdjJoints.Add(joint);
            foreach(PaperJoint j in allAdjJoints)
                CheckIfJointAdjacent(j);
        }
    }

    void Update()
    {
        UpdateCenter();
    }

    private void UpdateCenter()
    {
        GetComponent<CapsuleCollider>().center = jointRenderer.offset;
    }

    /*private void FindAdjJoints()
    {
        Collider[] hits = Physics.OverlapCapsule(capsuleCollider.center - transform.forward * capsuleCollider.height/2,
                                capsuleCollider.center - transform.forward * capsuleCollider.height/2,
                                capsuleCollider.radius, 7);
        foreach (Collider c in hits)
        {
            PaperJoint joint = c.GetComponent<PaperJoint>();
            if(c.gameObject.activeInHierarchy)
                allAdjJoints.Add(joint);
        }
        foreach(PaperJoint j in allAdjJoints)
            CheckIfJointAdjacent(j);
    }*/

    private void CheckIfJointAdjacent(PaperJoint joint)
    {
        Vector3 diff = this.transform.position - joint.transform.position;
        int difX = Mathf.Abs(diff.x) > 0.1 ? 1 : 0;
        int difY = Mathf.Abs(diff.y) > 0.1 ? 1 : 0;
        int difZ = Mathf.Abs(diff.z) > 0.1 ? 1 : 0;
        //C: Either adjacent on same axis or adjacent on a diff axis but not connected to a square on this joint
        if(difX + difY + difZ == 1 || (difX + difY + difZ == 2 && DiffNormals(joint))) 
        {
            List<PaperJoint> checkList = new List<PaperJoint>();
            checkList.AddRange(allAdjJoints);
            checkList.Remove(joint);
            if(checkList.Count < 2)
                return;
            
            List<PaperSquare> squares1 = new List<PaperSquare>();
            squares1.AddRange(paperSquares);
            squares1.AddRange(joint.PaperSquares);

            foreach(PaperJoint j1 in checkList)
            {
                foreach(PaperJoint j2 in checkList)
                {
                    if(j1 != j2)
                    {
                        List<PaperSquare> squares2 = new List<PaperSquare>();
                        squares2.AddRange(j1.PaperSquares);
                        squares2.AddRange(j2.paperSquares);

                        bool same = squares1.All(squares2.Contains) && squares1.Count == squares2.Count;
                        if(same && ! adjFoldJointsList.Contains(joint))
                        {
                            adjFoldJointsList.Add(joint);
                            return;
                        }
                    }
                }
            }
        }

    }

    private bool DiffNormals(PaperJoint joint)
    {
        if(!SameOrFlipped(paperSquares[0].transform.up,paperSquares[1].transform.up) ||
        !SameOrFlipped(joint.paperSquares[0].transform.up,joint.paperSquares[1].transform.up))
            return false;
        return !SameOrFlipped(paperSquares[0].transform.up, joint.paperSquares[0].transform.up);
    }

    private bool SameOrFlipped(Vector3 v1, Vector3 v2)
    {
        return v1 == v2 || v1 * -1 == v2;
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == 7)
            allAdjJoints.Remove(other.GetComponent<PaperJoint>());
    }

    private void Remove()
    {
        if (gameObject != null)
        {
            //Debug.Log($"Destroying Joint {gameObject.name}");
            foreach (PaperSquare square in PaperSquares)
            {
                square.adjacentJoints.Remove(this);
            }
        }
    }       

    public void OnFold()
    {
       // GlowStickLogic[] sticks = GetComponentsInChildren<GlowStickLogic>();
       // foreach (GlowStickLogic g in sticks)
         //   g.OnFold(showLine);
    }
}
