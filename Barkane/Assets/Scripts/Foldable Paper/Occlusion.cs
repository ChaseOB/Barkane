using BarkaneJoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class OcclusionMap : IEnumerable<KeyValuePair<Vector3Int, OcclusionQueue>>
{
    private Dictionary<(int, int, int), OcclusionQueue> Map = new Dictionary<(int, int, int), OcclusionQueue>();

    public OcclusionQueue this[Vector3Int key] { 
        get => Map[(key.x, key.y, key.z)]; 
        set
        {
            var keyTuple = (key.x, key.y, key.z);
            if (Map.ContainsKey(keyTuple))
                Map[keyTuple] = value;
            else
                Map.Add(keyTuple, value);
        }
    }

    public void Clear() => Map.Clear();
    public bool ContainsKey(Vector3Int key) => Map.ContainsKey((key.x, key.y, key.z));

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Containing { Map.Count } values:");
        foreach(var (_, v) in Map) 
            sb.AppendLine(v.ToString());

        return sb.ToString();
    }

    public void Prune()
    {
        // https://stackoverflow.com/questions/469202/best-way-to-remove-multiple-items-matching-a-predicate-from-a-net-dictionary
        foreach (var (k, v) in Map.Where( kv => kv.Value.IsEmpty ).ToList())
            Map.Remove(k);
    }

    public IEnumerator<KeyValuePair<Vector3Int, OcclusionQueue>> GetEnumerator() 
        => new Enumerator(Map);

    IEnumerator IEnumerable.GetEnumerator() 
        => new Enumerator(Map);

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

        object IEnumerator.Current => m_Enumerator.Current;
        public void Dispose() => m_Enumerator.Dispose();
        public bool MoveNext() => m_Enumerator.MoveNext();

        public void Reset() => m_Enumerator.Reset();
    }
}

public class OcclusionQueue
{
    /// <summary>
    /// Right-handed basis of the occlusion queue's orientation relative to its encoder
    /// +Normal cross +Major = +Minor
    /// </summary>
    public enum SpatialOrientation
    {
        /// <summary>
        /// X normal, Y major, Z minor
        /// </summary>
        XYZ,
        /// <summary>
        /// Y normal, Z major, X minor
        /// </summary>
        YZX,
        /// <summary>
        /// Z nnormal, X major, Y minor
        /// </summary>
        ZXY
    }

    public SpatialOrientation Orientation
    {
        get => m_Orientation;
        set
        {
            m_Orientation = value;
            switch (value)
            {
                case SpatialOrientation.XYZ:
                    SpatialBasis = (
                        Vector3Int.right,
                        Vector3Int.up,
                        Vector3Int.forward);
                    break;
                case SpatialOrientation.YZX:
                    SpatialBasis = (
                        Vector3Int.up,
                        Vector3Int.forward,
                        Vector3Int.right);
                    break;
                case SpatialOrientation.ZXY:
                    SpatialBasis = (
                        Vector3Int.forward,
                        Vector3Int.right,
                        Vector3Int.up);
                    break;
            }
        }
    }

    public (Vector3Int normal, Vector3Int major, Vector3Int minor) SpatialBasis { get; private set; }

    private SpatialOrientation m_Orientation;

    LinkedList<SquareSide>
        qFaceUp = new(),
        qFaceDown = new();
    HashSet<SquareSide>
        faceDown = new(),
        faceUp = new();

    LinkedList<JointPieceOwnership>
        majorUp = new(),
        majorDown = new(),
        minorUp = new(),
        minorDown = new();

    public bool IsEmpty => faceUp.Count == 0 && faceDown.Count == 0;

    public Vector3Int Offset { get; private set; }

    Func<Matrix4x4> encoderFactory;

    public static Func<Matrix4x4> IdentityEncoder = delegate ()
    {
        return Matrix4x4.identity;
    };

    public OcclusionQueue MakeFlippedCopy() => new OcclusionQueue
    {
        Orientation = Orientation,
        Offset = Offset,
        qFaceDown = qFaceUp,
        qFaceUp = qFaceDown,
        faceDown = faceUp,
        faceUp = faceDown,
        encoderFactory = encoderFactory
    };

    public void UpdateSpace(Vector3Int normal, Vector3Int center, Func<Matrix4x4> transformFactory)
    {
        if (normal.x == 1)
        {
            Orientation = SpatialOrientation.XYZ;
        } else if (normal.y == 1)
        {
            Orientation = SpatialOrientation.YZX;
        } else if (normal.z == 1)
        {
            Orientation = SpatialOrientation.ZXY;
        } else
        {
            throw new UnityException($"Invalid normal {normal} does not match any spacial orientations");
        }
        this.Offset = center;
        this.encoderFactory = transformFactory;
    }

    public static OcclusionQueue MakeOcclusionQueue(Vector3Int position, Func<Matrix4x4> transformFactory)
    {
        var tCenter = Vector3Int.RoundToInt(transformFactory().MultiplyPoint(position));

        var xmod = tCenter.x % 2;
        var ymod = tCenter.y % 2;
        var zmod = tCenter.z % 2;

        var axSum = xmod + ymod + zmod;
        if (axSum != 1 && axSum != -1) { return null; }

        SpatialOrientation? orien = null;
        if (xmod == 1 || xmod == -1)
        {
            orien = SpatialOrientation.XYZ;
        }
        else if (ymod == 1 || ymod == -1)
        {
            orien = SpatialOrientation.YZX;
        }
        else if (zmod == 1 || zmod == -1)
        {
            orien = SpatialOrientation.ZXY;
        }

        if (!orien.HasValue)
        {
            return new OcclusionQueue()
            {
                Orientation = orien.Value,
                Offset = position,
                encoderFactory = transformFactory
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
        var mtrx = encoderFactory();
        var centerRounded = Vector3Int.RoundToInt(mtrx.MultiplyPoint(ps.transform.position));

        if (!centerRounded.Equals(Offset)) { return; }
        var nA = mtrx.MultiplyVector(ps.topSide.transform.up);
        var nB = -nA;

        if (Vector3.Dot(nA, SpatialBasis.normal) > Vector3.Dot(nB, SpatialBasis.normal))
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

    /// <summary>
    /// Merge such that...
    /// - the top queue of other is inserted in front of the top queue of this
    /// - the bottom queue of this is inserted in front of the bottom queue of other
    /// </summary>
    /// <param name="other"></param>
    public void MergeApproachingFromNegative(OcclusionQueue other)
    {
        MergeSide(other.qFaceUp, ref qFaceUp);
        MergeSide(qFaceDown, ref other.qFaceDown);
        MergeChk(faceUp, other.faceUp);
        MergeChk(faceDown, other.faceDown);
    }

    /// <summary>
    /// Merge such that...
    /// - the top queue of other is inserted behind the top queue of this
    /// - the bottom queue of this is inserted behind the bottom queue of other
    /// </summary>
    /// <param name="other"></param>
    public void MergeApproachingFromPositive(OcclusionQueue other)
    {
        MergeSide(qFaceUp, ref other.qFaceUp);
        MergeSide(other.qFaceDown, ref qFaceDown);
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

        if (comesFirst.Count > 0)
        {
            comesFirst.Last.Value.Visibility = SquareSide.SideVisiblity.full;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"... oq ({Offset} facing {SpatialBasis}): ");
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