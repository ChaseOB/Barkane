using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OcclusionQueue
{
    HashSet<SquareSide> faceUp = new HashSet<SquareSide>();
    Queue<SquareSide> qFaceUp = new Queue<SquareSide>();
    HashSet<SquareSide> faceDown = new HashSet<SquareSide>();
    Queue<SquareSide> qFaceDown = new Queue<SquareSide>();

    public Vector3Int upwards { get; private set; }
    public Vector3 center { get; private set; }

    public static OcclusionQueue MakeOcclusionQueue(Vector3Int position)
    {
        var xmod = position.x % 2;
        var ymod = position.y % 2;
        var zmod = position.z % 2;

        if (xmod + ymod + zmod != 1) { return null; }

        var upwards = Vector3Int.zero;
        if (xmod == 1)
        {
            upwards = Vector3Int.right;
        }
        else if (ymod == 1)
        {
            upwards = Vector3Int.up;
        }
        else if (zmod == 1)
        {
            upwards = Vector3Int.forward;
        }

        if (!upwards.Equals(Vector3.zero))
        {
            return new OcclusionQueue()
            {
                upwards = upwards,
                center = position
            };
        }

        return null;
    }

    public void Enqueue(PaperSquare ps)
    {
        if (ps.occlusionQueue != null)
        {
            throw new UnityException("Must clear occlusion queue by dequeing before enqueuing a different one");
        }

        var center = ps.transform.position;
        var centerRounded = Vector3Int.RoundToInt(center);

        if (!centerRounded.Equals(center)) { return; }

        var dist = (center - centerRounded).sqrMagnitude;
        var nA = ps.topSide.transform.up;
        var nB = ps.bottomSide.transform.up;

        if (Vector3.Dot(nA, upwards) > Vector3.Dot(nB, upwards))
        {
            EnqueueSide(ps.topSide, faceUp, qFaceUp);
            EnqueueSide(ps.bottomSide, faceDown, qFaceDown);
        }
        else
        {
            EnqueueSide(ps.bottomSide, faceUp, qFaceUp);
            EnqueueSide(ps.topSide, faceDown, qFaceDown);
        }

        ps.occlusionQueue = this;
    }

    public void Dequeue(PaperSquare ps, SquareSide.SideVisiblity afterDequeue)
    {
        ps.occlusionQueue = null;

        // just try both lol
        DequeueSide(ps.topSide, faceUp, ref qFaceUp);
        DequeueSide(ps.topSide, faceDown, ref qFaceDown);
        DequeueSide(ps.bottomSide, faceUp, ref qFaceUp);
        DequeueSide(ps.bottomSide, faceDown, ref qFaceDown);

        ps.topSide.SetVisibility(afterDequeue);
        ps.bottomSide.SetVisibility(afterDequeue);
    }

    private void EnqueueSide(SquareSide s, HashSet<SquareSide> chk, Queue<SquareSide> q)
    {
        if (chk.Contains(s))
        {
            Debug.Log("queue already contains side");
            return;
        }

        chk.Add(s);

        // notify the current outer-most display
        if (q.Count > 0)
        {
            q.Peek().SetVisibility(SquareSide.SideVisiblity.none);
        }
        q.Enqueue(s);

        q.Peek().SetVisibility(SquareSide.SideVisiblity.full);
    }

    // MAY REPLACE Q, ref keyword needed!
    private void DequeueSide(SquareSide s, HashSet<SquareSide> chk, ref Queue<SquareSide> q)
    {
        if (!chk.Contains(s))
        {
            Debug.Log("queue does not contain side");
            return;
        }

        chk.Remove(s);

        // O(0) remove if tail
        if (q.Peek() == s)
        {
            q.Dequeue();
        }
        // O(n) remove if middle, for some reason C# doesn't support that on queues
        else
        {
            var qL = q.ToList();
            qL.Remove(s);
            q = new Queue<SquareSide>(qL);
        }
        // notify the next outer-most display
        if (q.Count > 0)
        {
            q.Peek().SetVisibility(SquareSide.SideVisiblity.full);
        }
    }
}