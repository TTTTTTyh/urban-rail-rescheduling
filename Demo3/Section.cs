using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo3
{
    public class Section
    {
        public Station FromStn;
        public Station ToStn;
        public TimeSpan runningTime;
        Order _speedLimitOrder;
        TimeSpan _limitedRunningTime;
        public TimeSpan GetRunningTime(DateTime depTime)
        {
            if (_speedLimitOrder!=null && depTime >= _speedLimitOrder.StartTime && depTime < _speedLimitOrder.EndTime)
                return _limitedRunningTime;
            return runningTime;
        }

        Order _blockage;
        public bool WillEntryBlockage(DateTime depTime, TimeSpan runningTime, out DateTime earDep)
        {
            if (_blockage != null)
                return _blockage.WillEntryOrder(depTime, runningTime, out earDep);
            earDep = depTime;
            return false;
        }
    }
}
