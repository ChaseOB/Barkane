using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperSquareStack : MonoBehaviour
{
    /* C: This class is used to handle sorting and enabling for when multiple papersquares are in the same grid location
        This is for visual purposes only. Just because all squares are in one stack does not mean that they will 
    */

    //lowest index = bottom of the stack, highest index = top of the stack 
    private List<PaperSquare> squares;
    public bool destroy = false;

    public void AddSquareTop(PaperSquare ps)
    {
        squares.Add(ps);
    }

    public void AddSquareBottom(PaperSquare ps)
    {
        squares.Insert(0, ps);
    }

    public void RemoveSquare(PaperSquare ps)
    {
        squares.Remove(ps);
    }

    public void TryRemoveSquare(PaperSquare ps)
    {
        if(squares.Contains(ps))
        {
            squares.Remove(ps);
            EnableSquareComponents();
        }
    }

    public bool Contains(PaperSquare ps)
    {
        return squares.Contains(ps);
    }

    public void EnableSquareComponents()
    {
        if(squares.Count <= 1)
        {
            foreach(PaperSquare ps in squares)
            {
                ps.TopHalf.SetActive(true);
                ps.BottomHalf.SetActive(true);
            }
            destroy = true;
        }
        foreach(PaperSquare ps in squares)
        {
            ps.TopHalf.SetActive(false);
            ps.BottomHalf.SetActive(false);
        }
        PaperSquare top = squares[squares.Count - 1];
        float topAngle = Quaternion.Angle(top.transform.rotation, this.transform.rotation);
        if(topAngle < 90.0f)
            top.TopHalf.SetActive(true);
        else
            top.BottomHalf.SetActive(true);
        PaperSquare bottom = squares[0];
        float bottomAngle = Quaternion.Angle(bottom.transform.rotation, this.transform.rotation);
        if(topAngle > 90.0f)
            bottom.TopHalf.SetActive(true);
        else
            bottom.BottomHalf.SetActive(true);
    }
}
