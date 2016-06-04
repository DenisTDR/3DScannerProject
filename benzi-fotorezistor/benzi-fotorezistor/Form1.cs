using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace benzi_fotorezistor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void doIt(int angle)
        {
            var size = new Size(10000, 10000);
            var list = GetTriangles(angle, size, size.Width/2);

            var img = new Bitmap(size.Width, size.Height);
            var g = Graphics.FromImage(img);

            var brush1 = new SolidBrush(Color.Black);
            var brush2 = new SolidBrush(Color.White);
            var flag = false;
            foreach (var triangle in list)
            {
                if(flag)
                g.FillPolygon(brush1, triangle.GetPoints);
                flag = !flag;
            }
            g.Dispose();
            img.Save("top/output_" + angle + ".png", ImageFormat.Png);
            img.Dispose();
        }

        private List<Triangle> GetTriangles(int angle, Size size, int range)
        {
            if (360%angle != 0)
            {
                throw new Exception("nope!");
            }
            var list = new List<Triangle>();
            var mid = new PointF(size.Width/2, size.Height/2);
            int cnt = 360/angle;

            var lastPoint = getPoint(0, angle, range, mid);
            for (int i = 1; i <= cnt; i++)
            {
                var newPoint = getPoint(i, angle, range, mid);
                list.Add(new Triangle()
                {
                    P1 = newPoint,
                    P2 = lastPoint,
                    P3 = mid
                });
                lastPoint = newPoint;
            }

            return list;
        }

        private PointF getPoint(int i, int angle, int range, PointF mid)
        {
            return new PointF(
              (float)(mid.X + range * Math.Cos(i * angle * Math.PI / 180)),
                (float)(mid.Y + range * Math.Sin(i * angle * Math.PI / 180)));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 4; i < 11; i++)
            {
                if (360%i == 0 && 360/i%2 == 0)
                    doIt(i);
            }
            MessageBox.Show("done!");
        }
    }
}
