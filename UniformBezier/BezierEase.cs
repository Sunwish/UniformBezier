using System;
using System.Drawing;
using System.Timers;

namespace UniformBezier.Ease
{
    public class BezierEase : Bezier_3
    {
        public enum BezierStatus
        {
            None, Motioning, Finished

        }
        /// <summary>
        /// 匀速化贝塞尔点回调委托
        /// </summary>
        /// <param name="bezierRoute"></param>
        /// <param name="realPoint"></param>
        public delegate void UniformBezierEaseCallBack(BezierEase bezierRoute, double realRate, BezierStatus status);
        Point[] points;
        Bezier_3 b3 = null;
        double length = 0;
        double fullTime = 0;
        double interval = 0;
        double timeSum = 0;
        Timer timer = null;
        bool start = false;
        Point realPoint;
        bool standardization = true;
        TimeSpan timeSpanStart;
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        public int Tag { get; set; }
        /// <summary>
        /// 若标准化，则最小x、y分别对齐至零
        /// </summary>
        public bool Standardization
        {
            get
            {
                return standardization;
            }
            set
            {
                if (standardization == value) return;
                standardization = value;
                if (standardization)
                {
                    foreach (Point point in points)
                    {
                        if (point.X < minX) minX = point.X;
                        if (point.Y < minY) minY = point.Y;
                    }
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].X -= minX;
                        points[i].Y -= minY;
                    }
                }
                else
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].X -= minX;
                        points[i].Y -= minY;
                    }
                }
            }
        }
        /// <summary>
        /// 完成曲线运动的期望总时间(ms)
        /// </summary>
        public double FullTime
        {
            get
            {
                return fullTime;
            }
            set
            {
                fullTime = value;
            }

        }
        /// <summary>
        /// 回调时间间隔(ms)
        /// </summary>
        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
            }
        }
        public bool Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
                timeSpanStart = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                if (timer != null) timer.Enabled = start;

            }
        }
        /// <summary>
        /// 匀速缓动贝塞尔曲线上此时刻点坐标
        /// </summary>
        public Point RealPoint
        {
            get
            {
                return realPoint;
            }
        }
        Delegate CallBack = null;
        /// <summary>
        /// 初始化匀速贝塞尔路径运动
        /// </summary>
        /// <param name="points">贝塞尔点(0:P1, 1:P1右控制点, 2:P2_左控制点, 3: P2)</param>
        /// <param name="fullTime">完成曲线运动的期望总时间(ms)</param>
        /// <param name="interval">精度</param>
        /// <param name="UniformBezierEaseCallBack">匀速化贝塞尔点回调函数</param>
        public BezierEase(Point[] points, int fullTime, int interval, UniformBezierEaseCallBack uniformBezierEaseCallBack)
        {
            Start = false;
            FullTime = fullTime;
            Interval = interval;
            CallBack = uniformBezierEaseCallBack;
            timer = new Timer(interval);
            timer.Elapsed += Timer_Elapsed;
            this.points = points;
            b3 = new Bezier_3();
            length = b3.beze_length(points, 1);
            for (int i = 0; i < points.Length && i < 4; i++)
            {
                switch (i)
                {
                    case 0: P1 = points[0]; break;
                    case 1: P1_Control = points[1]; break;
                    case 2: P2_Control = points[2]; break;
                    case 3: P2 = points[3]; break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 运动进度归零
        /// </summary>
        public void Reset() => timeSum = 0;

        public void Reverse()
        {
            Point tpA = new Point(points[0].X, points[0].Y);
            Point tpB = new Point(points[1].X, points[1].Y);
            Point tpC = new Point(points[2].X, points[2].Y);
            Point tpD = new Point(points[3].X, points[3].Y);
            points[0] = tpD;
            points[1] = tpC;
            points[2] = tpB;
            points[3] = tpA;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeSum = ((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)) - timeSpanStart).TotalMilliseconds;
            if (timeSum <= fullTime)
            {
                double rt = b3.t2rt(points, timeSum / fullTime);
                realPoint = b3.b3_c(points, rt);
                double realRate = (double)(points[0].Y - realPoint.Y) / (points[0].Y - points[3].Y);
                CallBack.DynamicInvoke(this, realRate, BezierStatus.Motioning);
            }
            else
            {
                timer.Enabled = false;
                start = false;
                CallBack.DynamicInvoke(this, 1, BezierStatus.Finished);
            }
        }
    }
}
