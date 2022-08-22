using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperJoint : MonoBehaviour
{
    [SerializeField] private PaperSqaure paperSqaure1;
    [SerializeField] private PaperSqaure paperSqaure2;
    private bool isSelected = false;

   // private bool JointEnabled = true; //CO: Set to false to "cut" the paper along the given joint
    private bool canFold = true; //CO: Set to false to lock the current joint in position, as if the squares were glued together

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        isSelected = true;
    }

    public void Deselect()
    {
        isSelected = false;
    }

}
