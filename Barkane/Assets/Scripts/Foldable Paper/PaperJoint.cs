using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperJoint : MonoBehaviour
{
    [SerializeField] private List<PaperSqaure> paperSqaures;
    public  List<PaperSqaure> PaperSqaures { get => paperSqaures;}
    private bool isSelected = false; //true when this is the current selected fold
    public bool showLine = false; //true when this joint or any adjacent joins are selected
    public LineRenderer lineRenderer;
    private PaperJoint currentJoint;
    FoldablePaper foldablePaper;
    List<GameObject> willBeFoldedAll;
    private bool isFirstCall = true;
    List<PaperSqaure> willBeFoldedPaperSquares = new List<PaperSqaure>();

    [SerializeField] private List<PaperJoint> adjList = new List<PaperJoint>();

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
    }

    public void Select()
    {
        isSelected = true;
        ShowLine(true);
        // isFirstCall = true;
    }

    public void Deselect()
    {
        isSelected = false;
        ShowLine(false);
        // isFirstCall = true;
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
            if(Mathf.Abs(diff.x) < 0.1 || Mathf.Abs(diff.z) < 0.1)
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
