using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperSquareStack : MonoBehaviour
{
    /* C: This class is used to handle sorting and enabling for when multiple papersquares are in the same grid location
        This is for visual purposes only. Just because all squares are in one stack does not mean that they will 
    */

    //lowest index = bottom of the stack, highest index = top of the stack 
    private List<PaperSqaure> sqaures;
    public bool destroy = false;

    public void AddSquareTop(PaperSqaure ps)
    {
        sqaures.Add(ps);
    }

    public void AddSquareBottom(PaperSqaure ps)
    {
        sqaures.Insert(0, ps);
    }

    public void RemoveSquare(PaperSqaure ps)
    {
        sqaures.Remove(ps);
    }

    public void TryRemoveSquare(PaperSqaure ps)
    {
        if(sqaures.Contains(ps))
        {
            sqaures.Remove(ps);
            EnableSquareComponents();
        }
    }

    public bool Contains(PaperSqaure ps)
    {
        return sqaures.Contains(ps);
    }

    public void EnableSquareComponents()
    {
        if(sqaures.Count <= 1)
        {
            foreach(PaperSqaure ps in sqaures)
            {
                ps.TopHalf.SetActive(true);
                ps.BottomHalf.SetActive(true);
            }
            destroy = true;
        }
        foreach(PaperSqaure ps in sqaures)
        {
            ps.TopHalf.SetActive(false);
            ps.BottomHalf.SetActive(false);
        }
        PaperSqaure top = sqaures[sqaures.Count - 1];
        float topAngle = Quaternion.Angle(top.transform.rotation, this.transform.rotation);
        if(topAngle < 90.0f)
            top.TopHalf.SetActive(true);
        else
            top.BottomHalf.SetActive(true);
        PaperSqaure bottom = sqaures[0];
        float bottomAngle = Quaternion.Angle(bottom.transform.rotation, this.transform.rotation);
        if(topAngle > 90.0f)
            bottom.TopHalf.SetActive(true);
        else
            bottom.BottomHalf.SetActive(true);
    }
}
