using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static RPEFluentManager.Models.Easings;

//generated automatically

namespace RPEFluentManager.Models
{

    public class BPMListItem
    {
        public float bpm { get; set; }
        public Time startTime { get; set; }
    }
    public class META
    {
        public int RPEVersion { get; set; }
        public string background { get; set; }
        public string charter { get; set; }
        public string composer { get; set; }
        public string id { get; set; }
        public string level { get; set; }
        public string name { get; set; }
        public int offset { get; set; }
        public string song { get; set; }
    }
    public class AlphaControlItem
    {
        public AlphaControlItem(float xx)
        {
            alpha = 1.0f;
            easing = 1;
            x = xx;
        }
        public float alpha { get; set; }
        public int easing { get; set; }
        public float x { get; set; }
    }
    public class RPEEvent
    {
        public RPEEvent()
        {
            linkgroup = 0;
            easingLeft = 0;
            easingRight = 1;
            bezier = 0;
            bezierPoints = new BezierPoints() { 0.0f, 0.0f, 0.0f, 0.0f };
        }


        public int GetMontonicity()
        {
            if(end > start)
            {
                return 1;
            }
            else if(end < start)
            {
                return -1;
            }
            return 0;
        }
        public double GetDuration()
        {
            return endTime.GetTime() - startTime.GetTime();
        }
        public double GetVelocity()
        {
            return (end-start)/GetDuration();
        }



        public EventList Cut(double density)
        {
            double duration = 1.0 / density;
            double startTimeAsDouble = startTime;
            double endTimeAsDouble = endTime;
            double timeRange = endTimeAsDouble - startTimeAsDouble;
            float valueRange = end - start;
            double easingFuncValue;

            EventList cuttedEvents = new EventList();

            EasingFunc easingFunc = Easings.easeFuncs[easingType];

            int maxIndex = (int)Math.Ceiling(timeRange / duration) - 1;

            float lastValue = start;

            for (int i = 0; i <= maxIndex; i++)
            {
                double t0 = startTimeAsDouble + duration * i;
                double t1 = t0 + duration;

                RPEEvent newEvent = new RPEEvent();

                newEvent.startTime = (Time)t0;
                newEvent.endTime = (Time)t1;

                newEvent.start = lastValue;

                double elapsed = t1 - startTimeAsDouble;
                if (elapsed < 0) elapsed = 0;
                else if (elapsed >= timeRange) elapsed = timeRange;

                easingFuncValue = easingFunc(elapsed / timeRange);

                newEvent.end = start + (float)(easingFuncValue * valueRange);

                lastValue = newEvent.end;


                cuttedEvents.Add(newEvent);
            }

            return cuttedEvents;
        }

        public double GetCurVal(double t)
        {
            if (t > startTime && t < endTime)
            {
                return (end - start) * Easings.easeFuncs[easingType]((t - startTime) / (endTime - startTime));
            }
            else if (Math.Abs(t - startTime)<=0.01 || Math.Abs(t - endTime)<=0.01)
            {
                return Math.Abs(t - startTime) <= 0.01 ? start : end;
            }
            else
            {
                return double.NaN;
            }
        }


        public int bezier { get; set; }
        public BezierPoints bezierPoints { get; set; }
        public float easingLeft { get; set; }
        public float easingRight { get; set; }
        public int easingType { get; set; }
        public float end { get; set; }
        public Time endTime { get; set; }
        public int linkgroup { get; set; }
        public float start { get; set; }
        public Time startTime { get; set; }
    }



    //Cubic Bezier
    public class BezierPoints : List<float>
    {

    }

    public class Time : List<int>
    {
        public static Time? Parse(string t)
        {
            double result;
            if (double.TryParse(t,out result))
            {
                return (Time)result;
            }
            else
            {
                string part1 = t.Split(':')[0];
                string part2 = t.Split(":")[1].Split('/')[0];
                string part3 = t.Split(":")[1].Split('/')[1];

                int num1;
                int num2;
                int num3;
                if (int.TryParse(part1, out num1) && int.TryParse(part2, out num2) && int.TryParse(part3, out num3))
                {
                    return new Time() { num1, num2, num3 };
                }
            }
            return null;
        }

