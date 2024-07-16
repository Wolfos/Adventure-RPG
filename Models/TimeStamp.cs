using System;

namespace Models
{
    [Serializable]
    public struct TimeStamp
    {
        public int hour;
        public int minute;

        public TimeStamp(int hour, int minute)
        {
            this.hour = hour;
            this.minute = minute;
        }
        
        public static bool operator >(TimeStamp a, TimeStamp b)
        {
            var minuteA = ((float) a.minute) / 60;
            var minuteB = ((float) b.minute) / 60;
            return ((float)a.hour) + minuteA > ((float)b.hour) + minuteB;
        }
        
        public static bool operator <(TimeStamp a, TimeStamp b)
        {
            var minuteA = ((float) a.minute) / 60;
            var minuteB = ((float) b.minute) / 60;
            return ((float)a.hour) + minuteA < ((float)b.hour) + minuteB;
        }

        public static bool operator >(float a, TimeStamp b)
        {
            var minute = ((float) b.minute) / 60;
            return a > ((float)b.hour) + minute;
        }

        public static bool operator <(float a, TimeStamp b)
        {
            var minute = ((float) b.minute) / 60;
            return a < ((float)b.hour) + minute;
        }
    }
}