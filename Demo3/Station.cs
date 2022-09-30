#define Console
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Demo3.DataManager;
using static Demo3.SegmentTree;

namespace Demo3
{
    public abstract class Station
    {
        protected static int cnt = 0;
        public string StnName;
        public double StnPriority;
        public static TimeSpan _delayThreshold = new TimeSpan(0, 6, 0);

        public string dir;
        public Section _nextSec;
        public Section _indepotLine;
        public Section _outDepotLine;

        public TimeSpan _normalHeadway = new TimeSpan(0, 5, 0);
        public TimeSpan _minHeadway = new TimeSpan(0, 2, 30);
        public TimeSpan _minWait = new TimeSpan(0, 0, 20);
        public TimeSpan _clearWait = new TimeSpan(0, 1, 0);

        public PriorityQueue<TrainEventNode> _arrivedTrain = new PriorityQueue<TrainEventNode>((a, b) =>
        {
            return a.ArrTime.CompareTo(b.ArrTime);
        });
        public void RemoveL(DateTime time)
        {
            while (_arrivedTrain.Count > 0 && _arrivedTrain.Peek().ArrTime > time)
            {
#if Console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0} 因回溯重置： ", _arrivedTrain.Peek().train.TrainNum);
                Console.ResetColor();
#endif
                _arrivedTrain.Dequeue().RemoveEvent();
            }
        }
        public Order _blockage;

        public TrainPlanSet _planArrSet = new TrainPlanSet();
        public TrainPlanSet _planDepSet = new TrainPlanSet();

        public abstract void Handle(TrainEventNode train);
        TimeSpan _overStock = new TimeSpan(0, 1, 0);
        //        protected internal bool CheckHeadway(TrainEventNode train)
        //        {
        //            if (_arrivedTrain.Count == 0 || (_arrivedTrain.Count > 0 && _arrivedTrain.Peek().train == train.train)) return true;
        //            TimeSpan headway = _normalHeadway;
        //            if(train.train.OrgArrs.ContainsKey(this) && train.train.OrgArrs[this] < train.ArrTime)
        //            {
        //                headway = _minHeadway;
        //            }
        //            if (train.train.TrainNum == "1030" && StnName == "坪山广场站")
        //            {
        //                int a = 1;
        //            }
        //            DateTime earArr = _arrivedTrain.Peek().DepTime + headway;
        //            if (train.ArrTime < earArr)//到早了，还在打挤
        //            {
        //#if Console
        //                Console.ForegroundColor = ConsoleColor.Red;
        //                Console.Write("{0} 因间隔时间重置: ", train.train.TrainNum);
        //                Console.ResetColor();
        //#endif
        //                if (train.Parent != null)//不是直接从段里面出来的
        //                {
        //                    if (earArr - train.ArrTime > _normalHeadway - _minHeadway)
        //                        train.Parent.DepTime += _arrivedTrain.Peek().DepTime + _minHeadway - train.ArrTime;
        //                    else
        //                        train.Parent.DepTime += earArr - train.ArrTime;
        //                    train.RemoveEvent();
        //                }
        //                else
        //                {
        //                    if (earArr - train.ArrTime > _normalHeadway - _minHeadway)
        //                        train.ArrTime += _arrivedTrain.Peek().DepTime + _minHeadway - train.ArrTime;
        //                    else
        //                        train.ArrTime += earArr - train.ArrTime;
        //                    GlobalWatcher.pq.Enqueue(train);
        //                }
        //                return false;
        //            }
        //            return true;
        //        }
        protected internal bool CheckHeadway(TrainEventNode train)
        {
            if (_arrivedTrain.Count == 0 || (_arrivedTrain.Count > 0 && _arrivedTrain.Peek().train == train.train)) return true;
            DateTime earArr = _arrivedTrain.Peek().DepTime + _minHeadway;
            if (train.ArrTime < earArr)//到早了，还在打挤
            {
#if Console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0} 因间隔时间重置: ", train.train.TrainNum);
                Console.ResetColor();
#endif
                if (train.Parent != null)//不是直接从段里面出来的
                {
                    train.Parent.DepTime += earArr - train.ArrTime;
                    train.RemoveEvent();
                }
                else
                {
                    train.ArrTime += earArr - train.ArrTime;
                    GlobalWatcher.pq.Enqueue(train);
                }
                return false;
            }
            return true;
        }

