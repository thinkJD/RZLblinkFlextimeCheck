using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RzlBlinkFlextime
{
    class Flextime
    {
        private int totalMinutesPerDay;
        private int offset;
        private int daylyExtraTime;

        public Flextime(int TotalMinutesPerDay, int Offset, int DaylyExtraTime)
        {
            totalMinutesPerDay = TotalMinutesPerDay;
            offset = Offset;
            daylyExtraTime = DaylyExtraTime;
        }

        public bool TimeToGo(DateTime WorkStart)
        {
            TimeSpan Result = DateTime.Now - WorkStart;
            return (Result.TotalMinutes >= totalMinutesPerDay) ? true : false;
        }

        public bool ExtraTimeReached(DateTime WorkStart)
        {
            TimeSpan Result = DateTime.Now - WorkStart;
            return (Result.TotalMinutes >= (totalMinutesPerDay + daylyExtraTime)) ? true : false;
        }
    }
}
