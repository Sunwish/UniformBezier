using System;
using System.Drawing;
namespace UniformBezier
{
    public class Bezier_3
    {
        public Point P1 { get; set; }
		public Point P1_Control { get; set; }
		public Point P2_Control { get; set; }
		public Point P2 { get; set; }

        private double b3(Point[] p, double t)
        {
			
            // 三阶贝塞尔运算
            double retn;
            retn = Math.Pow(1 - t, 3) * p[0].X + 3 * t * Math.Pow(1 - t, 2) * p[1].X + 3 * Math.Pow(t, 2) * (1 - t) * p[2].X + Math.Pow(t, 3) * p[3].X;
            return retn;
        }

		public Point b3_c(Point[] p, double t)
		{
			// 三阶贝塞尔运算
			Point c = new Point();
			c.X = (int)(Math.Pow(1 - t, 3) * p[0].X + 3 * t * Math.Pow(1 - t, 2) * p[1].X + 3 * Math.Pow(t, 2) * (1 - t) * p[2].X + Math.Pow(t, 3) * p[3].X);
			c.Y = (int)(Math.Pow(1 - t, 3) * p[0].Y + 3 * t * Math.Pow(1 - t, 2) * p[1].Y + 3 * Math.Pow(t, 2) * (1 - t) * p[2].Y + Math.Pow(t, 3) * p[3].Y);
			return c;
		}

		public double t2rt(Point[] p, double t)
		{
			// 定义真实时间与差时变量
			double realTime, deltaTime = 0;

			// 曲线上的 x 坐标
			double bezierX;

			// 计算 t 对应曲线上匀速的 x 坐标
			double x = p[0].X + (p[3].X - p[0].X) * t;

			double low = 0, high = 1;

			realTime = 0.5;
			int intT = 0;
			do
			{
				++intT;

				// 半分
				if (deltaTime > 0)
				{

					realTime -= (double)(realTime - low) / 2;
				}
				else
				{
					realTime += (double)(high - realTime) / 2;
				}

				// 计算本此 "rt" 对应的曲线上的 x 坐标
				bezierX = b3(p, realTime);

				// 计算差时
				deltaTime = bezierX - x;

				if (deltaTime > 0) high = realTime;
				else low = realTime;

				// Console.WriteLine(deltaTime);

				// Console.WriteLine("realTime: " + realTime + ", bezierX: " + bezierX + ", realX: " + x + ", deltaTime: " + deltaTime);
			}
			// 差时逼近为0时跳出循环
			while (Math.Abs(deltaTime) > 0.000000000001);

			// Console.WriteLine("求rt迭代 " + intT + " 次");
			return realTime;
		}

		// 根据 baze_length 求 t 到 rt 的映射
		public double t2rt_by_baze_length(Point[] p, double length)
		{
			double realTime = 0;
			double rt_length = 0;
			double deltaLength = 0;
			double low = 0, high = 1;
			double deltaTime = 0;
			do
			{
				// 半分
				if (deltaLength > 0)
				{

					realTime -= (double)(realTime - low) / 2;
					deltaTime = realTime - low;
				}
				else
				{
					realTime += (double)(high - realTime) / 2;
					deltaTime = high - realTime;
				}

				// 计算弧长差值
				rt_length = beze_length(p, realTime);
				deltaLength = rt_length - length;

				if (deltaLength > 0) high = realTime;
				else low = realTime;

				//Console.WriteLine("realTime: " + realTime + ", rt_length: " + rt_length + ", length: " + length + ", deltaLength: " + deltaLength);

			} while (Math.Abs(deltaLength) > 0.01 && deltaTime >0.00000000000000001);

			return realTime;
		}


		// 求 0~t 段的贝塞尔曲线长度
		public double beze_length(Point[] p, double t)
		{
			int TOTAL_SIMPSON_STEP;
			int stepCounts;
			int halfCounts;
			int i = 0;
			double sum1 = 0, sum2 = 0, dStep = 0;

			TOTAL_SIMPSON_STEP = 1000;
			stepCounts = (int)(TOTAL_SIMPSON_STEP * t);

			if (stepCounts == 0)
			{
				return 0;
			}
			if (stepCounts % 2 == 0)
			{
				stepCounts++;
			}

			halfCounts = stepCounts / 2;
			dStep = t / stepCounts;

			while (i < halfCounts)
			{
				sum1 += beze_speed(p, (2 * i + 1) * dStep);
				i++;
			}
			i = 1;
			while (i < halfCounts)
			{
				sum2 += beze_speed(p, 2 * i * dStep);
				i++;
			}
			return ((beze_speed(p, 0) + beze_speed(p, 1) + 2 * sum2 + 4 * sum1) * dStep / 3);
		}

		//	求合速度
		double beze_speed(Point[] p, double t)
		{
			double vx = beze_speed_x(p, t);
			double vy = beze_speed_y(p, t);
			return Math.Sqrt(Math.Pow(vx, 2) + Math.Pow(vy, 2));
		}

		double beze_speed_x(Point[] p, double t)
		{
			// 三阶
			return -3 * p[0].X * Math.Pow(1 - t, 2) + 3 * p[1].X * Math.Pow(1 - t, 2) - 6 * p[1].X * (1 - t) *t + 6 * p[2].X * (1 - t) * t - 3 * p[2].X * Math.Pow(t, 2) + 3 * p[3].X * Math.Pow(t, 2);
		}

		double beze_speed_y(Point[] p, double t)
		{
			// 三阶
			return -3 * p[0].Y * Math.Pow(1 - t, 2) + 3 * p[1].Y * Math.Pow(1 - t, 2) - 6 * p[1].Y * (1 - t) *t + 6 * p[2].Y * (1 - t) * t - 3 * p[2].Y * Math.Pow(t, 2) + 3 * p[3].Y * Math.Pow(t, 2);
		}
	}
}
