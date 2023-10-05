using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsUI : MonoBehaviour
{
    public GameObject[] stars = new GameObject[3];

    public void DisplayStars(Level level, int numFolds)
    {
        int numStars = 0;
        if(numFolds <= level.ThreeStarMaxFolds)
            numStars = 3;
        else if (numFolds <= level.TwoStarMaxFolds)
            numStars = 2;
        else if(numFolds <= level.OneStarMaxFolds)
            numStars = 1;

        for(int i = 0; i < 3; i++)
        {
            stars[i].SetActive(numStars > i);
        }
    }
}
