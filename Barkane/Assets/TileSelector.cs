using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    new private Camera camera;

    private PaperSqaure hoverSquare;
    private PaperSqaure currentSquare;


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
        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Paper")))
        {
            if(info.transform.gameObject.GetComponent<PaperSqaure>())
                hoverSquare = info.transform.gameObject.GetComponent<PaperSqaure>();
        } 
        else
        {
            hoverSquare = null;
        }
    }


    private void OnClick(InputValue value)
    {
        if(value.isPressed && hoverSquare != null)
        {
            currentSquare = hoverSquare;
            Debug.Log(currentSquare);
        }
    }
}
