using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    new private Camera camera;

    private PaperJoint hoverJoint;
    private PaperJoint currJoint;

    public FoldChecker foldChecker;

    [SerializeField] private PaperSquare hoverSquare;

    public FoldablePaper foldablePaper;
    public FoldAnimator foldAnimator;
    public FoldObjects foldObjects;
    private GameObject ghostFold90;
    private GameObject ghostFoldNeg90;
    public GameObject ghostSquare;
    public Vector2 foldcenter90;
    public Vector2 foldcenterneg90;
    public Vector2 mousePos;
    public float dist90;
    public float distNeg90;
    public List<Transform> posList90 = new List<Transform>();
    public List<Transform> posListNeg90 = new List<Transform>();
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
        UpdateSquareRefs();
        UpdateGhostPosition();
    }

    private void UpdateSquareRefs()
    {
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
    }

    private void UpdateGhostPosition()
    {
        if(ghostFold90 == null) return;
        
        foldcenter90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenterTransform(posList90));
        foldcenterneg90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenterTransform(posListNeg90));

        mousePos = Mouse.current.position.ReadValue();
        dist90 = Vector3.Magnitude(mousePos - foldcenter90);
        distNeg90 = Vector3.Magnitude(mousePos - foldcenterneg90);

        if(dist90 < distNeg90){
            ghostFold90.SetActive(true);
            ghostFoldNeg90.SetActive(false);
        }
        else{
            ghostFold90.SetActive(false);
            ghostFoldNeg90.SetActive(true);
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

    private void OnClick(InputValue value)
    {
        if(foldablePaper == null || foldablePaper.isComplete || !value.isPressed || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        if(hoverJoint != null && hoverJoint.canFold)
        {
            if(currJoint == hoverJoint)
                return;
            currJoint?.Deselect();
            Destroy(ghostFold90);
            Destroy(ghostFoldNeg90);
            currJoint = hoverJoint;
            currJoint.Select();
            foldablePaper.foldJoint = currJoint;
            CreateGhostFold();
        }
        else
        {
            //fold base on direction
            if(dist90 < distNeg90)
                foldablePaper.TryFold(90);
            else
                foldablePaper.TryFold(-90);
           DeselectJoint();
        }
    }

    private void OnRightClick(InputValue value)
    {
        if(foldablePaper == null || foldablePaper.isComplete || !value.isPressed || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        DeselectJoint();
    }

    private void DeselectJoint()
    {
        currJoint?.Deselect();
        currJoint = null;
        foldablePaper.foldJoint = null;
        if(ghostFold90 != null){
            Destroy(ghostFold90);
            Destroy(ghostFoldNeg90);
        }
    }


    //TODO: update this POS to use fold checking
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

        ghostFold90 = new GameObject();
        ghostFold90.transform.position = foldObjects.foldJoints[0].transform.position;
        foreach(GameObject go in foldObjects.foldSquares)
        {
            GameObject newSquare = Instantiate(ghostSquare, go.transform.position, go.transform.rotation);
            newSquare.transform.parent = ghostFold90.transform;
        }
        ghostFold90.transform.RotateAround(foldObjects.foldJoints[0].transform.position, foldObjects.foldJoints[0].transform.rotation * Vector3.right, 90);
        int children = ghostFold90.transform.childCount;
        posList90.Clear();
        for (int i = 0; i < children; i++)
        {
            posList90.Add(ghostFold90.transform.GetChild(i));
        }
        foldcenter90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenterTransform(posList90));
        

        ghostFoldNeg90 = new GameObject();
        ghostFoldNeg90.transform.position = foldObjects.foldJoints[0].transform.position;
        foreach(GameObject go in foldObjects.foldSquares)
        {   
            GameObject newSquare = Instantiate(ghostSquare, go.transform.position, go.transform.rotation);
            newSquare.transform.parent = ghostFoldNeg90.transform;
        }
        ghostFoldNeg90.transform.RotateAround(foldObjects.foldJoints[0].transform.position, foldObjects.foldJoints[0].transform.rotation * Vector3.right, -90);
        posListNeg90.Clear();
        for (int i = 0; i < children; i++)
        {
            posListNeg90.Add(ghostFoldNeg90.transform.GetChild(i));
        }
        foldcenterneg90 = camera.WorldToScreenPoint(CoordUtils.CalculateCenterTransform(posListNeg90));
    
        ghostFold90.SetActive(false);
        ghostFoldNeg90.SetActive(false);
    }
    

    private void OnFoldUp(InputValue value)
    {
        if(!value.isPressed) return;
        FindObjectOfType<Goal>().EndLevel();
    }

}
