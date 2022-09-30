#define Console_Write
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sms;
using Windows.UI.WebUI;

namespace Demo3
{
    public class TrainEventNode
    {
        public TrainEventNode(Station stn, DateTime time0, Train t,TrainEventNode parent)
        {
            Stn = stn;
            ArrTime = time0;
            DepTime = DateTime.MinValue;
            train = t;
            Parent = parent;
        }
        public Station Stn;
        public Train train;
        public TrainPlanSet.TrainPlanPair TargetPlan { get; set; }
        public DateTime ArrTime;
        public DateTime DepTime;
        public TrainEventNode Parent;
        public TrainEventNode Child;
        public bool Completed;
        public Depot _depot;
        public DateTime time0;
        public bool IsDelay = false;
        public void Set() 
        {
            train.SetArr(Stn, ArrTime);
            train.SetDep(Stn, DepTime); Completed = true;
#if Console_Write
            Console.WriteLine("{0} {1} 到达 {3}{2} 站", train.TrainNum, ArrTime, Stn.StnName,Stn.dir);
            Console.WriteLine("{0} {1} 离开 {3}{2} 站", train.TrainNum, DepTime, Stn.StnName,Stn.dir);
#endif
            if (train.OrgDeps != null && train.OrgDeps.ContainsKey(Stn))
            {
                IsDelay = DepTime > train.OrgDeps[Stn];
            }
        }
        public void Handle() 
        {
            train.NowEvent = this;
            Stn.Handle(this);
        }
        public static bool operator <=(TrainEventNode left, TrainEventNode right)
        {
            if (left.ArrTime != right.ArrTime)
            {
                return left.ArrTime <= right.ArrTime;
            }
            else
            {
                return left.Stn.StnPriority <= right.Stn.StnPriority;
            }
        }
        public static bool operator <(TrainEventNode left, TrainEventNode right)
        {
            if (left.ArrTime != right.ArrTime)
            {
                return left.ArrTime < right.ArrTime;
            }
            else
            {
                return left.Stn.StnPriority < right.Stn.StnPriority;
            }
        }
        public static bool operator >(TrainEventNode left, TrainEventNode right)
        {
            return !(left < right);
        }
        public static bool operator >=(TrainEventNode left, TrainEventNode right)
        {
            return !(left <= right);
        }
        void RemoveChild()
        {
            if(Child != null)
            {
                Child.RemoveChild();
                Child = null;
            }
            Stn.RemoveL(ArrTime);
        }
        public void RemoveEvent()
        {
#if Console_Write
            Console.WriteLine("{0} 重置{2}{1} 站", train.TrainNum, Stn.StnName, Stn.dir);
#endif
            if (Parent != null)
            {
                if (GlobalWatcher.pq.IsInqueue(train) && Parent.train==train)
                {
                    GlobalWatcher.pq.Adjust(Parent);
                }
                else
                {
                    GlobalWatcher.pq.Enqueue(Parent);
                }
            }
            else
            {
                if (GlobalWatcher.pq.IsInqueue(train))
                {
                    GlobalWatcher.pq.Remove(train);
                }
            }
            RemoveChild();
            Completed = false;
            if (TargetPlan != null) TargetPlan.IsPicked = false;
            if (_depot != null)
            {
                _depot.Add(time0, 1);
            }
        }
    }
    internal static class GlobalWatcher
    {
        //public static PriorityQueue<TrainEventNode> pq = new PriorityQueue<TrainEventNode>((a, b) =>
        //{
        //    if (a <= b) return 1;
        //    return -1;
        //});
        public static EventPQ pq = new EventPQ();
        public static void Start()
        {
            //pq.RemainOne();
            while (pq.Count > 0)
            {
                if (pq.Peek().train.TrainNum == "1031" /*&& pq.Peek().Stn.StnName == "坪山广场站"*/)
                {
                    int a = 1;
                }
                //if (pq.Peek().ArrTime.Day==30)
                //{
                //    int a = 1;
                //}
                //if (pq.Peek().ArrTime.Hour>=10)
                //{
                //    int a = 1;
                //}
                pq.Dequeue().Handle();
                //DataManager.SaveFileTest("test");
                //if (i++ % 40 == 0)
                //{
                //    Console.WriteLine("=================={0}=====================",j++);
                //    //Console.ReadLine();
                //}
            }
        }
    }
}
