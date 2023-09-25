using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JointStack: FoldableObject
{
    public LinkedList<JointData> jointList = new();

    public JointStack(PaperJoint paperJoint)
    {
        PositionData positionData = new(
            Vector3Int.RoundToInt(paperJoint.transform.position),
            paperJoint.transform.rotation,
            GetAxisFromCoordinates( Vector3Int.RoundToInt(paperJoint.transform.position))
        );

        JointData jointData = new(positionData, paperJoint);
        jointList.AddFirst(jointData);
        currentPosition = positionData;
        targetPosition = positionData;
    }

    public static Vector3 GetAxisFromCoordinates(Vector3Int coordinates)
    {
        if(coordinates.x % 2 == 0) return Vector3.right;
        if(coordinates.y % 2 == 0) return Vector3.up;
        return Vector3.forward;
    }

    public override void SendToTarget()
    {
        foreach(JointData jd in jointList)
        {
            jd.SendToTarget();
        }  
    }

    public override void SetParent(Transform parent)
    {
        throw new System.NotImplementedException();
    }


}
