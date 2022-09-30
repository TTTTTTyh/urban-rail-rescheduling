using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABI.Windows.ApplicationModel.Activation;
using Newtonsoft.Json;
namespace Demo3
{

    internal static class Test
    {
        public static void TestMain()
        {
            Read(DateTime.Today + TimeSpan.FromHours(9));
            DataManager.Stns1[4]._blockage = new Order(DateTime.Today + TimeSpan.FromHours(9), DateTime.Today + TimeSpan.FromHours(9) + TimeSpan.FromMinutes(30));
            Action<Station> act = stn =>
            {
                stn._planArrSet.Pop(DateTime.Today + TimeSpan.FromHours(9));
                stn._planDepSet.Pop(DateTime.Today + TimeSpan.FromHours(9));
            };
            DataManager.Stns1.ForEach(act); DataManager.Stns2.ForEach(act);
            GlobalWatcher.Start();
            DataManager.SaveFileTest("SZ14-test");
        }
        public static void Read(DateTime nowTime)
        {
            string file1 = Environment.CurrentDirectory + @"\line.json";
            string file2 = Environment.CurrentDirectory + @"\trainPlan.json";
            using FileStream fs1 = new FileStream(file1, FileMode.Open);
            using StreamReader sr1 = new StreamReader(fs1);
            Rootobject root1 = JsonConvert.DeserializeObject<Rootobject>(sr1.ReadToEnd());
            Rootobject2 root2;
            using FileStream fs2 = new FileStream(file2, FileMode.Open);
            using StreamReader sr2 = new StreamReader(fs2);
            root2 = JsonConvert.DeserializeObject<Rootobject2>(sr2.ReadToEnd());
            int i = 0;
            DataManager.Stns1 = new List<Station>();
            Depot dep1 = new Depot(), dep2 = new Depot();dep1.StnName = "dep1";dep2.StnName = "dep2";
            dep1.Init(3, DateTime.Today.AddHours(9), DateTime.Today.AddHours(25));
            dep2.Init(3, DateTime.Today.AddHours(9), DateTime.Today.AddHours(25));
            foreach (var _stn in root1.stations)
            {
                Station stn;
                if (i == root1.stations.Length - 1)
                {
                    stn = new TurnBackStn();
                    var turnbackLine = new TurnBackLine();
                    turnbackLine.StnName = _stn.StnName + "折返线";
                    (stn as TurnBackStn)._turnBackLine = turnbackLine;
                    turnbackLine.StnPriority = i + 0.5;
                    turnbackLine._normalHeadway = new TimeSpan(0, 0, 10);
                }
                else
                {
                    stn = new NormalStn();
                }
                if (i == 0)
                {
                    stn._outDepotLine = new Section();
                    stn._outDepotLine.FromStn = dep1;
                    stn._outDepotLine.ToStn = stn;
                }
                stn.StnPriority = i;
                ++i;
                stn.StnName = _stn.StnName;
                DataManager.Stns1.Add(stn);
            }
            for (int ii = 0; ii < DataManager.Stns1.Count - 1; ++ii)
            {
                var sec = new Section();
                sec.FromStn = DataManager.Stns1[ii];
                sec.ToStn = DataManager.Stns1[ii + 1];
                DataManager.Stns1[ii]._nextSec = sec;
            }
            DataManager.Stns2 = new List<Station>();
            Dictionary<string, Station> stnDic1 = new Dictionary<string, Station>();
            Dictionary<string, Station> stnDic2 = new Dictionary<string, Station>();
            DataManager.Stns1.ForEach(stn => stnDic1.Add(stn.StnName, stn));
            i = 0;
            foreach (var _stn in root1.stations)
            {
                Station stn;
                if (i == 0)
                {
                    stn = new TurnBackStn();
                    var turnbackLine = new TurnBackLine();
                    turnbackLine.StnName = _stn.StnName + "折返线";
                    (stn as TurnBackStn)._turnBackLine = turnbackLine;
                    turnbackLine.StnPriority = (root1.stations.Length - 1 - i) + 0.5;
                    (stn as TurnBackStn)._antiStation = DataManager.Stns1[i];
                    //(DataManager.Stns1[i] as TurnBackStn)._antiStation = stn;
                    turnbackLine._normalHeadway = TimeSpan.FromSeconds(10);
                }
                else
                {
                    stn = new NormalStn();
                }
                if (i == root1.stations.Length-1)
                {
                    stn._outDepotLine = new Section();
                    stn._outDepotLine.FromStn = dep1;
                    stn._outDepotLine.ToStn = stn;
                }
                stn.StnPriority = (root1.stations.Length - 1 - i);
                stn.StnName = _stn.StnName;
                DataManager.Stns2.Add(stn);
                ++i;
            }
            DataManager.Stns2.Reverse();
            DataManager.Stns2.Last()._indepotLine = new Section();
            DataManager.Stns2.Last()._indepotLine.FromStn = DataManager.Stns2.Last();
            DataManager.Stns2.Last()._indepotLine.ToStn = dep2;
            DataManager.Stns1.Last()._indepotLine = new Section();
            DataManager.Stns1.Last()._indepotLine.FromStn = DataManager.Stns2.Last();
            DataManager.Stns1.Last()._indepotLine.ToStn = dep1;
            (DataManager.Stns1.Last() as TurnBackStn)._antiStation = DataManager.Stns2[0];
            DataManager.Stns2.ForEach(stn => stnDic2.Add(stn.StnName, stn));
            foreach (var _sec in root1.sections)
            {
                var sec = new Section();
                sec.FromStn = stnDic1[_sec.FromStation];
                sec.ToStn = stnDic1[_sec.ToStation];
                sec.runningTime = TimeSpan.FromSeconds(_sec.SectionRunningTime);
                sec.FromStn._nextSec = sec;
                var sec2 = new Section();
                sec2.FromStn = stnDic2[_sec.ToStation];
                sec2.ToStn = stnDic2[_sec.FromStation];
                sec2.runningTime = sec.runningTime;
                sec2.FromStn._nextSec = sec2;
            }
            foreach (var t in root2.trains)
            {
                Train trainTemp = new Train(); trainTemp.OrgArrs = new Dictionary<Station, DateTime>();
                trainTemp.OrgDeps = new Dictionary<Station, DateTime>();
                trainTemp.OrgArrs = new Dictionary<Station, DateTime>();
                if (t.ArrTime.First().Key == DataManager.Stns1[0].StnName)
                {
                    foreach (var arrStn in t.ArrTime.Keys)
                    {
                        var stn = stnDic1[arrStn];
                        TrainPlan p = new TrainPlan(); p.PlanTrainNum = t.Name;
                        p.PlanningTime = DateTime.Today + TimeSpan.FromSeconds(t.ArrTime[arrStn]);
                        trainTemp.OrgArrs.Add(stn, p.PlanningTime);
                        stn._planArrSet.Add(p.PlanningTime.AddMinutes(-7), p.PlanningTime, p, trainTemp);
                    }
                }
                else
                {
                    foreach (var arrStn in t.ArrTime.Keys)
                    {
                        var stn = stnDic2[arrStn];
                        TrainPlan p = new TrainPlan(); p.PlanTrainNum = t.Name;
                        p.PlanningTime = DateTime.Today + TimeSpan.FromSeconds(t.ArrTime[arrStn]);
                        trainTemp.OrgArrs.Add(stn, p.PlanningTime);
                        stn._planArrSet.Add(p.PlanningTime.AddMinutes(-7), p.PlanningTime, p, trainTemp);
                    }
                }
                if (t.DepTime.First().Key == DataManager.Stns1[0].StnName)
                {
                    foreach (var arrStn in t.DepTime.Keys)
                    {
                        var stn = stnDic1[arrStn];
                        TrainPlan p = new TrainPlan(); p.PlanTrainNum = t.Name;
                        p.PlanningTime = DateTime.Today + TimeSpan.FromSeconds(t.DepTime[arrStn]);
                        trainTemp.OrgDeps.Add(stn, p.PlanningTime);
                        stn._planDepSet.Add(p.PlanningTime.AddMinutes(-4), p.PlanningTime, p, trainTemp);
                    }
                }
                else
                {
                    foreach (var arrStn in t.ArrTime.Keys)
                    {
                        var stn = stnDic2[arrStn];
                        TrainPlan p = new TrainPlan(); p.PlanTrainNum = t.Name;
                        p.PlanningTime = DateTime.Today + TimeSpan.FromSeconds(t.DepTime[arrStn]);
                        trainTemp.OrgDeps.Add(stn, p.PlanningTime);
                        stn._planDepSet.Add(p.PlanningTime.AddMinutes(-4), p.PlanningTime, p, trainTemp);
                    }
                }
            }
            foreach (var t in root2.trains)
            {
                var start = t.ArrTime.First().Value;
                var end = t.DepTime.Last().Value;
                var startTime = DateTime.Today + TimeSpan.FromSeconds(start);
                var endTime = DateTime.Today + TimeSpan.FromSeconds(end);
                if (startTime <= nowTime && nowTime <= endTime)
                {
                    Train tt = new Train(); tt.TrainNum = t.Name;
                    var dic = t.ArrTime.First().Key == DataManager.Stns1[0].StnName ? stnDic1 : stnDic2;
                    foreach (var stn in t.ArrTime.Keys)
                    {
                        tt.OrgArrs.Add(dic[stn], DateTime.Today + TimeSpan.FromSeconds(t.ArrTime[stn]));
                        tt.OrgDeps.Add(dic[stn], DateTime.Today + TimeSpan.FromSeconds(t.DepTime[stn]));
                        if (tt.OrgArrs[dic[stn]] <= nowTime && tt.OrgDeps[dic[stn]] >= nowTime)
                        {
                            TrainEventNode node = new TrainEventNode(dic[stn], tt.OrgArrs[dic[stn]], tt, null);
                            GlobalWatcher.pq.Enqueue(node);
                        }
                        var nowStn = dic[stn];
                        if (nowStn._nextSec != null)
                        {
                            var arrTime = DateTime.Today + TimeSpan.FromSeconds(t.ArrTime[nowStn._nextSec.ToStn.StnName]);
                            if (tt.OrgDeps[dic[stn]] <= nowTime && arrTime >= nowTime)
                            {
                                TrainEventNode node = new TrainEventNode(dic[nowStn._nextSec.ToStn.StnName], arrTime, tt, null);
                                GlobalWatcher.pq.Enqueue(node);

                            }
                        }
                    }
                    DataManager.OnlineTrains.Add(tt);
                    tt.RollingStockNum = DataManager.nowRollingStockNum++;
                    tt.OrderNum = 1;
                    DataManager.Dic4RollingStock.Add(tt.RollingStockNum, 1);
                    DataManager.Trains.Add(tt);
                }
            }
            foreach (var stn in DataManager.Stns1)
            {
                stn.dir = "下行";
                stn._planArrSet.First(DateTime.MinValue).start = DateTime.MinValue;
                stn._planDepSet.First(DateTime.MinValue).start = DateTime.MinValue;
                if (stn is TurnBackStn _stn)
                {
                    var sec = new Section();
                    sec.FromStn = _stn._turnBackLine;
                    sec.ToStn = _stn._antiStation;
                    _stn._turnBackLine._nextSec = sec;

                }
            }
            foreach (var stn in DataManager.Stns2)
            {
                stn.dir = "上行";
                stn._planArrSet.First(DateTime.MinValue).start = DateTime.MinValue;
                stn._planDepSet.First(DateTime.MinValue).start = DateTime.MinValue;
                if (stn is TurnBackStn _stn)
                {
                    var sec = new Section();
                    sec.FromStn = _stn._turnBackLine;
                    sec.ToStn = _stn._antiStation;
                    _stn._turnBackLine._nextSec = sec;

                }
            }

        }

