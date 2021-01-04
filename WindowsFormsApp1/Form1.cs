using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniformBezier;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        double timeSigma = 0;
        int timeFull = 3200;
        Point[] p = new Point[4];
        Bezier_3 b3 = new Bezier_3();
        Graphics g;
        Pen pen = new Pen(new SolidBrush(Color.Red));
        TimeSpan ts;
        double lastTime = 0;
        int lastX = 0;
        int easeX_Start, easeX_End = 0;
        int direction = 0;
        bool finish = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            p[0] = new Point(10, 111);
            p[1] = new Point(10, 0);
            p[2] = new Point(10, 10);
            p[3] = new Point(911, 10);

            /*
            p[0] = new Point(100, 311);
            p[1] = new Point(200, -321);
            p[2] = new Point(500, 721);
            p[3] = new Point(500, 111);
            */

            /*
            p[3] = new Point(100, 311);
            p[2] = new Point(650, 291);
            p[1] = new Point(100, 211);
            p[0] = new Point(500, 111);
            */
            /*
            p[0] = new Point(100, 311);
            p[1] = new Point(650, 291);
            p[2] = new Point(100, 211);
            p[3] = new Point(500, 111);
            */
            /*
            p[0] = new Point(100, 311);
            p[1] = new Point(100, 311);
            p[2] = new Point(500, 111);
            p[3] = new Point(500, 111);
            */

            easeX_Start = button1.Left;
            easeX_End = button1.Left + 200;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Interval = 60;
            ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            timer1.Enabled = true;
            // timer2.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            new Thread(timerHandlerThread).Start();
        }

        private void timerHandlerThread()
        {
            int uniform = 0, ununiform = 1, timer = 3, beze_length = 4;
            int way = uniform;
            double length = 0;

            if (way != timer)
            {
                TimeSpan ts2 = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)) - ts;
                timeSigma = ts2.TotalMilliseconds;
            }
            if(way == beze_length)
            {
                length = b3.beze_length(p, 1);
            }
            double deltaTime = timeSigma - lastTime;
            // Console.WriteLine(deltaTime);
            lastTime = timeSigma;
            if (timeSigma <= timeFull)
            {
                Point realPoint = new Point();
                Point unrealPoint = new Point();

                if (way == uniform)
                {
                    // Uniform
                    double rt = b3.t2rt(p, timeSigma / timeFull);
                    realPoint = b3.b3_c(p, rt);
                    int deltaX = realPoint.X - lastX;
                    lastX = realPoint.X;
                    //Console.WriteLine((double)deltaX / deltaTime);

                    lock (this)
                    {
                        // x(Uniform)
                        g.DrawRectangle(pen, new Rectangle(realPoint.X - 1, p[0].Y - 1, 2, 2));

                        // y(Ease)
                        g.DrawRectangle(pen, new Rectangle(p[0].X - 1, realPoint.Y - 1, 2, 2));

                        // CurveMotion
                        g.DrawRectangle(pen, new Rectangle(realPoint.X - 2, realPoint.Y - 2, 4, 4));
                    }
                }
                else if(way == ununiform)
                {
                    // Ununiform
                    unrealPoint = b3.b3_c(p, timeSigma / timeFull);
                    int deltaX = unrealPoint.X - lastX;
                    lastX = unrealPoint.X;
                    //Console.WriteLine((double)deltaX / deltaTime);

                    lock (this)
                    {
                        // x(Uniform)
                        g.DrawRectangle(pen, new Rectangle(unrealPoint.X - 1, p[0].Y - 1, 2, 2));

                        // y(Ease)
                        g.DrawRectangle(pen, new Rectangle(p[0].X - 1, unrealPoint.Y - 1, 2, 2));

                        // CurveMotion
                        g.DrawRectangle(pen, new Rectangle(unrealPoint.X - 2, unrealPoint.Y - 2, 4, 4));
                    }
                }
                else if(way == timer)
                {
                    // Timer
                    timeSigma += timer1.Interval;
                    unrealPoint = b3.b3_c(p, timeSigma / timeFull);

                    lock (this)
                    {
                        // x(Uniform)
                        g.DrawRectangle(pen, new Rectangle(unrealPoint.X - 1, p[0].Y - 1, 2, 2));

                        // y(Ease)
                        g.DrawRectangle(pen, new Rectangle(p[0].X - 1, unrealPoint.Y - 1, 2, 2));

                        // CurveMotion
                        g.DrawRectangle(pen, new Rectangle(unrealPoint.X - 2, unrealPoint.Y - 2, 4, 4));
                    }
                }
                else if(way == beze_length)
                {
                    lock (this)
                    {
                        // BeizeLength
                        double rt = b3.t2rt_by_baze_length(p, length * timeSigma / timeFull);
                        realPoint = b3.b3_c(p, rt);
                    
                        // x(Uniform)
                        g.FillRectangle(new SolidBrush(pen.Color), new Rectangle(realPoint.X - 1, p[0].Y - 1, 2, 2));

                        // y(Ease)
                        g.FillRectangle(new SolidBrush(pen.Color), new Rectangle(p[0].X - 1, realPoint.Y - 1, 2, 2));

                        // CurveMotion
                        g.FillRectangle(new SolidBrush(pen.Color), new Rectangle(realPoint.X - 3, realPoint.Y - 3, 6, 6));
                    }
                }

                // Console.WriteLine("timeSigma = " + timeSigma + ", t = " + timeSigma / timeFull + ", rt = " + rt);

                //double shiftRate = (realPoint.Y - p[0].Y) / (p[3].Y - p[0].Y); // 位移率，当前y轴（位移）量占始末点总位移量的矢量比例
                //button1.BeginInvoke(new Action(() => button1.Left = (int)(easeX_Start + (easeX_End - easeX_Start) * shiftRate)));
                // Console.WriteLine(button1.Left);
            }
            else
            {
                timeSigma = 0;
                timer1.Enabled = false;
                finish = true;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!finish) return;
            finish = false;
            Random random = new Random();
            pen.Color = Color.FromArgb((int)(random.NextDouble() * 255), (int)(random.NextDouble() * 255), (int)(random.NextDouble() * 255));

            if (direction == 1)
            {
                p[3] = new Point(100, 311);
                p[2] = new Point(650, 291);
                p[1] = new Point(100, 211);
                p[0] = new Point(500, 111);
                direction = 0;
            }
            else
            {
                p[0] = new Point(100, 311);
                p[1] = new Point(650, 291);
                p[2] = new Point(100, 211);
                p[3] = new Point(500, 111);
                direction = 1;
            }
            timer1.Enabled = true;
            Console.WriteLine("Hello!");
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            g = panel1.CreateGraphics();
            lock(this)
            {
                g.DrawBezier(pen, p[0], p[1], p[2], p[3]);
            }
            foreach (Point point in p) g.DrawRectangle(pen, new Rectangle(point.X - 2, point.Y - 2, 4, 4));
            g.DrawLine(pen, p[0].X, p[0].Y, p[1].X, p[1].Y);
            g.DrawLine(pen, p[2].X, p[2].Y, p[3].X, p[3].Y);
        }
    }
}
