using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    new private Camera camera;

    private PaperJoint hover;
    private PaperJoint curr;


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
        if(value.isPressed && hover != null)
        {
            curr = hover;
            Debug.Log(curr);
        }
    }
}
