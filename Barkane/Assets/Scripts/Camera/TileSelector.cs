using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    new private Camera camera;

    private PaperJoint hoverJoint;
    private PaperJoint currJoint;

    private PaperSqaure hoverSquare;
    private PaperSqaure currSquare;

    public FoldablePaper foldablePaper;


    // Start is called before the first frame update
    void Start()
    {
        camera = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit info;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Joint")))
        {
            hoverJoint = info.transform.gameObject.GetComponent<PaperJoint>();
        } 
        else
        {
            hoverJoint = null;
        }
    }


    private void OnClick(InputValue value)
    {
        if(!value.isPressed || !CameraOrbit.Instance.CameraDisabled)
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
