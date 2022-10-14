using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperJoint : MonoBehaviour
{
    [SerializeField] private List<PaperSquare> paperSquares;
    public  List<PaperSquare> PaperSquares { get => paperSquares;}

    private bool isSelected = false; //true when this is the current selected fold
    public bool showLine = false; //true when this joint or any adjacent joins are selected. Used for showing visuals and partitioning graph

    //private PaperJoint currentJoint;
   // FoldablePaper foldablePaper;
    //private bool isFirstCall = true;
  //  List<PaperSquare> willBeFoldedPaperSquares = new List<PaperSquare>();

    [SerializeField] private List<PaperJoint> adjList = new List<PaperJoint>();

    [SerializeField] private CapsuleCollider capsuleCollider;
    public bool canFold = true; //CO: Set to false to lock the current joint in position, as if the squares were glued together

    [SerializeField] private JointRenderer jointRenderer;
    public JointRenderer JointRenderer => jointRenderer;

    private void Start() {
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
        showLine = value;
        
        jointRenderer?.ShowLine(value);

        Debug.Log("Showing line for " + this.gameObject.name);

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

#if UNITY_EDITOR
    public void Remove()
    {
        if (gameObject != null)
        {
            //Debug.Log($"Destroying Joint {gameObject.name}");
            foreach (PaperSquare square in PaperSquares)
            {
                square.adjacentJoints.Remove(this);
            }
            DestroyImmediate(this.gameObject);
        }
    }       
#endif
}
