using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

            // Debug.Log($"... new entry at { keyTuple }");

            if (map.ContainsKey(keyTuple))
            {
                // Debug.Log("... ... overwriting an old value");
                map[keyTuple] = value;
            } else
            {
                map.Add(keyTuple, value);
                // Debug.Log("... ... this is a new value");
            }
        }
    }

    public void Clear() => map.Clear();
    public bool ContainsKey(Vector3Int key) => map.ContainsKey((key.x, key.y, key.z));

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Containing { map.Count } values:");
        foreach(var (k, v) in map)
        {
            sb.AppendLine(v.ToString());
        }

        return sb.ToString();
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
    public Vector3Int center { get; private set; }

    Func<Matrix4x4> transformFactory;

    public static Func<Matrix4x4> WorldTransformFactory = delegate ()
    {
        return Matrix4x4.identity;
    };

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
            transformFactory = transformFactory
        };
    }

    public void UpdateSpace(Vector3Int upwards, Vector3Int center, Func<Matrix4x4> transformFactory)
    {
        this.upwards = upwards;
        this.center = center;
        this.transformFactory = transformFactory;
    }

    public static OcclusionQueue MakeOcclusionQueue(Vector3Int position, Func<Matrix4x4> transformFactory)
    {
        var tCenter = Vector3Int.RoundToInt(transformFactory().MultiplyPoint(position));

        var xmod = tCenter.x % 2;
        var ymod = tCenter.y % 2;
        var zmod = tCenter.z % 2;

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
                center = position,
                transformFactory = transformFactory
            };
        }

        return null;
    }

    public void UseAsGlobal()
    {
        foreach (var s in faceDown)
        {
            s.parentSquare.globalOcclusionQueue = this;
        }
        foreach (var s in faceUp)
        {
            s.parentSquare.globalOcclusionQueue = this;
        }
    }

    public void Enqueue(PaperSquare ps)
    {
        var mtrx = transformFactory();
        var centerRounded = Vector3Int.RoundToInt(mtrx.MultiplyPoint(ps.transform.position));

        if (!centerRounded.Equals(center)) { return; }
        var nA = mtrx.MultiplyVector(ps.topSide.transform.up);
        var nB = -nA;

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
    }

    public void Dequeue(PaperSquare ps)
    {
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
            throw new UnityException("queue already contains side");
        }

        chk.Add(s);

        // notify the current outer-most display
        if (q.Count > 0)
        {
            q.Last.Value.Visibility = SquareSide.SideVisiblity.none;
        }
        q.AddLast(s);

        q.Last.Value.Visibility = SquareSide.SideVisiblity.full;
    }

    // MAY REPLACE Q, ref keyword needed!
    private void DequeueSide(SquareSide s, HashSet<SquareSide> chk, ref LinkedList<SquareSide> q)
    {
        if (!chk.Contains(s))
        {
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
            q.Last.Value.Visibility = SquareSide.SideVisiblity.full;
        }
    }

    public void MergeToFrontAndDispose(OcclusionQueue other)
    {
        MergeSide(other.qFaceUp, ref qFaceUp);
        MergeSide(other.qFaceDown, ref qFaceDown);
        MergeChk(faceUp, other.faceUp);
        MergeChk(faceDown, other.faceDown);
    }

    public void MergeToBackAndDispose(OcclusionQueue other)
    {
        MergeSide(qFaceUp, ref other.qFaceUp);
        MergeSide(qFaceDown, ref other.qFaceDown);

        if (qFaceUp.Count > 0)
        {
            qFaceUp.Last.Value.Visibility = SquareSide.SideVisiblity.full;
        }
        
        if (qFaceDown.Count > 0)
        {
            qFaceDown.Last.Value.Visibility = SquareSide.SideVisiblity.full;
        }

        MergeChk(faceUp, other.faceUp);
        MergeChk(faceDown, other.faceDown);
    }

    private void MergeChk(HashSet<SquareSide> mine, HashSet<SquareSide> theirs)
    {
        mine.UnionWith(theirs);
    }

    private void MergeSide(LinkedList<SquareSide> comesFirst, ref LinkedList<SquareSide> comesSecond)
    {
        if (comesFirst.Count > 0)
        {
            comesFirst.Last.Value.Visibility = SquareSide.SideVisiblity.none;
        }

        // there's probably a better way to merge without new allocations
        foreach (var i in comesSecond)
        {
            comesFirst.AddLast(i);
        }

        comesSecond = comesFirst;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"... oq ({center} facing {upwards}): ");
        if (qFaceUp.Count > 0)
        {
            sb.AppendLine(string.Join(", ", qFaceUp));
        }
        if (qFaceDown.Count > 0)
        {
            sb.AppendLine(string.Join(", ", qFaceDown));
        }
        return sb.ToString();
    }
}