using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace benzi_fotorezistor
{
    class Triangle
    {
        public PointF P1 { get; set; }

        public PointF P2 { get; set; }
        public PointF P3 { get; set; }

        public PointF[] GetPoints => new PointF[] {P1, P2, P3};
    }
}