        public double GetTime()
        {
            return this[2] == 0 ? 0 : (double)this[0] + ((double)this[1] / (double)this[2]);
        }

        public static implicit operator double(Time time)
        {
            return time[2] == 0 ? 0 : (double)time[0] + ((double)time[1] / (double)time[2]);
        }

        public static explicit operator Time(double value)
        {

            int wholePart = (int)value;
            double fractionPart = value - wholePart;
            int numerator = (int)Math.Round(fractionPart * 1000000);
            int denominator = 1000000;

            // 约分分子和分母
            int gcd = GetGcd(numerator, denominator);
            numerator /= gcd;
            denominator /= gcd;

            
            return new Time() { wholePart, numerator, denominator };
        }


        private static int GetGcd(int a, int b)
        {
            return b == 0 ? a : GetGcd(b, a % b);
        }


    }


    public class SpeedEventsItem
    {
        public float end { get; set; }
        public Time endTime { get; set; }
        public int linkgroup { get; set; }
        public float start { get; set; }
        public Time startTime { get; set; }
    }

    public class EventList : List<RPEEvent>
    {
        public double GetValByTime(double time)
        {
            foreach (RPEEvent e in this)
            {
                if (time >= e.startTime && time <= e.endTime)
                {
                    return e.GetCurVal(time);
                }
            }

            return double.NaN; // 如果列表为空，则返回默认值
        }


        private void GetEventInRange(int baseIndex, int eventCount, out double[] values, out double[] beatTimes, out double beatTimeRange)
        {
            EventList Events = this;

            values = new double[eventCount + 1];
            beatTimes = new double[eventCount];

            for (int outIndex = baseIndex; outIndex < baseIndex + eventCount; outIndex++)
            {
                // 获取当前事件的start值和前一个事件到当前事件的duration
                values[outIndex - baseIndex] = Events[outIndex].start;
                beatTimes[outIndex - baseIndex] = Events[outIndex].GetDuration();
            }

            // 最后一个事件的end值存储到values数组的最后一个位置
            values[^1] = Events[baseIndex + eventCount - 1].end;

            // 计算beatTimeRange
            beatTimeRange = Events[baseIndex + eventCount - 1].endTime.GetTime() - Events[baseIndex].startTime.GetTime();
        }

        private void ReplaceEvent(RPEEvent eventToAdd, int baseIndex, int removeCount)
        {
            EventList eventsToBeReplaced = this;
            eventsToBeReplaced.RemoveRange(baseIndex, removeCount);
            eventsToBeReplaced.Insert(baseIndex, eventToAdd);
        }

        public void CutEventInRange(int startIndex, int endIndex, int density)
        {
            EventList events = this;

            EventList cutted = new EventList { };

            var range = events.GetRange(startIndex, endIndex - startIndex +1);
            
            foreach (RPEEvent e in range)
            {
                cutted.AddRange(e.Cut(density));
            }

            events.RemoveRange(startIndex, endIndex - startIndex + 1);

            events.InsertRange(startIndex, cutted);
        }

        public EventList GetEventInTimeRange(double startTime, double endTime)
        {
            EventList events = this;

            EventList eventsInTime = new EventList { };

            foreach (RPEEvent e in events)
            {
                if ((e.endTime >= startTime && e.endTime <= endTime) || (e.startTime >= startTime && e.startTime <= endTime))
                {
                    eventsInTime.Add(e);
                }
            }
            return eventsInTime;
        }

        public (int,int) GetEventIndexRangeByTime(double startTime, double endTime)
        {
            EventList list = GetEventInTimeRange(startTime, endTime);
            return (IndexOf(list[0]), IndexOf(list[^1]));
        }
    }


