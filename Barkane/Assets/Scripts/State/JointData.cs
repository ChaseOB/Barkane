using UnityEngine;

[System.Serializable]
public class JointData: FoldableObject
{
    public PaperJoint paperJoint;
    public Vector3 currentOffset;
    public Vector3 targetOffset;

    
    public JointData(PaperJoint paperJoint)
    {
        PositionData positionData = new(
            Vector3Int.RoundToInt(paperJoint.transform.position),
            paperJoint.transform.rotation,
            GetAxisFromCoordinates( Vector3Int.RoundToInt(paperJoint.transform.position))
        );
        currentPosition = positionData;
        targetPosition = positionData;
        this.paperJoint = paperJoint;
        storedParent = paperJoint.transform.parent;
    }

    public JointData(PositionData position, PaperJoint paperJoint)
    {
        currentPosition = position;
        targetPosition = position;
        this.paperJoint = paperJoint;
        storedParent = paperJoint.transform.parent;

    }

    public JointData(PositionData currentPosition, PositionData targetPosition, PaperJoint paperJoint)
    {
        this.currentPosition = currentPosition;
        this.targetPosition = targetPosition;
        this.paperJoint = paperJoint;
        storedParent = paperJoint.transform.parent;
    }

    public static Vector3 GetAxisFromCoordinates(Vector3Int coordinates)
    {
        if(coordinates.x % 2 == 0) return Vector3.right;
        if(coordinates.y % 2 == 0) return Vector3.up;
        return Vector3.forward;
    }

    public override void SendToTarget()
    {
        currentPosition = targetPosition;
        currentOffset = targetOffset;
        paperJoint.transform.position = currentPosition.location;
        paperJoint.transform.rotation = currentPosition.rotation;
        //paperJoint.Offset = currentYOffset;
    }

    public override void SetParent(Transform parent)
    {
        if(parent != null)
        {
            paperJoint.transform.parent = parent;
        }
        else
        {
            paperJoint.transform.parent = storedParent;
        }
    }
}
