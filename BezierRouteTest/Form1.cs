using System;
using System.Drawing;
using System.Windows.Forms;
using UniformBezier.Route;
using UniformBezier.Ease;
using System.Threading;
namespace UniformBezierTest
{
    public partial class Form1 : Form
    {
        Point[] points = new Point[4];
        Point[] easePoints = new Point[4];
        BezierEase[] bezierEase = new BezierEase[3];
        SolidBrush[] brush = new SolidBrush[3];
        Point[] pE = new Point[3];
        bool[] pE_Drawed = new bool[3];
        Graphics g_demo = null;
        Graphics g_curve = null;
        Point[] orignPoints = new Point[3];
        double distance = 0;
        int curvePointI = 0;
        int curvePointInterval = 5;

        Graphics g_bitmap;
        Bitmap bitmap;

        BezierRoute bezierRoute = null;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Create graphics
            g_demo = panel1.CreateGraphics();
            g_curve = panel2.CreateGraphics();

            // Bezier points
            points[0] = new Point(10, 221);
            points[1] = new Point(40, 31);
            points[2] = new Point(310, 201);
            points[3] = new Point(340, 11);

            // Ease points
            easePoints[0] = new Point(10, 221);
            easePoints[1] = new Point(40, 31);
            easePoints[2] = new Point(310, 201);
            easePoints[3] = new Point(340, 11);

            // Route bezier initialization
            bezierRoute = new BezierRoute(points, 2000, 1, UniformBezierRouteCallBack);
            bezierRoute.Tag = 1;

            // Ease bezier demo points parameters
            orignPoints[0] = new Point(0, panel1.Height / 2);
            orignPoints[1] = new Point(-8, panel1.Height / 2);
            orignPoints[2] = new Point(-16, panel1.Height / 2);
            distance = panel1.Width;
            pE[0] = new Point(orignPoints[0].X, orignPoints[0].Y);
            pE[1] = new Point(orignPoints[1].X, orignPoints[1].Y);
            pE[2] = new Point(orignPoints[2].X, orignPoints[2].Y);
            brush[0] = new SolidBrush(Color.White);
            brush[1] = new SolidBrush(Color.White);
            brush[2] = new SolidBrush(Color.White);
            bitmap = new Bitmap(panel1.Width, panel1.Height);
            g_bitmap = Graphics.FromImage(bitmap);

            // Ease bezier demo points initialization
            bezierEase[0] = new BezierEase(easePoints, 2000, 1, UniformBezierEaseDemoCallBack);
            bezierEase[0].Tag = 0;
            bezierEase[1] = new BezierEase(easePoints, 2000, 1, UniformBezierEaseDemoCallBack);
            bezierEase[1].Tag = 1;
            bezierEase[2] = new BezierEase(easePoints, 2000, 1, UniformBezierEaseDemoCallBack);
            bezierEase[2].Tag = 2;  
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawBezierCurve();
        }

        private void DrawBezierCurve()
        {
            lock (this)
            {
                g_curve.Clear(this.BackColor);
                g_curve.DrawBezier(new Pen(new SolidBrush(Color.Red)), points[0], points[1], points[2], points[3]);
                foreach (Point p in points)
                {
                    g_curve.FillRectangle(new SolidBrush(Color.Red), p.X - 2, p.Y - 2, 4, 4);
                }
                g_curve.DrawLine(new Pen(Color.Red), points[0], points[1]);
                g_curve.DrawLine(new Pen(Color.Red), points[2], points[3]);
            }
        }

        private void UpdateCurvePanel(Point RealPoint)
        {
            lock (this)
            {
                // X
                g_curve.FillRectangle(new SolidBrush(Color.Red), RealPoint.X - 2, points[0].Y - 2, 4, 4);
                // Y
                g_curve.FillRectangle(new SolidBrush(Color.Red), points[0].X - 2, RealPoint.Y - 2, 4, 4);
                // Curve point
                g_curve.FillRectangle(new SolidBrush(Color.Red), RealPoint.X - 2, RealPoint.Y - 2, 4, 4);
            }
        }