    public class EventLayersItem
    {
        [JsonIgnore]
        private EventList _alphaEvents;
        [JsonIgnore]
        private EventList _moveXEvents;
        [JsonIgnore]
        private EventList _moveYEvents;
        [JsonIgnore]
        private EventList _rotateEvents;
        [JsonIgnore]
        private List<SpeedEventsItem> _speedEvents;

        public EventList alphaEvents
        {
            get { return _alphaEvents; }
            set { _alphaEvents = value; }
        }

        public EventList moveXEvents
        {
            get { return _moveXEvents; }
            set { _moveXEvents = value; }
        }

        public EventList moveYEvents
        {
            get { return _moveYEvents; }
            set { _moveYEvents = value; }
        }

        public EventList rotateEvents
        {
            get { return _rotateEvents; }
            set { _rotateEvents = value; }
        }

        public List<SpeedEventsItem> speedEvents
        {
            get { return _speedEvents; }
            set { _speedEvents = value; }
        }

        //<summary>
        //1:moveX 2:moveY 3:rotation 4:alpha 5:speed
        //</summary>
        

        
        
    }


    public class Extended
    {
        public Extended() {
            RPEEvent defaultInc = new RPEEvent();
            defaultInc.start = 0.0f;
            defaultInc.end = 0.0f;
            defaultInc.easingType = 1;
            defaultInc.startTime = new Time { 0, 0, 1 };
            defaultInc.endTime = new Time { 1, 0, 1 };
            inclineEvents = new EventList
            {
                defaultInc
            };
        }
        public EventList inclineEvents { get; set; }
    }

    public class NotesItem
    {
        public int above { get; set; }
        public int alpha { get; set; }
        public Time endTime { get; set; }
        public int isFake { get; set; }
        public float positionX { get; set; }
        public float size { get; set; }
        public float speed { get; set; }
        public Time startTime { get; set; }
        public int type { get; set; }
        public float visibleTime { get; set; }
        public float yOffset { get; set; }
    }

    public class PosControlItem
    {
        public PosControlItem(float xx)
        {
            pos = 1.0f;
            easing = 1;
            x = xx;
        }
        public int easing { get; set; }
        public float pos { get; set; }
        public float x { get; set; }
    }
    public class SizeControlItem
    {
        public SizeControlItem(float xx)
        {
            size = 1.0f;
            easing = 1;
            x = xx;
        }
        public int easing { get; set; }
        public float size { get; set; }
        public float x { get; set; }
    }
    public class SkewControlItem
    {
        public SkewControlItem(float xx)
        {
            skew = 0.0f;
            easing = 1;
            x = xx;
        }
        public int easing { get; set; }
        public float skew { get; set; }
        public float x { get; set; }
    }
    public class YControlItem
    {
        public YControlItem(float xx)
        {
            y = 1.0f;
            easing = 1;
            x = xx;
        }
        public int easing { get; set; }
        public float x { get; set; }
        public float y { get; set; }
    }
    public class JudgeLineListItem
    {
        public int @Group { get; set; }
        public string Name { get; set; }
        public string Texture { get; set; }
        public List<AlphaControlItem> alphaControl { get; set; }
        public float bpmfactor { get; set; }
        public List<EventLayersItem> eventLayers { get; set; }
        public Extended extended { get; set; }
        public int father { get; set; }
        public int isCover { get; set; }
        public List<NotesItem> notes { get; set; }
        public int numOfNotes { get; set; }
        public List<PosControlItem> posControl { get; set; }
        public List<SizeControlItem> sizeControl { get; set; }
        public List<SkewControlItem> skewControl { get; set; }
        public List<YControlItem> yControl { get; set; }
        public int zOrder { get; set; }
    }
    public class RPEChart
    {
        public List<BPMListItem> BPMList { get; set; }
        public META META { get; set; }
        public List<string> judgeLineGroup { get; set; }
        public List<JudgeLineListItem> judgeLineList { get; set; }
    }
}
