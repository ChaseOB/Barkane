using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on the Joints
// Finds the correct PaperSquares to interact with

public class EmitEdgeParticles : MonoBehaviour
{
    PaperJoint currentJoint;
    List<PaperSqaure> paperSqaures;
    FoldablePaper foldablePaper;
    FoldAnimator foldAnimator;
    List<GameObject> willBeFoldedAll;
    List<PaperSqaure> willBeFoldedPaperSquares = new List<PaperSqaure>();
    bool atCapacity = false;

    void Start()
    {
        currentJoint = gameObject.GetComponent<PaperJoint>(); // the clicked joint
        paperSqaures = currentJoint.PaperSqaures;
        foldablePaper = FindObjectOfType<FoldablePaper>();
        foldAnimator = foldablePaper.GetComponent<FoldAnimator>();
        willBeFoldedAll = foldablePaper.getFoldSide();
    }

// PROBLEM: after selecting a different Joint
// when selecting the original joint,
// the ps from the 2nd joint dont activate/play

    void Update()
    {
        print("size of willBeFoldedPS is " + willBeFoldedPaperSquares.Count.ToString());
        print("atCapacity is " + atCapacity.ToString());
        if (currentJoint.getIsSelected()) {
            print(currentJoint.ToString() + " is selected! time to emit particles around what can be folded");
            if (!atCapacity) {
                foldablePaper.FindFoldObjects();
                willBeFoldedAll = foldablePaper.getFoldSide();

                print("willBeFoldedAll.Count = " + willBeFoldedAll.Count.ToString());
                for (int i = 0; i < willBeFoldedAll.Count; i++) {
                    print("within the all to ps " + willBeFoldedAll[i].ToString());
                    if (willBeFoldedAll[i].GetComponent<PaperSqaure>() != null) {
                        willBeFoldedPaperSquares.Add(willBeFoldedAll[i].GetComponent<PaperSqaure>());
                    }
                }

                atCapacity = true;
            }

            for (int i = 0; i < willBeFoldedPaperSquares.Count; i++) {
                print("in emitting from ps; index = " + i.ToString());
                willBeFoldedPaperSquares[i].GetComponent<EdgeParticles>().Emit();
            }
        } else {
            print("count for unemeit loop: " + willBeFoldedPaperSquares.Count.ToString());
            if (atCapacity) {
                for (int i = 0; i < willBeFoldedPaperSquares.Count; i++) {
                    print("en route to unemitting from ps; index = " + i.ToString());
                    willBeFoldedPaperSquares[i].GetComponent<EdgeParticles>().Unemit();
                }
            }
        }
    }
}