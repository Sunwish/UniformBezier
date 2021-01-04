using System;
using System.Drawing;
using UniformBezier;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
			Bezier_3 b3 = new Bezier_3();

			// 输出 t=0.5 对应的匀速 rt
			Point[] p = new Point[4];
			p[0] = new Point(100, 611);
			p[1] = new Point(300, 411);
			p[2] = new Point(400, 311);
			p[3] = new Point(500, 411);
			Console.WriteLine(b3.t2rt(p, 0.05));

		}
    }
}
