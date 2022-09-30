#define Console_Write
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo3
{
    public class Train
    {
        public string TrainNum;
        public Dictionary<Station, DateTime> Arrs=new Dictionary<Station, DateTime>();
        public Dictionary<Station, DateTime> Deps=new Dictionary<Station, DateTime>();
        public Dictionary<Station, DateTime> OrgArrs = new Dictionary<Station, DateTime>();
        public Dictionary<Station, DateTime> OrgDeps = new Dictionary<Station, DateTime>();
        public int RollingStockNum;
        public int OrderNum;
        public int dir;//1 下行 0 上行
        public TrainEventNode NowEvent;
        public void SetArr(Station stn,DateTime time)
        {
            if (Arrs.ContainsKey(stn))
            {
                Arrs[stn] = time;
            }
            else
            {
                Arrs.Add(stn, time);
            }
        }
        public void SetDep(Station stn,DateTime time)
        {
            if (Deps.ContainsKey(stn))
            {
                Deps[stn] = time;
            }
            else
            {
                Deps.Add(stn, time);
            }
        }
        public Train NextTrain;
        public Station TargetStation;
        public Station ClearStation;
        public bool AnyDepot = false;
        public bool CheckCross(Station stn)
        {
            if(TargetStation !=null && TargetStation.StnPriority > stn.StnPriority)
            {
                return true;
            }
            return false;
        }
    }
}
