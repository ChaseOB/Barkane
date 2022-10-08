using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PaperSquares : MonoBehaviour
{
    public const int SIZE = 43; // Should be 4k + 3 form, k is approximately # of valid squares per direction
    public readonly Vector3Int center = new Vector3Int(SIZE / 2, SIZE / 4 * 2, SIZE / 2);
    [SerializeField] private MonoBehaviour[,,] paperSquares = new MonoBehaviour[SIZE, SIZE, SIZE];
    [SerializeField] public GameObject squareList;
    // [SerializeField] GameObject jointList;

    public enum VertexType
    {
        Void,   // Should contain null
        Plane,  // Should contain PaperSquare
        Edge,   // Should contain Joint
        Vertex  // Should contain null (for now)
    }

    public PaperSqaure GetCenter()
    {
        return GetSquareAt(center);
    }

    public bool ExistsAt(Vector3Int relPos)
    {
        return GetSquareAt(relPos) != null;
    }

    private void Awake()
    {
        PaperSqaure[] squares = squareList.GetComponentsInChildren<PaperSqaure>();
        foreach (PaperSqaure square in squares)
        {
            Vector3Int relPos = GetRelativePosition(square.transform.position);
            SetSquareAt(relPos, square);
        }
    }

    // paperSquares Implementation Specifics

    public Vector3Int GetNearestSquarePos(Vector3 absPos, Orientation orientation)
    {
        int relX = 2 * (int)((absPos.x + SIZE / 2) / 2) + 1;
        int relY = 2 * (int)((absPos.y + SIZE / 2) / 2) + 1;
        int relZ = 2 * (int)((absPos.z + SIZE / 2) / 2) + 1;

        switch (orientation)
        {
            case Orientation.YZ:
                relX = Mathf.RoundToInt(absPos.x + SIZE / 2);
                break;
            case Orientation.XZ:
                relY = Mathf.RoundToInt(absPos.y + SIZE / 2);
                break;
            case Orientation.XY:
                relZ = Mathf.RoundToInt(absPos.z + SIZE / 2);
                break;
        }

        return new Vector3Int(relX, relY, relZ);
    }

    public Vector3Int GetRelativePosition(Vector3 absPos)
    {
        int relX = Mathf.RoundToInt(absPos.x + SIZE / 2);
        int relY = Mathf.RoundToInt(absPos.y + SIZE / 2);
        int relZ = Mathf.RoundToInt(absPos.z + SIZE / 2);

        return new Vector3Int(relX, relY, relZ);
    }

    public Vector3 GetAbsolutePosition(Vector3Int relPos)
    {
        float absX = relPos.x - SIZE / 2;
        float absY = relPos.y - SIZE / 2;
        float absZ = relPos.z - SIZE / 2;

        return new Vector3(absX, absY, absZ);
    }

    private VertexType WhichVertex(Vector3Int relPos)
    {
        int centerCount = relPos.x % 2 + relPos.y % 2 + relPos.z % 2;
        switch (centerCount)
        {
            case 0:
                return VertexType.Vertex;
            case 1:
                return VertexType.Edge;
            case 2:
                return VertexType.Plane;
            default:
                return VertexType.Void;
        }
    }

    public MonoBehaviour GetObjectAt(Vector3Int relPos)
    {
        return paperSquares[relPos.x, relPos.y, relPos.z];
    }

    public PaperSqaure GetSquareAt(Vector3Int relPos)
    {
        return GetObjectAt(relPos) as PaperSqaure;
    }

    public void SetSquareAt(Vector3Int relPos, PaperSqaure square)
    {
        paperSquares[relPos.x, relPos.y, relPos.z] = square;
    }

    void RemoveRefernceMessage(Vector3 position)
    {
        RemoveReference(GetRelativePosition(position));
    }

    public void RemoveReference(Vector3Int relPos)
    {
        SetSquareAt(relPos, null);
    }
}
