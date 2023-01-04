using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : Singleton<TileSelector>
{
    new private Camera camera;

    private PaperJoint hoverJoint;
    private PaperJoint prevHoverJoint;
    private PaperJoint currJoint;

    public FoldChecker foldChecker;

    [SerializeField] private PaperSquare hoverSquare;

    public GameObject indicatorPrefab;

    public FoldablePaper foldablePaper;
    public FoldAnimator foldAnimator;
    private GameObject ghostFold90;
    private GameObject ghostFoldNeg90;
    private FoldIndicator indicator90;
    private FoldIndicator indicatorNeg90;
    private float dist90;
    private float distNeg90;

    public LayerMask paperMask;
    public LayerMask jointMask;

    public static event System.EventHandler<bool> OnFoldSelect;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        ReloadReferences();
    }

    public void ReloadReferences()
    {
        camera = this.GetComponent<Camera>();
        foldAnimator = FindObjectOfType<FoldAnimator>();
        foldablePaper = FindObjectOfType<FoldablePaper>();
    }

    private void Update()
    {
        if(foldablePaper.isComplete) return;
        UpdateSquareRefs();
        UpdateJointHoverIndicator();
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
            hoverSquare = info.transform.gameObject.GetComponent<PaperSquare>(); 

    }

    private void UpdateJointHoverIndicator()
    {
        if(prevHoverJoint != hoverJoint)
        {
            prevHoverJoint?.OnHoverExit();
            hoverJoint?.OnHoverEnter();
        }
        prevHoverJoint = hoverJoint;
    }

    private void UpdateGhostPosition()
    {
        if(ghostFold90 == null || ghostFoldNeg90 == null || indicator90 == null || indicatorNeg90 == null) return;
        
        Vector2 foldcenter90 = indicator90.foldCenter;
        Vector2 foldcenterneg90 = indicatorNeg90.foldCenter;
        Vector2 mousePos = Mouse.current.position.ReadValue();
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

    

    private void OnClick(InputValue value)
    {
        if(!value.isPressed) return;
        ChooseClickAction();
    }

    private void ChooseClickAction()
    {
        if(foldablePaper == null || foldablePaper.isComplete || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        if(hoverJoint != null && hoverJoint.canFold)            
            SelectNewJoint();
        else
            ChooseFoldDir();
    }

    private void SelectNewJoint()
    {
        OnFoldSelect?.Invoke(this, true);
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

    private void ChooseFoldDir()
    {
        if(dist90 < distNeg90)
            foldablePaper.TryFold(90);
        else
            foldablePaper.TryFold(-90);
        DeselectJoint();
    }

    private void OnRightClick(InputValue value)
    {
        if(foldablePaper == null || foldablePaper.isComplete || !value.isPressed || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        DeselectJoint();
    }

    public void DeselectJoint()
    {
        OnFoldSelect.Invoke(this, false);
        currJoint?.Deselect();
        currJoint = null;
        foldablePaper.foldJoint = null;
        if(ghostFold90 != null){
            Destroy(ghostFold90);
            Destroy(ghostFoldNeg90);
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
        FoldData fd1 = foldablePaper.BuildFoldData(90);

        if(!FoldChecker.CheckKinkedJoint(fd1.axisJoints)) return;

        ghostFold90 = Instantiate(indicatorPrefab);
        indicator90 = ghostFold90.GetComponent<FoldIndicator>();
        indicator90.BuildIndicator(fd1, camera);

        FoldData fd2 = foldablePaper.BuildFoldData(-90);

        ghostFoldNeg90 = Instantiate(indicatorPrefab);
        indicatorNeg90 = ghostFoldNeg90.GetComponent<FoldIndicator>();
        indicatorNeg90.BuildIndicator(fd2, camera);        

        ghostFold90.SetActive(false);
        ghostFoldNeg90.SetActive(false);
    }


    private void OnFoldUp(InputValue value)
    {
        if(!value.isPressed) return;
        FindObjectOfType<Goal>().EndLevel();
    }

}