        public static Dictionary<string, int> _upDic = new Dictionary<string, int>()
        {
            { "沙田站",53},
            { "坑梓站",52},
            { "坪山中心站",46},
            { "坪山广场站",45},
            { "坪山围站",40},
            { "锦龙站",39},
            { "宝龙站",34},
            { "南约站",33},
            { "嶂背站",32},
            { "大运站",29},
            {"坳背站",28 },
            {"四联站" ,21},
            {"六约北站",20 },
            {"石芽岭站", 60},
            {"布吉站",16 },
            {"罗湖北站" ,14},
            {"黄木岗站",6 },
            {"岗厦北站",2 }
        };// 岗厦北 → 沙田
        public static Dictionary<string, int> _downDic = new Dictionary<string, int>()
        {
            { "沙田站",57},
            { "坑梓站",56},
            { "坪山中心站",49},
            { "坪山广场站",48},
            { "坪山围站",43},
            { "锦龙站",42},
            { "宝龙站",38},
            { "南约站",36},
            { "嶂背站",35},
            { "大运站",31},
            {"坳背站",30 },
            {"四联站" ,25},
            {"六约北站",24 },
            {"石芽岭站", 62},
            {"布吉站",19 },
            {"罗湖北站" ,18},
            {"黄木岗站",12 },
            {"岗厦北站",8 }
        };// 沙田 → 岗厦北
    }
    public class Rootobject
    {
        public StationJson[] stations { get; set; }
        public SectionJson[] sections { get; set; }
    }

    public class StationJson
    {
        public string StnName { get; set; }
        public int HeadWay { get; set; }
        public int StnID { get; set; }
        public int MinWait { get; set; }
        public int MaxWait { get; set; }
        public int MinTransTime { get; set; }
        public int MaxTransTime { get; set; }
    }

    public class SectionJson
    {
        public string FromStation { get; set; }
        public string ToStation { get; set; }
        public int SectionRunningTime { get; set; }
    }


    public class Rootobject2
    {
        public TrainJson[] trains { get; set; }
    }

    public class TrainJson
    {
        public Dictionary<string, int> ArrTime { get; set; }
        public Dictionary<string, int> DepTime { get; set; }
        public string Name { get; set; }
    }

}
