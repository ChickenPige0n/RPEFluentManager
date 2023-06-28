using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPEFluentManager.Models
{
    internal class Easings
    {
        //constants for Back
        const double c1 = 1.70158;
        const double c2 = c1 * 1.525;
        const double c3 = c1 + 1;
        const double c4 = (2 * Math.PI) / 3;
        const double n1 = 7.5625;
        const double d1 = 2.75;
        const double c5 = (2 * Math.PI) / 4.5;


        //1
        public static double EaseLinear(double x)
        {
            return x;
        }
        //2
        public static double EaseOutSin(double x)
        {
            return Math.Sin((x * Math.PI) / 2);
        }
        //3
        public static double EaseInSin(double x)
        {
            return 1 - Math.Cos((x * Math.PI) / 2);
        }
        //4
        public static double EaseOutQuad(double x)
        {
            return 1 - (1 - x) * (1 - x);
        }
        //5
        public static double EaseInQuad(double x)
        {
            return x * x;
        }
        //6
        public static double EaseInOutSin(double x)
        {
            return -(Math.Cos(Math.PI * x) - 1) / 2;
        }
        //7
        public static double EaseInOutQuad(double x)
        {
            return x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2;
        }
        //8
        public static double EaseOutCubic(double x)
        {
            return 1 - Math.Pow(1 - x, 3);
        }
        //9
        public static double EaseInCubic(double x)
        {
            return x * x * x;
        }
        //10
        public static double EaseOutQuart(double x)
        {
            return 1 - Math.Pow(1 - x, 4);
        }
        //11
        public static double EaseInQuart(double x)
        {
            return x * x * x * x;
        }
        //12
        public static double EaseInOutCubic(double x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }
        //13
        public static double EaseInOutQuart(double x)
        {
            return x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2;
        }
        //14
        public static double EaseOutQuint(double x)
        {
            return 1 - Math.Pow(1 - x, 5);
        }
        //15
        public static double EaseInQuint(double x)
        {
            return x * x * x * x * x;
        }
        //16
        public static double EaseOutExpo(double x)
        {
            return x == 1 ? 1 : 1 - Math.Pow(2, -10.0 * x);
        }
        //17
        public static double EaseInExpo(double x)
        {
            return x == 0 ? 0 : Math.Pow(2, 10 * x - 10);
        }
        //18
        public static double EaseOutCirc(double x)
        {
            return Math.Sqrt(1 - Math.Pow(x - 1, 2));
        }
        //19
        public static double EaseInCirc(double x)
        {
            return 1 - Math.Sqrt(1 - Math.Pow(x, 2));
        }
        //20
        public static double EaseOutBack(double x)
        {
            return 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
        }
        //21
        public static double EaseInBack(double x)
        {
            return c3 * x * x * x - c1 * x * x;
        }
        //22
        public static double EaseInOutCirc(double x)
        {
            return x < 0.5
            ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2
            : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2;
        }
        //23
        public static double EaseInOutBack(double x)
        {
            return x < 0.5
            ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
            : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }
        //24
        public static double EaseOutElastic(double x)
        {
            return x == 0
              ? 0
              : x == 1
              ? 1
              : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;
        }
        //25
        public static double EaseInElastic(double x)
        {
            return x == 0
              ? 0
              : x == 1
              ? 1
              : -Math.Pow(2, 10 * x - 10) * Math.Sin((x * 10 - 10.75) * c4);
        }
        //26
        public static double EaseOutBounce(double x)
        {
            if (x < 1 / d1)
            {
                return n1 * x * x;
            }
            else if (x < 2 / d1)
            {
                return n1 * (x -= 1.5 / d1) * x + 0.75;
            }
            else if (x < 2.5 / d1)
            {
                return n1 * (x -= 2.25 / d1) * x + 0.9375;
            }
            else
            {
                return n1 * (x -= 2.625 / d1) * x + 0.984375;
            }
            
        }
        //27
        public static double EaseInBounce(double x)
        {
            return 1 - EaseOutBounce(1 - x);
        }
        //28
        public static double EaseInOutBounce(double x)
        {
            return x< 0.5
              ? (1 - EaseOutBounce(1 - 2 * x)) / 2
              : (1 + EaseOutBounce(2 * x - 1)) / 2;
        }


        public delegate double EasingFunc(double x);

        public static EasingFunc[] easeFuncs = {
            new EasingFunc(EaseLinear),
            new EasingFunc(EaseLinear),
            new EasingFunc(EaseOutSin),
            new EasingFunc(EaseInSin),
            new EasingFunc(EaseOutQuad),
            new EasingFunc(EaseInQuad),//5
            new EasingFunc(EaseInOutSin),
            new EasingFunc(EaseInOutQuad),
            new EasingFunc(EaseOutCubic),
            new EasingFunc(EaseInCubic),
            new EasingFunc(EaseOutQuart),//10
            new EasingFunc(EaseInQuart),
            new EasingFunc(EaseInOutCubic),
            new EasingFunc(EaseInOutQuart),
            new EasingFunc(EaseOutQuint),
            new EasingFunc(EaseInQuint),//15
            new EasingFunc(EaseOutExpo),
            new EasingFunc(EaseInExpo),
            new EasingFunc(EaseOutCirc),
            new EasingFunc(EaseInCirc),
            new EasingFunc(EaseOutBack),//20
            new EasingFunc(EaseInBack),
            new EasingFunc(EaseInOutCirc),
            new EasingFunc(EaseInOutBack),
            new EasingFunc(EaseOutElastic),
            new EasingFunc(EaseInElastic),//25
            new EasingFunc(EaseOutBounce),
            new EasingFunc(EaseInBounce),
            new EasingFunc(EaseInOutBounce)
        };
    }
}
