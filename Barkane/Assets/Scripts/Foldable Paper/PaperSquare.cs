using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
[RequireComponent(typeof(EdgeParticles))]
public class PaperSquare : MonoBehaviour, IComparable<PaperSquare>
{

    [SerializeField] private bool playerOccupied = false; //true if the player is on this square
    public bool PlayerOccupied { get => playerOccupied;}

    
    //Visuals
    public float paperLength = 2f;
    public float paperThickness = 0.001f;
    [SerializeField] private GameObject topHalf;
    public GameObject TopHalf => topHalf;
    [SerializeField] private GameObject bottomHalf;
    public GameObject BottomHalf => bottomHalf;
    [SerializeField] private EdgeParticles edgeParticles;

    public SquareSide topSide;
    public SquareSide bottomSide;

    public List<int> priority = new List<int>();

    public GameObject topPlayerCol;
    public GameObject botPlayerCol;

    public PaperSquare topStack;
    public PaperSquare bottomStack;

    public Vector3 storedPos;

////#if UNITY_EDITOR
    public Orientation orientation;
    public List<PaperJoint> adjacentJoints;
//#endif

    private void Awake() {
        priority.Add(0);
    }
    private void Start() 
    {
        edgeParticles = GetComponent<EdgeParticles>();
        storedPos = transform.position;
        topSide = TopHalf.GetComponent<SquareSide>();
        bottomSide = BottomHalf.GetComponent<SquareSide>();
    }

    
    public int CompareTo(PaperSquare other)
    {
        int index = 0;
        while(this.priority == other.priority)
            index++;
        return this.priority[index] - other.priority[index];
    }

    public void ToggleTop(bool val)
    {
        topHalf.SetActive(val);
        topPlayerCol.SetActive(val);
    }

    public void ToggleBottom(bool val)
    {
        bottomHalf.SetActive(val);
        botPlayerCol.SetActive(val);
    }

    public void SetPlayerOccupied(bool value)
    {
        playerOccupied = value;
    }

    public void UpdateSquarePriority(int priority)
    {
        this.priority.Insert(0, priority);
        //topSide.UpdateSquarePriority(priority);
        //bottomSide.UpdateSquarePriority(priority);
    }

    //select is true when this region is selected and false when deselected
    public void OnFoldHighlight(bool select)
    {
        if(select)
            edgeParticles?.Emit();
        else
            edgeParticles?.Unemit();
    }

    //foldStart is true when starting a fold and false when ending a fold
    public void OnFold(bool foldStart)
    {
    }

    public void StorePosition(Vector3 pos)
    {
        storedPos = pos;
    }

    //C: if the a stacked PS is not in the list, remove it and re-enable mesh
    public void CheckAndRemoveRefs(List<PaperSquare> list)
    {   
        if(topStack != null && !list.Contains(topStack)) {
            ToggleTop(true);
            topStack = null;
        }
        if(bottomStack != null && !list.Contains(bottomStack)) {
            ToggleBottom(true);
            bottomStack = null;
        }
    }

    public void UpdateHitboxes()
    {
        topPlayerCol.SetActive(topHalf.activeSelf);
        botPlayerCol.SetActive(bottomHalf.activeSelf);
    }

    public bool IsInMiddle()
    {
        return topStack != null && bottomStack != null;
    }

    public List<GameObject> GetOpenSides()
    {
        List<GameObject> list = new List<GameObject>();
        if(topStack == null)
            list.Add(topHalf);
        if(bottomStack == null)
            list.Add(bottomHalf);
        return list;
    }

#if UNITY_EDITOR
    public void AddJoint(PaperJoint joint)
    {
        adjacentJoints.Add(joint);
    }

    public void RemoveAdjacentJoints()
    {
        while(adjacentJoints.Count > 0)
        {
            adjacentJoints[0]?.Remove();
        }
    }
#endif

    private void OnValidate()
    {
        Vector3 offset = this.transform.rotation * new Vector3(0, paperThickness / 2, 0);
        if (topHalf) topHalf.transform.position = this.transform.position + offset;
        if (bottomHalf) bottomHalf.transform.position = this.transform.position - offset;
    }

    //C: returns 0 if closer to top or 1 if closer to bottom. Returns -1 if equidistant
    public int FindCloserSide(Vector3 position)
    {
        Vector3 topPos = this.transform.position + topHalf.transform.localPosition * 100;
        Vector3 botPos = this.transform.position + bottomHalf.transform.localPosition * 100;
        float dist1 = Vector3.Magnitude(position - topPos);
        float dist2 = Vector3.Magnitude(position - botPos);
        if(Mathf.Approximately(dist1, dist2)) return -1;
        if(dist2 < dist1) return 1;
        if(dist1 > dist2) return 0;
        return -1;
    }

   // public static List<GameObject> FindClosestSides(PaperSquare s1, PaperSquare s2)
   // {
     //   int 
   // }

   /* 
    public static void JoinSquares(PaperSquare top, PaperSquare bottom, bool invertBottomPos = false)
    {
        if(invertBottomPos)
        {
            top.topStack = bottom;
            bottom.topStack = top;
        }
        top.bottomStack = bottom;
        bottom.topStack = top;
    }*/
}
