using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PaperSquares : MonoBehaviour
{
    public const int SIZE = 21;
    public readonly Vector3Int center = new Vector3Int(10, 0, 10);
    private PaperSqaure[,] paperSquares = new PaperSqaure[SIZE, SIZE];
    [SerializeField] private PaperSqaure copyFrom;

    public void SelectSquare(Vector3 selectPos)
    {
        Vector3Int relPos = GetRelativePosition(selectPos);
        if (checkNotValid(relPos))
        {
            return;
        }
        PaperSqaure square = GetPaperAt(relPos);

        if (square != null)
        {
            copyFrom = square;
        } else
        {
            Vector3 absPos = GetAbsolutePosition(relPos);
            PaperSqaure newSquare = Instantiate<PaperSqaure>(copyFrom, absPos, copyFrom.transform.rotation, this.transform);
            SetPaperAt(relPos, newSquare);
        }
    }

    public void RemoveSquare(Vector3 selectPos)
    {
        Vector3Int relPos = GetRelativePosition(selectPos);
        if (checkNotValid(relPos))
        {
            return;
        }
        PaperSqaure square = GetPaperAt(relPos);

        if (square == null)
        {
            // Debug.Log(string.Format("Error: Deleting nonexistant PaperSquare at {0}", selectPos));
            return;
        }
        if (!relPos.Equals(center))
        {
            RemoveReference(relPos);
            if (square.Equals(copyFrom))
            {
                copyFrom = GetPaperAt(center);
            }
            DestroyImmediate(square.gameObject);
        }
    }

    // Implementation Specific
    private void Start()
    {
        SetPaperAt(center, copyFrom);
    }

    private bool checkNotValid(Vector3Int relPos)
    {
        return relPos.x < 0 || relPos.x >= SIZE || relPos.z < 0 || relPos.z >= SIZE;
    }

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

    private PaperSqaure GetPaperAt(Vector3Int relPos)
    {
        return paperSquares[relPos.x, relPos.z];
    }

    private void SetPaperAt(Vector3Int relPos, PaperSqaure square)
    {
        paperSquares[relPos.x, relPos.z] = square;
    }

    private void RemoveReference(Vector3Int relPos)
    {
        SetPaperAt(relPos, null);
    }
}
