using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector2 : Singleton<TileSelector2>
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
    // private float dist90;
    // private float distNeg90;
     private FoldData2 foldData90;
     private FoldData2 foldDataNeg90;
    // private bool[] validFolds = new bool[2];

    public LayerMask paperMask;
    public LayerMask jointMask;

    public static event System.EventHandler<bool> OnFoldSelect;

    public SelectState state;

    public List<FoldableObject> targetState90 = new();
    public List<FoldableObject> targetStateNeg90 = new();
    //public List<SquareStack>[] foldStacks = new List<SquareStack>[2];

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
        camera = GetComponent<Camera>();
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
        if(ghostFold90 == null) return;
        Vector2 foldcenter90 = camera.WorldToScreenPoint(indicator90.Center);
        Vector2 foldcenterneg90 = camera.WorldToScreenPoint(indicatorNeg90.Center);
        Vector2 mousePos = Mouse.current.position.ReadValue();
        float dist90 = Vector3.Magnitude(mousePos - foldcenter90);
        float distNeg90 = Vector3.Magnitude(mousePos - foldcenterneg90);

        if(dist90 < distNeg90)
        {
            ghostFold90.SetActive(true);
            ghostFoldNeg90.SetActive(false);
        }
        else
        {
            ghostFold90.SetActive(false);
            ghostFoldNeg90.SetActive(true);
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
        CheckForValidFolds();
        CreateGhostFold();
        state = SelectState.SELECTED;
    }

    private void CheckForValidFolds()
    {
        if(FoldChecker2.Instance == null) return;
        foldData90 = foldablePaper.BuildFoldData2(90);
        targetState90 = FoldChecker2.Instance.GetFoldPosition(foldData90);

        foldDataNeg90 = foldablePaper.BuildFoldData2(-90);
        targetStateNeg90 = FoldChecker2.Instance.GetFoldPosition(foldDataNeg90);
    }

    private void CreateGhostFold()
    {
        ghostFold90 = Instantiate(indicatorPrefab);
        indicator90 = ghostFold90.GetComponent<FoldIndicator>();
        indicator90.BuildIndicator2(targetState90, camera);

        ghostFoldNeg90 = Instantiate(indicatorPrefab);
        indicatorNeg90 = ghostFoldNeg90.GetComponent<FoldIndicator>();
        indicatorNeg90.BuildIndicator2(targetStateNeg90, camera);
    }

    private void ChooseFoldDir()
    {
        // if(currJoint == null) return;
        // int caseNum = (validFolds[0]? 2: 0) + (validFolds[1]? 1: 0);
        // switch (caseNum)
        // {
        //     case 3:
        //         if(ghostFold90.activeSelf)
        //             foldAnimator.Fold(foldData90);
        //         else
        //             foldAnimator.Fold(foldDataNeg90);
        //         //if(dist90 < distNeg90)
        //            // foldAnimator.Fold(foldablePaper.BuildFoldData(90));
        //        // else
        //            // foldAnimator.Fold(foldablePaper.BuildFoldData(-90));
        //         break;
        //     case 2:
        //         foldAnimator.Fold(foldablePaper.BuildFoldData(90));
        //         break;
        //     case 1:
        //         foldAnimator.Fold(foldablePaper.BuildFoldData(-90));
        //         break;
        //     case 0:
        //         DeselectJoint(true);
        //         break;
        // }
        // if(caseNum != 0)
        //     state = SelectState.FOLDING;

        // DeselectJoint(false);
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
            foldData90 = null;
        }
        if(ghostFoldNeg90 != null){
            Destroy(ghostFoldNeg90);
            foldDataNeg90 = null;
        }

        //validFolds = new bool[] {false, false};
    }

    public void TryMakeNewGhost()
    {
        // if(currJoint != null && currJoint.canFold)
        // {
        //     currJoint.Select();
        //     foldablePaper.foldJoint = currJoint;
        //     CreateGhostFold();
        // }
    }

    
}

// public enum SelectState {
//     NONE, 
//     SELECTED,
//     FOLDING
// }
