using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperJoint : MonoBehaviour
{
    [SerializeField] private PaperSqaure paperSqaure1;
    [SerializeField] private PaperSqaure paperSqaure2;
    private bool isSelected = false; //true when this is the current selected fold
    public bool showLine = false; //true when this joint or any adjacent joins are selected
    public LineRenderer lineRenderer;

    [SerializeField] private List<PaperJoint> adjList = new List<PaperJoint>();

   // private bool JointEnabled = true; //CO: Set to false to "cut" the paper along the given joint
    private bool canFold = true; //CO: Set to false to lock the current joint in position, as if the squares were glued together

   

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
            adjList.Add(other.GetComponent<PaperJoint>());
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == 7)
            adjList.Remove(other.GetComponent<PaperJoint>());
    }
}