        protected void ToDepot(TrainEventNode train, DateTime depTarget)
        {
            var runningTime = _indepotLine.GetRunningTime(depTarget);
            if (_indepotLine.WillEntryBlockage(depTarget, runningTime, out DateTime earDep))//发早了，入段还在封锁
            {
                depTarget = earDep;
            }
            train.DepTime = depTarget;
            _arrivedTrain.Enqueue(train);
            train.Set();
            //var pair = _planDepSet.Find(depTarget);
            //train.TargetPlan = pair;
            train.Child = new TrainEventNode(_indepotLine.ToStn, depTarget + runningTime, train.train, train);
            (_indepotLine.ToStn as Depot).Add(train.Child.ArrTime, 1);
#if Console
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("{0} 入段", train.train);
            Console.ResetColor();
#endif
        }
        TimeSpan _delayThreshold2 = new TimeSpan(0, 1, 0);
        protected bool HandleArr(TrainEventNode train, out DateTime depTarget)
        {
#if Console
            if (_arrivedTrain.Count > 0 && _arrivedTrain.Peek().ArrTime > train.ArrTime)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} 开始回溯： ", train.train.TrainNum);
                Console.ResetColor();
            }
#endif
            RemoveL(train.ArrTime);
            depTarget = DateTime.MinValue;
            if (!CheckHeadway(train))
                return false;
            if (_blockage != null && _blockage.StartTime <= train.ArrTime && _blockage.EndTime > train.ArrTime)
            {
#if Console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0} 因封锁重置: ", train.train.TrainNum);
                Console.ResetColor();
#endif
                train.Parent.DepTime += _blockage.EndTime - train.ArrTime;
                train.RemoveEvent();
                return false;
            }
            if (train.train.CheckCross(this))//跳停
                depTarget = train.ArrTime;
            else if (train.train.ClearStation == this)//本站清客
                depTarget = train.ArrTime + _clearWait;
            else
            {
                DateTime earArr = _arrivedTrain.Count > 0 ? _arrivedTrain.Peek().DepTime + _minHeadway : DateTime.MinValue;
                var firstTime = _planDepSet.First(earArr);
                if (firstTime != null && _outDepotLine != null && train.ArrTime + _minWait - firstTime.Plan.PlanningTime >= _delayThreshold)//晚点有点多，召唤一个车过来
                {
                    var depot = _outDepotLine.FromStn as Depot;
                    DateTime time0 = firstTime.Plan.PlanningTime - _outDepotLine.runningTime;
                    DateTime end = time0 + _delayThreshold / 2;
                    for (; time0 <= end; time0 = time0.AddSeconds(1))
                    {
                        if (depot.CheckRS(time0))
                        {
                            firstTime.IsPicked = true;
                            Train newTrain = new Train(); newTrain.TrainNum = firstTime.Plan.PlanTrainNum + "临客";
                            newTrain.RollingStockNum = DataManager.nowRollingStockNum++;
                            newTrain.OrderNum = 1;
                            Dic4RollingStock.Add(newTrain.RollingStockNum, 1);
                            Trains.Add(newTrain);
                            TrainEventNode depotArr = new TrainEventNode(this, time0 + _outDepotLine.runningTime, newTrain, null);
                            while (_arrivedTrain.Count > 0 && _arrivedTrain.Peek() >= depotArr)
                            {
#if Console
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("{0} 出段重置：  ", newTrain.TrainNum);
                                Console.ResetColor();
#endif
                                _arrivedTrain.Dequeue().RemoveEvent();
                            }
                            GlobalWatcher.pq.Enqueue(depotArr);
                            depotArr._depot = depot;
                            depotArr.time0 = time0;
                            depot.Add(time0, -1);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0} {1} 出车", depot.StnName, time0);
                            Console.ResetColor();
                            GlobalWatcher.pq.Enqueue(train);
                            return false;
                        }
                    }
                }
                DateTime time1 = train.ArrTime + _minWait;
                if (train.train.OrgDeps != null && train.train.OrgDeps.ContainsKey(this))
                {
                    DateTime time0 = train.train.OrgDeps[this];
                    depTarget = (time0 > time1 && time0 - time1 < _delayThreshold2) ? time0 : time1;
                    //depTarget = time0 > time1 ? time0 : time1;
                }
                else
                {
                    var nearestTime = _planDepSet.Find(time1);
                    Debug.Assert(nearestTime == null || nearestTime.Plan.PlanningTime >= time1);
                    if (nearestTime != null)
                    {
                        depTarget = nearestTime.Plan.PlanningTime;
                    }
                    else depTarget = time1;
                }
            }
            depTarget = depTarget > train.DepTime ? depTarget : train.DepTime;

            return true;
        }
        protected internal void HandleDep(TrainEventNode train, DateTime depTarget)
        {
            if (_indepotLine != null && _indepotLine.ToStn == train.train.TargetStation)//入段
            {
                ToDepot(train, depTarget);
            }
            else if (this is TurnBackStn _stn && _stn._nextSec == null)//终点站，判断是否折返或者入段
            {
                _stn.TurnbackDep(train, depTarget);
            }
            else
            {
                var runningTime = _nextSec.GetRunningTime(depTarget);
                if (_blockage != null && _blockage.WillEntryOrder(depTarget, runningTime, out DateTime earDep))//发早了，还在封锁
                {
                    depTarget = earDep;
                }
                train.DepTime = depTarget;
                _arrivedTrain.Enqueue(train);
                train.Set();
                //var pair = _planDepSet.Find(depTarget);
                //train.TargetPlan = pair;
                train.Child = new TrainEventNode(_nextSec.ToStn, depTarget + runningTime, train.train, train);
                GlobalWatcher.pq.Enqueue(train.Child);
            }
        }
    }
    /// <summary>
    /// 没有折返线的车站
    /// </summary>
    internal class NormalStn : Station
    {
        public override void Handle(TrainEventNode train)
        {
            if (!HandleArr(train, out DateTime depTarget))
            {
                return;
            }
            HandleDep(train, depTarget);
        }
    }
    internal class TurnBackStn : Station
    {
        TimeSpan _minTurnBack = new TimeSpan(0, 0, 30);
        public TurnBackLine _turnBackLine;
        public Order _smallCrossRoad;
        public Order _crossAndTurn;
        public Station _antiStation;
        public Order _preTurnBack;
        protected internal void TurnbackDep(TrainEventNode train, DateTime depTarget)
        {
            if (_nextSec == null)
            {
                var runningTime = _minTurnBack;
                if (_blockage != null && _blockage.WillEntryOrder(depTarget, runningTime, out DateTime earDep)) //发早了，折返道岔故障还没解决
                {
                    depTarget = earDep;
                }
                var earArr = depTarget + runningTime + _turnBackLine._minTurnBack;
                var nearest = _antiStation._planArrSet.Find(earArr);
                if ((nearest != null && !nearest.IsPicked) || _indepotLine == null)
                {
                    Train newTrain = new Train()
                    {
                        TrainNum = nearest != null ? nearest.Plan.PlanTrainNum : "折返入段" + cnt++,
                        RollingStockNum = train.train.RollingStockNum,
                        OrderNum = train.train.OrderNum + 1
                    };
                    train.DepTime = depTarget;
                    if (nearest != null)
                    {
                        newTrain.OrgArrs = nearest.train.OrgArrs;
                        newTrain.OrgDeps = nearest.train.OrgDeps;
                        nearest.IsPicked = true;
                    }
                    train.Set();
                    _arrivedTrain.Enqueue(train);
                    DataManager.Dic4RollingStock[newTrain.RollingStockNum] += 1;
                    train.train.NextTrain = newTrain;
                    DataManager.Trains.Add(newTrain);
                    train.Child = new TrainEventNode(_turnBackLine, train.DepTime + _minTurnBack, newTrain, train);
                    train.Child.TargetPlan = nearest;
                    GlobalWatcher.pq.Enqueue(train.Child);
                }
                else
                {
                    ToDepot(train, depTarget);
                }
            }
            else
            {
                var runningTime = _nextSec.GetRunningTime(depTarget);
                if (_smallCrossRoad != null && _smallCrossRoad.StartTime <= depTarget && _smallCrossRoad.EndTime >= depTarget)//执行小交路
                {
                    //var pair = _planDepSet.Find(depTarget);
                    //train.TargetPlan = pair;
                    train.Set();
                    _arrivedTrain.Enqueue(train);
                    Train newTrain = new Train()
                    {
                        TrainNum = train.train.TrainNum,
                        RollingStockNum = train.train.RollingStockNum,
                        OrderNum = train.train.OrderNum + 1
                    };
                    train.train.NextTrain = newTrain;
                    DataManager.Dic4RollingStock[newTrain.RollingStockNum] += 1;
                    DataManager.Trains.Add(newTrain);
                    train.Child = new TrainEventNode(_turnBackLine, depTarget + _minTurnBack, newTrain, train);
                    GlobalWatcher.pq.Enqueue(train.Child);
                }
                else
                {
                    if (_blockage != null && _blockage.WillEntryOrder(depTarget, runningTime, out DateTime earDep))//发早了，车站还在封锁
                    {
                        depTarget = earDep;
                    }
                    //var pair = _planDepSet.Find(depTarget);
                    //train.TargetPlan = pair;
                    train.Set();
                    _arrivedTrain.Enqueue(train);
                    train.Child = new TrainEventNode(_nextSec.ToStn, depTarget + runningTime, train.train, train);
                    GlobalWatcher.pq.Enqueue(train.Child);
                }
            }
        }
        public override void Handle(TrainEventNode train)
        {
            if (_preTurnBack != null && train.ArrTime >= _preTurnBack.StartTime && train.ArrTime < _preTurnBack.EndTime)
            {
#if Console
                if (_arrivedTrain.Count > 0 && _arrivedTrain.Peek().ArrTime > train.ArrTime)
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} 开始回溯： ", train.train.TrainNum);
                    Console.ResetColor();
                }
#endif
                RemoveL(train.ArrTime);
                if (!_antiStation.CheckHeadway(train)) return;
                train.Stn = _antiStation;
                DateTime depTarget = train.ArrTime + _antiStation._clearWait;
                _antiStation.HandleDep(train, depTarget);
            }
            else
            {
                if (!HandleArr(train, out DateTime depTarget))
                {
                    return;
                }
                HandleDep(train, depTarget);
            }
        }
    }
    internal class TurnBackLine : Station
    {
        public TimeSpan _minTurnBack = new TimeSpan(0, 0, 30);
        public override void Handle(TrainEventNode train)
        {
            if (!HandleArr(train, out DateTime depTarget))
            {
                return;
            }
            var runningTime = _nextSec.GetRunningTime(depTarget);
            if (_blockage != null && _blockage.WillEntryOrder(depTarget, runningTime, out DateTime earDep))//发早了，还在封锁
            {
                depTarget = earDep;
            }
            if(train.TargetPlan.Plan.PlanningTime - runningTime > depTarget)
            {
                depTarget = train.TargetPlan.Plan.PlanningTime - runningTime;
            }
            train.DepTime = depTarget;
            train.Set();
            _arrivedTrain.Enqueue(train);
            train.Child = new TrainEventNode(_nextSec.ToStn, depTarget + runningTime, train.train, train);
            GlobalWatcher.pq.Enqueue(train.Child);
        }
    }
    public class Depot : Station
    {
        SegmentTree rsNum = new SegmentTree();
        public void Init(int rollingStockNum, DateTime start, DateTime end)
        {
            rsNum.SetStartEnd(start, end);
            rsNum.Insert(start, end, rollingStockNum);
        }
        public bool CheckRS(DateTime time)
        {
            return rsNum.Find(time) > 0;
        }
        public void Add(DateTime time, int val)
        {
            rsNum.Insert2End(time, val);
        }
        public override void Handle(TrainEventNode train)
        {
            throw new NotImplementedException();
        }
    }
    class SegmentTree
    {
        public class TreeNode
        {
            public TreeNode left;
            public TreeNode right;
            public int val;
            public int add;
        }
        TreeNode _root;
        public SegmentTree()
        {
            _root = new TreeNode();
        }
        DateTime start;
        DateTime end;
        int _start, _end;
        public void SetStartEnd(DateTime start, DateTime end)
        {
            this.start = start; this.end = end;
            _start = 0; _end = (int)(end - start).TotalSeconds;
        }
        public void Insert(DateTime left, DateTime right, int val)
        {
            Insert(_root, _start, _end, (int)(left - start).TotalSeconds, (int)(right - start).TotalSeconds, val);
        }
        public void Insert2End(DateTime time, int val)
        {
            Insert(_root, _start, _end, (int)(time - start).TotalSeconds, (int)(end - start).TotalSeconds, val);
        }
        public int Find(DateTime target)
        {
            return Find(_root, _start, _end, (int)(target - start).TotalSeconds, (int)(end - start).TotalSeconds);
        }
        void Insert(TreeNode node, int start, int end, int left, int right, int val)
        {
            if (start >= left && end <= right)
            {
                node.val += val;
                node.add += val;
                return;
            }
            int mid = start + (end - start) / 2;
            PushDown(node);
            if (left <= mid) Insert(node.left, start, mid, left, right, val);
            if (right > mid) Insert(node.right, mid + 1, end, left, right, val);
            PushUp(node);
        }
        int Find(TreeNode node, int start, int end, int left, int right)
        {
            if (start >= left && end <= right)
            {
                return node.val;
            }
            PushDown(node);
            int mid = start + (end - start) / 2;
            int res = int.MaxValue;
            if (left <= mid)
            {
                res = Find(node.left, start, mid, left, right);
            }
            if (right > mid)
            {
                res = Math.Min(res, Find(node.right, mid + 1, end, left, right));
            }
            return res;
        }
        void PushDown(TreeNode node)
        {
            if (node.left == null) node.left = new TreeNode();
            if (node.right == null) node.right = new TreeNode();
            if (node.add == 0) return;
            node.left.val += node.add;
            node.right.val += node.add;
            node.left.add += node.add;
            node.right.add += node.add;
            node.add = 0;
        }
        void PushUp(TreeNode node)
        {
            node.val = Math.Min(node.left.val, node.right.val);
        }
    }
}
