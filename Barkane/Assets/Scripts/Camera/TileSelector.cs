using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    new private Camera camera;

    private PaperJoint hover;
    private PaperJoint curr;

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
            hover = info.transform.gameObject.GetComponent<PaperJoint>();
        } 
        else
        {
            hover = null;
        }
    }


    private void OnClick(InputValue value)
    {
        if(!value.isPressed || !CameraOrbit.Instance.CameraDisabled)
            return;
        if(hover != null && hover.canFold)
        {
            if(curr == hover)
                return;
            curr?.Deselect();
            curr = hover;
            curr.Select();
            foldablePaper.foldJoint = curr;
        }
        else
        {
            curr?.Deselect();
            curr = null;
        }
    }

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
