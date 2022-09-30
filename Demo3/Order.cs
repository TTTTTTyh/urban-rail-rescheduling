using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo3
{
    public class Order
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public bool WillEntryOrder(DateTime depTime, TimeSpan runningTime, out DateTime earDep)
        {
            if (StartTime <= depTime && EndTime > depTime)
            {
                earDep = EndTime;
                return true;
            }
            DateTime nextArr = depTime + runningTime;
            if (StartTime <= nextArr && EndTime > nextArr)
            {
                earDep = EndTime;
                return true;
            }
            earDep = depTime;
            return false;
        }
        public Order(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }

}
