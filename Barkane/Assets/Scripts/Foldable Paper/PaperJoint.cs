using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperJoint : MonoBehaviour
{
    [SerializeField] private List<PaperSqaure> paperSqaures;
    public  List<PaperSqaure> PaperSqaures { get => paperSqaures;}

    private bool isSelected = false; //true when this is the current selected fold
    public bool showLine = false; //true when this joint or any adjacent joins are selected. Used for showing visuals and partitioning graph
    public LineRenderer lineRenderer;
    private PaperJoint currentJoint;
    FoldablePaper foldablePaper;
    List<GameObject> willBeFoldedAll;
    private bool isFirstCall = true;
    List<PaperSqaure> willBeFoldedPaperSquares = new List<PaperSqaure>();

    [SerializeField] private List<PaperJoint> adjList = new List<PaperJoint>();

    [SerializeField] private CapsuleCollider capsuleCollider;
   // private bool JointEnabled = true; //CO: Set to false to "cut" the paper along the given joint
    public bool canFold = true; //CO: Set to false to lock the current joint in position, as if the squares were glued together

    void Update(){
        if (isSelected) {
            if (isFirstCall) {
                isFirstCall = !isFirstCall;
                foldablePaper = FindObjectOfType<FoldablePaper>();
                foldablePaper.FindFoldObjects();
                willBeFoldedAll = foldablePaper.getFoldSide();
                GetWillBeFoldedPaperSquares();
                EmitEdgeParticles();
            }
        } else {
            if (!isFirstCall) {
                isFirstCall = !isFirstCall;
                UnEmitParticles();
            }
        }
    } private void Start() {
        if(capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider>();
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

    private void ShowLine(bool value)
    {
        lineRenderer.enabled = value;
        showLine = value;
        foreach(PaperJoint pj in adjList)
            if(pj.showLine != value)
                pj.ShowLine(value);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.layer == 7)
        {
            PaperJoint joint = other.GetComponent<PaperJoint>();
            Vector3 diff = this.transform.position - joint.transform.position;
            int difX = Mathf.Abs(diff.x) > 0.1 ? 1 : 0;
            int difY = Mathf.Abs(diff.y) > 0.1 ? 1 : 0;
            int difZ = Mathf.Abs(diff.z) > 0.1 ? 1 : 0;
            if(difX + difY + difZ == 1) //C: 3-way XOR to check that the folds are along the same axis
                adjList.Add(other.GetComponent<PaperJoint>());
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == 7)
            adjList.Remove(other.GetComponent<PaperJoint>());
    }

    private void GetWillBeFoldedPaperSquares() {
        for (int i = 0; i < willBeFoldedAll.Count; i++) {
            if (willBeFoldedAll[i].GetComponent<PaperSqaure>() != null) {
                willBeFoldedPaperSquares.Add(willBeFoldedAll[i].GetComponent<PaperSqaure>());
            }
        }
    }

    private void EmitEdgeParticles() {
        for (int i = 0; i < willBeFoldedPaperSquares.Count; i++) {
            willBeFoldedPaperSquares[i].GetComponent<EdgeParticles>().Emit();
        }
    }

    private void UnEmitParticles() {
        for (int i = 0; i < willBeFoldedPaperSquares.Count; i++) {
            willBeFoldedPaperSquares[i].GetComponent<EdgeParticles>().Unemit();
        }
    }
}
