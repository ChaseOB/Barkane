using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    new private Camera camera;

    private PaperJoint hoverJoint;
    private PaperJoint currJoint;

    [SerializeField] private PaperSquare hoverSquare;

    public FoldablePaper foldablePaper;
    public FoldAnimator foldAnimator;
    public FoldObjects foldObjects;
    private GameObject ghostFold;
    private GameObject ghostFold2;
    public GameObject ghostSquare;
    public Vector2 foldcenter90;
    public Vector2 foldcenterneg90;
    public float udist;
    public float ddist;
    public List<Vector3> posList = new List<Vector3>();
    public List<Vector3> posList2 = new List<Vector3>();
    public LayerMask paperMask;
    public LayerMask jointMask;



    // Start is called before the first frame update
    void Start()
    {
        ReloadReferences();
    }

    public void ReloadReferences()
    {
        camera = this.GetComponent<Camera>();
        foldAnimator = FindObjectOfType<FoldAnimator>();
        foldablePaper = FindObjectOfType<FoldablePaper>();
    }

    // Update is called once per frame
    void Update()
    {
        if(foldablePaper.isComplete) return;

        RaycastHit info;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 100, jointMask);
        if(hits.Length == 0)
            hoverJoint = null;
        else if (hits.Length == 1)
            hoverJoint = hits[0].transform.gameObject.GetComponent<PaperJoint>();
        else
        {
            foreach (RaycastHit hit in hits)
            {
                PaperJoint joint = hit.transform.gameObject.GetComponent<PaperJoint>();
                if (hoverSquare != null && joint.PaperSquares.Contains(hoverSquare))
                    hoverJoint = joint;
            }
        }

        if(Physics.Raycast(ray, out info, 100, paperMask))
        {
            hoverSquare = info.transform.gameObject.GetComponent<PaperSquare>();
        } 
        
        if(ghostFold == null) return;
        
        foldcenter90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenter(posList));
        foldcenterneg90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenter(posList2));

        Vector2 mousePos = Mouse.current.position.ReadValue();
        udist = Vector3.Magnitude(mousePos - foldcenter90);
        ddist = Vector3.Magnitude(mousePos - foldcenterneg90);
        if(udist < ddist){
            ghostFold.SetActive(true);
            ghostFold2.SetActive(false);
        }
        else{
            ghostFold.SetActive(false);
            ghostFold2.SetActive(true);
        }
    }

    public void TryMakeNewGhost()
    {
        if(currJoint != null && currJoint.canFold)
        {
            currJoint.Select();
            foldablePaper.foldJoint = currJoint;
            CreateGhostFold();
        }
    }
    private void CreateGhostFold()
    {
                    foldObjects = foldablePaper.FindFoldObjects()[1];

        HashSet<int> x = new HashSet<int>();
        HashSet<int> y = new HashSet<int>();
        HashSet<int> z = new HashSet<int>();

        foreach(PaperJoint pj in foldablePaper.PaperJoints)
        {
            if(pj.showLine)
            {
                x.Add(Vector3Int.RoundToInt(pj.transform.position).x);
                y.Add(Vector3Int.RoundToInt(pj.transform.position).y);
                z.Add(Vector3Int.RoundToInt(pj.transform.position).z);
            }
        }

        if((x.Count > 1 && y.Count > 1) || (x.Count > 1 && z.Count > 1) || (z.Count > 1 && y.Count > 1)) {
            Debug.Log($"Cannot make ghost: joint is kinked. {x.Count} {y.Count} {z.Count}");
            return;
        }

        ghostFold = new GameObject();
        ghostFold.transform.position = foldObjects.foldJoints[0].transform.position;
        foreach(GameObject go in foldObjects.foldSquares)
        {
            GameObject newSquare = Instantiate(ghostSquare, go.transform.position, go.transform.rotation);
            newSquare.transform.parent = ghostFold.transform;
        }
        ghostFold.transform.RotateAround(foldObjects.foldJoints[0].transform.position, foldObjects.foldJoints[0].transform.rotation * Vector3.right, 90);
        int children = ghostFold.transform.childCount;
        for (int i = 0; i < children; i++)
        {
            posList.Add(ghostFold.transform.GetChild(i).position);
        }
        foldcenter90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenter(posList));
        
        ghostFold2 = new GameObject();
        ghostFold2.transform.position = foldObjects.foldJoints[0].transform.position;
        foreach(GameObject go in foldObjects.foldSquares)
        {
            GameObject newSquare = Instantiate(ghostSquare, go.transform.position, go.transform.rotation);
            newSquare.transform.parent = ghostFold2.transform;
        }
        ghostFold2.transform.RotateAround(foldObjects.foldJoints[0].transform.position, foldObjects.foldJoints[0].transform.rotation * Vector3.right, -90);
        List<Vector3> posList2 = new List<Vector3>();
        for (int i = 0; i < children; i++)
        {
            posList2.Add(ghostFold2.transform.GetChild(i).position);
        }
        foldcenterneg90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenter(posList2));
    
        ghostFold.SetActive(false);
        ghostFold2.SetActive(false);
    }

    //private void Destroy

    private void OnClick(InputValue value)
    {
        if(foldablePaper == null || foldablePaper.isComplete || !value.isPressed || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        if(hoverJoint != null && hoverJoint.canFold)
        {
            if(currJoint == hoverJoint)
                return;
            currJoint?.Deselect();
            Destroy(ghostFold);
            Destroy(ghostFold2);
            currJoint = hoverJoint;
            currJoint.Select();
            foldablePaper.foldJoint = currJoint;
            CreateGhostFold();
        }
        else
        {
            //fold base on direction
            if(udist < ddist)
                foldablePaper.TryFold(90);
            else
                foldablePaper.TryFold(-90);
            Destroy(ghostFold);
            Destroy(ghostFold2);
           /* currJoint?.Deselect();
            currJoint = null;
            foldablePaper.foldJoint = null;*/
        }
    }

    private void OnRightClick(InputValue value)
    {
        if(foldablePaper == null || foldablePaper.isComplete || !value.isPressed || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        currJoint?.Deselect();
        currJoint = null;
        foldablePaper.foldJoint = null;
        if(ghostFold != null){
            Destroy(ghostFold);
            Destroy(ghostFold2);
        }
    }


    private void OnFoldUp(InputValue value)
    {
        if(!value.isPressed) return;
        FindObjectOfType<Goal>().EndLevel();
        //if(!value.isPressed || currJoint == null || !currJoint.isSelected)
          //  return;
       // foldablePaper.TryFold(90);
    }
/*
    private void OnFoldDown(InputValue value)
    {
        if(!value.isPressed || currJoint == null || !currJoint.isSelected)
            return;
        foldablePaper.TryFold(-90);
    }*/
}
