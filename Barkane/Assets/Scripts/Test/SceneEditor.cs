using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEditor : MonoBehaviour
{
    [SerializeField] private Tiles tiles;
    [SerializeField] private bool editorOn = false;
    private Plane curPlane = new Plane(Vector3.up, Vector3.zero);

    private void Awake()
    {
        tiles = GetComponent<Tiles>();
    }

    private void OnToggleEditor()
    {
        editorOn = !editorOn;
    }

    private void OnClick()
    {
        if (editorOn)
        {
            float enterDist = 0.0f;
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (curPlane.Raycast(mouseRay, out enterDist))
            {
                Vector3 hitPoint = mouseRay.GetPoint(enterDist);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
