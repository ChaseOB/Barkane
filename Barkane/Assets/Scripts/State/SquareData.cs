using UnityEngine;

[System.Serializable]
public class SquareData: FoldableObject
{
    public PaperSquare paperSquare;
    public float currentYOffset;
    public float targetYOffset;

    public SquareData(PositionData position, PaperSquare paperSquare)
    {
        currentPosition = new(position);
        targetPosition = new(position);
        this.paperSquare = paperSquare;
        storedParent = paperSquare.transform.parent;
    }

    public SquareData(PositionData currentPosition, PositionData targetPosition, PaperSquare paperSquare)
    {
        this.currentPosition = new(currentPosition);
        this.targetPosition = new(targetPosition);
        this.paperSquare = paperSquare;
        storedParent = paperSquare.transform.parent;
    }

    public override void SendToTarget()
    {
        currentPosition = targetPosition;
        currentYOffset = targetYOffset;
        paperSquare.transform.position = currentPosition.location;
        paperSquare.transform.rotation = currentPosition.rotation;
        paperSquare.YOffset = currentYOffset;
    }

    public override void SetParent(Transform parent)
    {
        if(parent != null)
        {
            paperSquare.transform.parent = parent;
        }
        else
        {
            paperSquare.transform.parent = storedParent;
        }
    }

    public void SetTargetYOffset(float offset)
    {
        targetYOffset = offset;
    }
}