        private void UniformBezierRouteCallBack(BezierRoute bezierRoute, Point realPoint, BezierRoute.BezierStatus status)
        {
            // Draw demo panel
            lock (this)
            {
                g_demo.Clear(panel1.BackColor);
                int biasX = 0;
                int biasY = 0;
                int biasW = 0;
                int biasH = 0;
                if(bezierRoute.EnableEaseRoute())
                {
                    if (bezierRoute.Tag == 1)
                    {
                        biasX = (int)(bezierRoute.EaseRealRete * (615 - 8)) / 2;
                        biasY = (int)(bezierRoute.EaseRealRete * (10 - 8)) / 2;
                        biasW = (int)(bezierRoute.EaseRealRete * (350 - 8));
                        biasH = (int)(bezierRoute.EaseRealRete * (120 - 8));
                    }
                    else if (bezierRoute.Tag == -1)
                    {
                        biasX = (int)((1 - bezierRoute.EaseRealRete) * (615 - 8)) / 2;
                        biasY = (int)((1 - bezierRoute.EaseRealRete) * (10 - 8)) / 2;
                        biasW = (int)((1 - bezierRoute.EaseRealRete) * (350 - 8));
                        biasH = (int)((1 - bezierRoute.EaseRealRete) * (120 - 8));
                    }
                }
                g_demo.FillRectangle(new SolidBrush(Color.White), new RectangleF(realPoint.X - 4 - biasX, realPoint.Y - 4 - biasY, 8 + biasW, 8 + biasH));
            }
            if (status == BezierRoute.BezierStatus.Finished)
            {
                curvePointI = 0;
                // DrawBezierCurve();
                bezierRoute.Reverse();
                bezierRoute.Tag = -bezierRoute.Tag;
                //bezierRoute.Start = true;
                Console.WriteLine("Finished");
            }

            // Update curve panel
            if (0 == ++curvePointI % curvePointInterval) UpdateCurvePanel(realPoint);
        }

        private void UniformBezierEaseDemoCallBack(BezierEase bezierEase, double realRate, BezierEase.BezierStatus status)
        {
            // Draw demo panel
            if (status == BezierEase.BezierStatus.Finished) realRate = 1;
            switch (status)
            {
                case BezierEase.BezierStatus.None:
                    break;
                case BezierEase.BezierStatus.Motioning:
                    lock (this)
                    {
                        pE[bezierEase.Tag].X = (int)(orignPoints[bezierEase.Tag].X + distance * realRate);
                    }
                    break;
                case BezierEase.BezierStatus.Finished:
                    DrawBezierCurve();
                    curvePointI = 0;
                    this.bezierEase[bezierEase.Tag].Start = true;
                    Console.WriteLine("Finished");
                    break;
                default:
                    break;
            }

            // Update curve panel
            if (0 == ++curvePointI % curvePointInterval) UpdateCurvePanel(bezierEase.RealPoint);
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            // Stop ease bezier
            bezierEase[0].Start = bezierEase[1].Start = bezierEase[2].Start = false;
            timer1.Enabled = false;

            // Start route bezier
            g_curve.Clear(this.BackColor);
            DrawBezierCurve();
            bezierRoute.Start = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Stop route bezier
            bezierRoute.Start = false;

            // Start ease bezier
            DrawBezierCurve();
            timer1.Enabled = true;
            bezierEase[0].Start = true;
            Thread.Sleep(150);
            bezierEase[1].Start = true;
            Thread.Sleep(150);
            bezierEase[2].Start = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (this)
            {
                g_bitmap.Clear(panel1.BackColor);

                for (int i = 0; i < pE.Length; i++)
                {
                    g_bitmap.FillRectangle(brush[i], new RectangleF(pE[i].X - 4, pE[i].Y - 4, 8, 8));
                }

                g_demo.DrawImage(bitmap, 0, 0);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Stop route bezier
            bezierRoute.Start = false;

            // Stop ease bezier demo
            bezierEase[0].Start = bezierEase[1].Start = bezierEase[2].Start = false;
            timer1.Enabled = false;

            // Start ease bezier
            g_curve.Clear(this.BackColor);
            DrawBezierCurve();
            timer1.Enabled = true;
            bezierEase[0].Start = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // 临时测试区 Bezier ease route
            Point[] tempRoutePoints = new Point[4];
            tempRoutePoints[0] = new Point(10, 221);
            tempRoutePoints[1] = new Point(211, 221);
            tempRoutePoints[2] = new Point(311, 201);
            tempRoutePoints[3] = new Point(311, 10);
            Point[] tempEasePoints = new Point[4];
            tempEasePoints[0] = new Point(10, 111);
            tempEasePoints[1] = new Point(110, 21);
            tempEasePoints[2] = new Point(21, 10);
            tempEasePoints[3] = new Point(311, 10);
            bezierRoute.RoutePoints = tempRoutePoints;
            if (bezierRoute.Tag == -1) bezierRoute.Reverse();
            bezierRoute.EnableEaseRoute(tempEasePoints);
            bezierRoute.FullTime = 1800;
            button1_Click(sender, e);
        }
    }
}
