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

        public List<RPEEvent> Cut(double density)
        {
            double duration = 1.0 / density;
            double startTimeAsDouble = startTime;
            double endTimeAsDouble = endTime;
            double timeRange = endTimeAsDouble - startTimeAsDouble;
            float valueRange = end - start;
            double easingFuncValue;

            List<RPEEvent> cuttedEvents = new List<RPEEvent>();

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



    public class EventLayersItem
    {
        [JsonIgnore]
        private List<RPEEvent> _alphaEvents;
        [JsonIgnore]
        private List<RPEEvent> _moveXEvents;
        [JsonIgnore]
        private List<RPEEvent> _moveYEvents;
        [JsonIgnore]
        private List<RPEEvent> _rotateEvents;
        [JsonIgnore]
        private List<SpeedEventsItem> _speedEvents;

        public List<RPEEvent> alphaEvents
        {
            get { return _alphaEvents; }
            set { _alphaEvents = value; }
        }

        public List<RPEEvent> moveXEvents
        {
            get { return _moveXEvents; }
            set { _moveXEvents = value; }
        }

        public List<RPEEvent> moveYEvents
        {
            get { return _moveYEvents; }
            set { _moveYEvents = value; }
        }

        public List<RPEEvent> rotateEvents
        {
            get { return _rotateEvents; }
            set { _rotateEvents = value; }
        }

        public List<SpeedEventsItem> speedEvents
        {
            get { return _speedEvents; }
            set { _speedEvents = value; }
        }

        private ref List<RPEEvent> GetEventsByType(string type)
        {
            switch (type)
            {
                case "alpha":
                    return ref _alphaEvents;

                case "moveX":
                    return ref _moveXEvents;

                case "moveY":
                    return ref _moveYEvents;

                case "rotate":
                    return ref _rotateEvents;

                default:
                    return ref _moveXEvents;
            }
        }

        private void GetEventInRange(string EventType, int baseIndex, int eventCount, out double[] values, out double[] beatTimes, out double beatTimeRange)
        {
            List<RPEEvent> Events = GetEventsByType(EventType);

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

        //<summary>
        //1:moveX 2:moveY 3:rotation 4:alpha 5:speed
        //</summary>
        private void ReplaceEvent(RPEEvent eventToAdd, int baseIndex, int removeCount, string EventType)
        {
            List<RPEEvent> eventsToBeReplaced = GetEventsByType(EventType);
            eventsToBeReplaced.RemoveRange(baseIndex, removeCount);
            eventsToBeReplaced.Insert(baseIndex, eventToAdd);
        }

        public void CutEventInRange(int startIndex, int endIndex, int density, string EventType)
        {
            List<RPEEvent> events = GetEventsByType(EventType);

            List<RPEEvent> cutted = new List<RPEEvent> { };

            List<RPEEvent> range = events.GetRange(startIndex, endIndex - startIndex);


            foreach (RPEEvent e in range)
            {
                cutted.AddRange(e.Cut(density));
            }

            events.RemoveRange(startIndex, endIndex - startIndex);

            events.InsertRange(startIndex, cutted);
        }

        public List<RPEEvent> GetEventInTimeRange(double startTime, double endTime, string EventType)
        {
            List<RPEEvent> events = GetEventsByType(EventType);

            List<RPEEvent> eventsInTime = new List<RPEEvent> { };

            foreach(RPEEvent e in events)
            {
                if ((e.endTime >= startTime && e.endTime >= endTime)|| (e.startTime >= startTime && e.startTime >= endTime))
                {
                    eventsInTime.Add(e);
                }
            }

            return eventsInTime;
        }
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
            inclineEvents = new List<RPEEvent>
            {
                defaultInc
            };
        }
        public List<RPEEvent> inclineEvents { get; set; }
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
