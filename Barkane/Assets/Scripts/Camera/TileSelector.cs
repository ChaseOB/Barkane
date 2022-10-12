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


    // Start is called before the first frame update
    void Start()
    {
        camera = this.GetComponent<Camera>();
        foldAnimator = FindObjectOfType<FoldAnimator>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit info;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 100, LayerMask.GetMask("Joint"));
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

        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Paper")))
        {
            hoverSquare = info.transform.gameObject.GetComponent<PaperSquare>();
        } 
          
    }


    private void OnClick(InputValue value)
    {
        if(!value.isPressed || !CameraOrbit.Instance.CameraDisabled || foldAnimator.isFolding)
            return;
        if(hoverJoint != null && hoverJoint.canFold)
        {
            if(currJoint == hoverJoint)
                return;
            currJoint?.Deselect();
            currJoint = hoverJoint;
            currJoint.Select();
            foldablePaper.foldJoint = currJoint;
        }
        else
        {
            currJoint?.Deselect();
            currJoint = null;
        }

        foldablePaper.FindFoldObjects();
    }


    //C: These are only here for testing purposes
    private void OnFoldUp(InputValue value)
    {
        Debug.Log("fold up");
        if(!value.isPressed)
            return;
        foldablePaper.TestFold(90);
    }

    private void OnFoldDown(InputValue value)
    {
        if(!value.isPressed)
            return;
        foldablePaper.TestFold(-90);
    }
}
