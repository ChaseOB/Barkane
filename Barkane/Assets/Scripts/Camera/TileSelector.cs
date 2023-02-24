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
    private FoldData foldData90;
    private FoldData foldDataNeg90;
    private bool[] validFolds = new bool[2];

    public LayerMask paperMask;
    public LayerMask jointMask;

    public static event System.EventHandler<bool> OnFoldSelect;

    public SelectState state;

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
        state = SelectState.NONE;
    }

    private void Update()
    {
        if(PauseManager.IsPaused) return;
        if(foldablePaper.isComplete) return;
        UpdateSquareRefs();
        UpdateJointHoverIndicator();
        UpdateGhostPosition();
    }

    private void UpdateSquareRefs()
    {
        if(!CameraOrbit.Instance.CameraDisabled) return;
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
        if(prevHoverJoint != hoverJoint && state == SelectState.NONE)
        {
            prevHoverJoint?.OnHoverExit();
            hoverJoint?.OnHoverEnter();
        }
        prevHoverJoint = hoverJoint;
    }

    private void UpdateGhostPosition()
    {
        int caseNum = (validFolds[0]? 2: 0) + (validFolds[1]? 1: 0);
        switch (caseNum)
        {
            case 3:
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
                break;
            case 2:
                ghostFold90.SetActive(true);
                break;
            case 1:
                ghostFoldNeg90.SetActive(true);
                break;
            case 0:
                break;
        }        
    }

    

    private void OnClick(InputValue value)
    {
        if(PauseManager.IsPaused) return;
        if(!value.isPressed) return;
        if(ActionLockManager.Instance.IsLocked) return;
        ChooseClickAction();
    }

    private void ChooseClickAction()
    {
        if(foldablePaper == null || foldablePaper.isComplete || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        switch(state)
        {
            case SelectState.NONE:
                if(hoverJoint != null && hoverJoint.canFold)            
                    SelectNewJoint();
                break;
            case SelectState.SELECTED:
                    ChooseFoldDir();
                break;
            case SelectState.FOLDING:
                break;
        }
        
    }

    private void SelectNewJoint()
    {
        if(currJoint == hoverJoint)
            return;
        OnFoldSelect?.Invoke(this, true);
        
        DeselectJoint();
        currJoint = hoverJoint;
        currJoint.Select();
        foldablePaper.foldJoint = currJoint;
        CreateGhostFold();
        state = SelectState.SELECTED;
    }

    private void ChooseFoldDir()
    {
        if(currJoint == null) return;
        int caseNum = (validFolds[0]? 2: 0) + (validFolds[1]? 1: 0);
        switch (caseNum)
        {
            case 3:
                if(dist90 < distNeg90)
                    foldAnimator.Fold(foldablePaper.BuildFoldData(90));
                else
                    foldAnimator.Fold(foldablePaper.BuildFoldData(-90));
                break;
            case 2:
                foldAnimator.Fold(foldablePaper.BuildFoldData(90));
                break;
            case 1:
                foldAnimator.Fold(foldablePaper.BuildFoldData(-90));
                break;
            case 0:
                DeselectJoint(true);
                break;
        }
        if(caseNum != 0)
            state = SelectState.FOLDING;

        DeselectJoint(false);
    }

    private void OnRightClick(InputValue value)
    {
        if(foldablePaper == null || foldablePaper.isComplete || !value.isPressed || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        DeselectJoint();
    }

    public void DeselectJoint(bool updateState = true)
    {
        if(updateState)
            state = SelectState.NONE;
        OnFoldSelect?.Invoke(this, false);
        currJoint?.Deselect();
        currJoint = null;
        foldablePaper.foldJoint = null;
        if(ghostFold90 != null){
            Destroy(ghostFold90);
        }
        if(ghostFoldNeg90 != null){
            Destroy(ghostFoldNeg90);
        }

        validFolds = new bool[] {false, false};
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
        if(FoldChecker.Instance == null) return;
        FoldData fd1 = foldablePaper.BuildFoldData(90);
        FoldFailureType failureType1 = FoldChecker.Instance.CheckFold(fd1);
        if(failureType1 == FoldFailureType.KINKED ||
            failureType1 == FoldFailureType.NOCHECK)
            return;
        if(failureType1 == FoldFailureType.NONE)
        {
            ghostFold90 = Instantiate(indicatorPrefab);
            indicator90 = ghostFold90.GetComponent<FoldIndicator>();
            indicator90.BuildIndicator(fd1, camera);
            ghostFold90.SetActive(false);
            validFolds[0] = true;
        }

        FoldData fd2 = foldablePaper.BuildFoldData(-90);
        FoldFailureType failureType2 = FoldChecker.Instance.CheckFold(fd2);
        if(failureType2 == FoldFailureType.KINKED ||
            failureType2 == FoldFailureType.NOCHECK)
            return;
        if(failureType2 == FoldFailureType.NONE)
        {
            ghostFoldNeg90 = Instantiate(indicatorPrefab);
            indicatorNeg90 = ghostFoldNeg90.GetComponent<FoldIndicator>();
            indicatorNeg90.BuildIndicator(fd2, camera);
            ghostFoldNeg90.SetActive(false);
            validFolds[1] = true;        
        }
        
        }


    private void OnFoldUp(InputValue value)
    {
        if(!value.isPressed) return;
        FindObjectOfType<Goal>().EndLevel();
    }

}

public enum SelectState {
    NONE, 
    SELECTED,
    FOLDING
}
