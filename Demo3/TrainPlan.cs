using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo3
{
    public class TrainPlan : IComparer<TrainPlan>
    {
        public DateTime PlanningTime;
        public string PlanTrainNum;
        public int Compare(TrainPlan x, TrainPlan y)
        {
            return x.PlanningTime.CompareTo(y.PlanningTime);
        }
    }
    public class TrainPlanSet
    {
        public class TrainPlanPair : IComparable<TrainPlanPair>
        {
            public DateTime start;
            public DateTime end;
            public TrainPlan Plan;
            public Train train;
            public bool IsPicked;
            public TrainPlanPair(DateTime start, DateTime end, TrainPlan plan,Train train)
            {
                this.start = start;
                this.end = end;
                Plan = plan;
                this.train = train;
            }

            public int CompareTo(TrainPlanPair other)
            {
                return start.CompareTo(other.start);
            }
        }

        public SortedSet<TrainPlanPair> set = new SortedSet<TrainPlanPair>();
        public void Add(DateTime start, DateTime end, TrainPlan plan,Train train)
        {
            set.Add(new TrainPlanPair(start, end, plan,train));
        }
        public TrainPlanPair Find(DateTime time)
        {
            int left = 0, right = set.Count;
            while (left < right)
            {
                int mid = left + (right - left) / 2;
                if (set.ElementAt(mid).start.CompareTo(time) <= 0)
                    left = mid + 1;
                else
                    right = mid;
            }
            if (left == 0) return null;
            var item= set.ElementAt(left - 1);
            return item.end >=time ? item : null;
        }
        public TrainPlanPair Top()
        {
            if (set.Count == 0) return null;
            return set.First();
        }
        public TrainPlanPair Pop()
        {
            var top = set.First();
            set.Remove(top);
            return top;
        }
        public void Clear()
        {
            set.Clear();
        }
        public void Pop(DateTime time)
        {
            while (set.Count > 0)
            {
                var top = set.First();
                if(top.Plan.PlanningTime <= time)
                {
                    set.Remove(top);
                }
                else
                {
                    break;
                }
            }
        }
        public void FindAndRemove(DateTime time)
        {
            var rmvItem = Find(time);
            if (rmvItem != null) rmvItem.IsPicked=true;
        }
        public TrainPlanPair First(DateTime earArr)
        {
            foreach (var item in set)
            {
                if (!item.IsPicked && item.Plan.PlanningTime>=earArr) return item;
            }
            return null;
        }
        public bool Check(DateTime start, DateTime end)
        {
            var nearest = Find(end);
            if (nearest == null || nearest.end <= start)
                return true;
            return false;
        }
    }
}
