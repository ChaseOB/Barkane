using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PaperSquares : MonoBehaviour
{
    public const int SIZE = 21;
    public readonly Vector3Int center = new Vector3Int(SIZE / 2, 0, SIZE / 2);
    private PaperSqaure[,] paperSquares = new PaperSqaure[SIZE, SIZE];
    [SerializeField] private PaperSqaure copyFrom;

    // Returns true if a new square is instantiated. Determines drag type
    public bool SelectSquare(Vector3 absPos, out PaperSqaure square)
    {
        Vector3Int relPos = GetRelativePosition(absPos);
        square = GetSquareAt(relPos);

        if (square != null)
        {
            copyFrom = square;
            return false;
        }

        Vector3 centerPos = GetAbsolutePosition(relPos);
        square = Instantiate<PaperSqaure>(copyFrom, centerPos, copyFrom.transform.rotation, this.transform);
        SetSquareAt(relPos, square);
        return true;
    }

    public PaperSqaure RemoveSquare(Vector3 selectPos)
    {
        Vector3Int relPos = GetRelativePosition(selectPos);
        PaperSqaure square = GetSquareAt(relPos);

        if (square == null || relPos.Equals(center))
        {
            return null;
        }

        RemoveReference(relPos);
        if (square.Equals(copyFrom))
        {
            copyFrom = GetSquareAt(center);
        }
        return square;
    }

    private void Start()
    {
        PaperSqaure[] squares = GetComponentsInChildren<PaperSqaure>();
        foreach (PaperSqaure square in squares)
        {
            Vector3Int relPos = GetRelativePosition(square.transform.position);
            SetSquareAt(relPos, square);
        }
        copyFrom = GetSquareAt(center);
    }

    // Implementation Specific

    private Vector3Int GetRelativePosition(Vector3 absPos)
    {
        int relX = (int) ((absPos.x + 21) / 2);
        int relY = 0;
        int relZ = (int) ((absPos.z + 21) / 2);

        return new Vector3Int(relX, relY, relZ);
    }

    private Vector3 GetAbsolutePosition(Vector3Int relPos)
    {
        float absX = relPos.x * 2 - 20;
        float absY = 0;
        float absZ = relPos.z * 2 - 20;

        return new Vector3(absX, absY, absZ);
    }

    private PaperSqaure GetSquareAt(Vector3Int relPos)
    {
        return paperSquares[relPos.x, relPos.z];
    }

    private void SetSquareAt(Vector3Int relPos, PaperSqaure square)
    {
        paperSquares[relPos.x, relPos.z] = square;
    }

    private void RemoveReference(Vector3Int relPos)
    {
        SetSquareAt(relPos, null);
    }
}
