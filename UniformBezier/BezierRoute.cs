using System;
using System.Drawing;
using System.Timers;

namespace UniformBezier.Route
{
    public class BezierRoute : Bezier_3
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
        public delegate void UniformBezierRouteCallBack(BezierRoute bezierRoute, Point realPoint, BezierStatus status);
        Point[] points;
        Point[] easePoints;
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
        bool enableEaseRoute = false;
        double easeRealRate = 0;
        // 由于使用二分求数据，因此可能出现 Finish 数据先算出并调用回调，而尚未 Finish 轮次的数据在 Finish 调用后才算出，又去调用回调，
        // 导致出现向使用者发出 Finish 回调之后仍发出残余回调的问题， 故在发出 Finish 轮回调后就将 banCall 置真，后续慢算出来的数据全部舍弃，不再回调给使用者
        bool banCall = true;

        /// <summary>
        /// 仅 EnableEaseRoute 时本参数有意义
        /// </summary>
        public double EaseRealRete
        {
            get { return easeRealRate; }
        }

        /// <summary>
        /// 运动(生成)轨迹贝塞尔控制点
        /// </summary>
        public Point[] RoutePoints
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }

        /// <summary>
        /// 在曲线上遵循的缓动贝塞尔曲线控制点（仅 EnableEaseRoute 时本参数有意义）
        /// </summary>
        public Point[] EasePoints
        {
            get
            {
                return easePoints;
            }
            set
            {
                easePoints = value;
            }
        }

        /// <summary>
        /// 获取是否启用了路径缓动模式
        /// </summary>
        /// <returns></returns>
        public bool EnableEaseRoute()
        {
            return enableEaseRoute;
        }

        /// <summary>
        /// 启用并设置路径生成中遵循的缓动规则
        /// </summary>
        /// <param name="easePoints">缓动贝塞尔曲线控制点</param>
        /// <returns></returns>
        public bool EnableEaseRoute(Point[] easePoints)
        {
            if (easePoints.Length != 4) return false;
            this.easePoints = easePoints;
            enableEaseRoute = true;
            return enableEaseRoute;
        }

        /// <summary>
        /// 禁用缓动路径生成（即恢复为匀速生成贝塞尔路径）
        /// </summary>
        /// <returns></returns>
        public bool DisableEaseRoute()
        {
            enableEaseRoute = false;
            return enableEaseRoute;
        }
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
                    foreach(Point point in points)
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
                banCall = !value;
                if (timer != null) timer.Enabled = start;

            }
        }
        /// <summary>
        /// 匀速生成贝塞尔曲线上此时刻点坐标
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
        /// <param name="UniformBezierRouteCallBack">匀速化贝塞尔点回调函数</param>
        public BezierRoute(Point[] points, int fullTime, int interval, UniformBezierRouteCallBack uniformBezierRouteCallBack)
        {
            Start = false;
            FullTime = fullTime;
            Interval = interval;
            CallBack = uniformBezierRouteCallBack;
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
                double timeRate = timeSum / fullTime;
                if (enableEaseRoute)
                {
                    double easeRt = b3.t2rt(easePoints, timeRate);
                    Point easeRealPoint = b3.b3_c(easePoints, easeRt);
                    timeRate = (double)(easePoints[0].Y - easeRealPoint.Y) / (easePoints[0].Y - easePoints[3].Y);
                    easeRealRate = timeRate;
                }
                double rt = b3.t2rt_by_baze_length(points, length * timeRate);
                realPoint = b3.b3_c(points, rt);
                if(!banCall) CallBack.DynamicInvoke(this, realPoint, BezierStatus.Motioning);
            }
            else
            {
                timer.Enabled = false;
                start = false;
                if (!banCall) CallBack.DynamicInvoke(this, points[3], BezierStatus.Finished);
                banCall = true;
            }
        }
    }
}
