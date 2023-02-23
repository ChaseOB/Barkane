using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OcclusionMap : IEnumerable<KeyValuePair<Vector3Int, OcclusionQueue>>
{
    private Dictionary<(int, int, int), OcclusionQueue> map = new Dictionary<(int, int, int), OcclusionQueue>();

    public OcclusionQueue this[Vector3Int key] { 
        get
        {
            return map[(key.x, key.y, key.z)];
        } set
        {
            var keyTuple = (key.x, key.y, key.z);
            if (map.ContainsKey(keyTuple))
            {
                map[keyTuple] = value;
            } else
            {
                map.Add(keyTuple, value);
            }
        }
    }

    public void Clear() => map.Clear();
    public bool ContainsKey(Vector3Int key) => map.ContainsKey((key.x, key.y, key.z));

    public override string ToString()
    {
        return string.Join("\n", map.Values);
    }

    public IEnumerator<KeyValuePair<Vector3Int, OcclusionQueue>> GetEnumerator()
    {
        return new Enumerator(map);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(map);
    }

    public class Enumerator : IEnumerator<KeyValuePair<Vector3Int, OcclusionQueue>>
    {
        IEnumerator<KeyValuePair<(int, int, int), OcclusionQueue>> m_Enumerator;

        public Enumerator(Dictionary<(int, int, int), OcclusionQueue> src) {
            m_Enumerator = src.GetEnumerator();
        }

        KeyValuePair<Vector3Int, OcclusionQueue> IEnumerator<KeyValuePair<Vector3Int, OcclusionQueue>>.Current
        {
            get
            {
                var (k, v) = m_Enumerator.Current;
                return new KeyValuePair<Vector3Int, OcclusionQueue>(new Vector3Int(k.Item1, k.Item2, k.Item3), v);
            }
        }

        object IEnumerator.Current
        {
            get { return m_Enumerator.Current; }
        }

        public void Dispose()
        {
            m_Enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return m_Enumerator.MoveNext();
        }

        public void Reset()
        {
            m_Enumerator.Reset();
        }
    }
}

public class OcclusionQueue
{
    HashSet<SquareSide> faceUp = new HashSet<SquareSide>();
    LinkedList<SquareSide> qFaceUp = new LinkedList<SquareSide>();
    HashSet<SquareSide> faceDown = new HashSet<SquareSide>();
    LinkedList<SquareSide> qFaceDown = new LinkedList<SquareSide>();

    public Vector3Int upwards { get; private set; }
    public Vector3 center { get; private set; }

    public OcclusionQueue MakeFlippedCopy()
    {
        return new OcclusionQueue
        {
            upwards = -upwards,
            center = center,
            qFaceDown = qFaceUp,
            qFaceUp = qFaceDown,
            faceDown = faceUp,
            faceUp = faceDown,
        };
    }

    public static OcclusionQueue MakeOcclusionQueue(Vector3Int position)
    {
        var xmod = position.x % 2;
        var ymod = position.y % 2;
        var zmod = position.z % 2;

        var axSum = xmod + ymod + zmod;
        if (axSum != 1 && axSum != -1) { return null; }

        var upwards = Vector3Int.zero;
        if (xmod == 1 || xmod == -1)
        {
            upwards = Vector3Int.right;
        }
        else if (ymod == 1 || ymod == -1)
        {
            upwards = Vector3Int.up;
        }
        else if (zmod == 1 || zmod == -1)
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
        if (ps.globalOcclusionQueue != null)
        {
            throw new UnityException("Must clear occlusion queue by dequeing before enqueuing a different one");
        }

        var center = ps.transform.position;
        var centerRounded = Vector3Int.RoundToInt(center);

        if (!centerRounded.Equals(center)) { return; }

        var dist = (center - centerRounded).sqrMagnitude;
        var nA = ps.topSide.transform.up;
        var nB = ps.bottomSide.transform.up;

        Debug.Log($"Enqueue {center}");

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

        ps.globalOcclusionQueue = this;
    }

    public void Dequeue(PaperSquare ps)
    {
        ps.globalOcclusionQueue = null;

        // just try both lol
        DequeueSide(ps.topSide, faceUp, ref qFaceUp);
        DequeueSide(ps.topSide, faceDown, ref qFaceDown);
        DequeueSide(ps.bottomSide, faceUp, ref qFaceUp);
        DequeueSide(ps.bottomSide, faceDown, ref qFaceDown);
    }

    private void EnqueueSide(SquareSide s, HashSet<SquareSide> chk, LinkedList<SquareSide> q)
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
            q.Last.Value.SetVisibility(SquareSide.SideVisiblity.none);
        }
        q.AddLast(s);

        Debug.Log("Enqueue item to list");

        q.Last.Value.SetVisibility(SquareSide.SideVisiblity.full);
    }

    // MAY REPLACE Q, ref keyword needed!
    private void DequeueSide(SquareSide s, HashSet<SquareSide> chk, ref LinkedList<SquareSide> q)
    {
        if (!chk.Contains(s))
        {
            Debug.Log("queue does not contain side");
            return;
        }

        chk.Remove(s);

        // O(0) remove if tail
        if (q.Last.Value == s)
        {
            q.RemoveLast();
        }
        // O(n) remove if middle, for some reason C# doesn't support that on queues
        else
        {
            var qL = q.ToList();
            qL.Remove(s);
            q = new LinkedList<SquareSide>(qL);
        }
        // notify the next outer-most display
        if (q.Count > 0)
        {
            q.Last.Value.SetVisibility(SquareSide.SideVisiblity.full);
        }
    }

    public void MergeToFrontAndDispose(OcclusionQueue other)
    {

        MergeSide(other.qFaceUp, ref qFaceUp);
        MergeSide(other.qFaceDown, ref qFaceDown);

        other.qFaceUp.Clear();
        other.qFaceDown.Clear();

        MergeChk(faceUp, other.faceUp);
        MergeChk(faceDown, other.faceDown);

        other.faceDown.Clear();
        other.faceDown.Clear();
    }

    public void MergeToBackAndDispose(OcclusionQueue other)
    {
        MergeSide(qFaceUp, ref other.qFaceUp);
        MergeSide(qFaceDown, ref other.qFaceDown);

        other.qFaceUp.Clear();
        other.qFaceDown.Clear();

        MergeChk(faceUp, other.faceUp);
        MergeChk(faceDown, other.faceDown);

        other.faceDown.Clear();
        other.faceDown.Clear();
    }

    private void MergeChk(HashSet<SquareSide> mine, HashSet<SquareSide> theirs)
    {
        mine.UnionWith(theirs);
    }

    private void MergeSide(LinkedList<SquareSide> comesFirst, ref LinkedList<SquareSide> comesSecond)
    {
        comesFirst.Last.Value.SetVisibility(SquareSide.SideVisiblity.none);

        // there's probably a better way to merge without new allocations
        foreach (var i in comesSecond)
        {
            comesFirst.AddLast(i);
        }

        comesSecond = comesFirst;
    }

    public override string ToString()
    {
        return $"... oq ({center} facing {upwards}): " + 
            string.Join(", ", qFaceUp) + "\n" + string.Join(", ", faceUp) + "\n"
            + string.Join(", ", qFaceDown) + "\n" + string.Join(", ", faceDown) + "\n";
    }
}