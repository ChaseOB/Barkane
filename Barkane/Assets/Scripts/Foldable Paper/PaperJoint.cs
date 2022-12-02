using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneJoint;

[ExecuteAlways]
public class PaperJoint : MonoBehaviour
{
    [SerializeField] private List<PaperSquare> paperSquares;
    public  List<PaperSquare> PaperSquares { get => paperSquares;}

    public bool isSelected = false; //true when this is the current selected fold
    public bool showLine = false; //true when this joint or any adjacent joins are selected. Used for showing visuals and partitioning graph


    [SerializeField] private List<PaperJoint> adjList = new List<PaperJoint>();

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

    private void ShowLine(bool value)
    {
        showLine = value;
        
        jointRenderer?.ShowLine(value);


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
            if(difX + difY + difZ == 1 || (difX + difY + difZ == 2 && DiffNormals(joint))) //C: Either adjacent on same axis or adjacent on a diff axis but not connected to a square on this joint
                adjList.Add(other.GetComponent<PaperJoint>());
        }
    }

    private bool DiffNormals(PaperJoint joint)
    {
        if(!SameOrFlipped(paperSquares[0].transform.up,paperSquares[1].transform.up) ||
        !SameOrFlipped(joint.paperSquares[0].transform.up,joint.paperSquares[1].transform.up))
            return false;
        return !SameOrFlipped(paperSquares[0].transform.up, paperSquares[0].transform.up);
    }

    private bool SameOrFlipped(Vector3 v1, Vector3 v2)
    {
        return v1 == v2 || v1 * -1 == v2;
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == 7)
            adjList.Remove(other.GetComponent<PaperJoint>());
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
        GlowStickLogic[] sticks = GetComponentsInChildren<GlowStickLogic>();
        foreach (GlowStickLogic g in sticks)
            g.OnFold();
    }
}
