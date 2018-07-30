using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.PlatformAbstractions;
using MoreLinq;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling
{
    public class Segment : IComparable<Segment> 
    {
        
        public int CompareTo(Segment other)
        {
            return SegmentId.CompareTo(other.SegmentId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Segment) obj);
        }

        public override int GetHashCode()
        {
            var hash = HashCodeCombiner.Start();
            hash.Add(Mask);
            hash.Add(SegmentId);
            return hash.CombinedHash;
        }

        private const int ZeroMask = 0;
        public static readonly Segment RootSegment = new Segment(0, ZeroMask);
        public static readonly Segment[] EmptySegments = new Segment[0];
        
        public int SegmentId { get; }
        public int Mask { get; }

        Segment(int segmentId, int mask)
        {
            SegmentId = segmentId;
            Mask = mask;
        }

        private static bool ComputeSegments(Segment segment, List<int> segments, HashSet<Segment> applicableSegments)
        {
            var splitSegment = segment.Split();
            // As the first segmentId mask, keeps the original segmentId, we only check the 2nd segmentId mask being a know.
            if (segments.Contains(splitSegment[1].SegmentId)) 
            {
                foreach (Segment segmentSplit in splitSegment) 
                {
                    if (!ComputeSegments(segmentSplit, segments, applicableSegments)) 
                    {
                        applicableSegments.Add(segmentSplit);
                    }
                }
            } 
            else 
            {
                applicableSegments.Add(segment);
            }
            return true;
        }
        public static Segment[] ComputeSegments(params int[] segments) 
        {
            if (segments == null || segments.Length == 0)
            {
                return EmptySegments;
            }
            var resolvedSegments = new HashSet<Segment>();
            ComputeSegments(RootSegment, segments.ToList(), resolvedSegments);
 
            // As we split and compute segment masks branching by first entry, the resolved segment mask is not guaranteed
            // to be added to the collection in natural order.
            
            var segmentsList = resolvedSegments.ToList();
            segmentsList.Sort();
            return resolvedSegments.OrderBy(x => x).ToArray();
        }
        public static List<Segment> SplitBalanced(Segment segment, int numberOfTimes) 
        {
            var toBeSplit = new SortedSet<Segment>(Comparer<Segment>.Default.Comparing(x => x.Mask)
                .ThenComparing(segment1 => segment1.SegmentId));
            toBeSplit.Add(segment);
            for (int i = 0; i < numberOfTimes; i++) 
            {
                var workingSegment = toBeSplit.First();
                toBeSplit.Remove(workingSegment);
                workingSegment.Split().ForEach(x => toBeSplit.Add(x));
            }
            var result = toBeSplit.ToList().OrderBy(x => x.SegmentId).ToList();
            return result;
        }
        
        public Segment MergedWith(Segment other) 
        {
            Assert.IsTrue(this.IsMergeableWith(other), () => "Given Segment cannot be merged with this segment.");
            return new Segment(Math.Min(this.SegmentId, other.SegmentId), (int)((uint)Mask >> 1));
        }
        
        public int MergeableSegmentId()
        {
            int parentMask = (int)((uint)Mask >> 1);
            int firstBit = Mask ^ parentMask;

            return SegmentId ^ firstBit;
        }
        public bool IsMergeableWith(Segment other) 
        {
            return Mask == other.Mask
                   && MergeableSegmentId() == other.SegmentId;
        }
        public bool Matches(int value) 
        {
            return Mask == 0 || (Mask & value) == SegmentId;
        }
        public bool Matches(Object value) 
        {
            return Mask == 0 || Matches(value?.GetHashCode() ?? 0);
        }
        
        private Segment[] Split()
        {
            if ((Mask << 1) < 0) 
            {
                
                throw new ArgumentException("Unable to split the given segmentId, as the mask exceeds the max mask size.");
            }

            var segments = new Segment[2];
            int newMask = ((Mask << 1) + 1);

            int newSegment = SegmentId + (Mask == 0 ? 1 : newMask ^ Mask);
            segments[0] = new Segment(SegmentId, newMask);
            segments[1] = new Segment(newSegment, newMask);

            return segments;
        }

        public override string ToString()
        {
            return $"Segment[{SegmentId}/{Mask}]";
        }
    }
}