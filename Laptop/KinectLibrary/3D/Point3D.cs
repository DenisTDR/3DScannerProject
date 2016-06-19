using System;

namespace KinectLibrary._3D

{
    public class Point3D
    {
        private static int contor = 0;
        public Point3D(double x, double y, double z, int index = -1)
        {
            X = x;
            Y = y;
            Z = z;
            if (index == -1)
                Index = contor++;
            else {
                if (contor < index)
                    contor = index + 1;
                Index = index;
            }
        }
        public Point3D(double x, double y) : this(x, y, 0) { }
        public Point3D() : this(0, 0, 0) { }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public int Index { get; set; }

        public Point3D RotateAroundPoint(Point3D p, double angle)
        {
           // Console.WriteLine("rotate" + p + angle);
            Translate(p, true);
            RotateAroundYAxis(angle);
            Translate(p, false);
            return this;
        }

        public void RotateAroundZAxis(double angle)
        {
            angle = angle * Math.PI / 180;
            var sinAngle = Math.Sin(angle);
            var cosAngle = Math.Cos(angle);
            var newX = this.X * cosAngle - this.Y * sinAngle;
            var newY = this.X * sinAngle + this.Y * cosAngle;
            this.X = newX;
            this.Y = newY;
        }
        public void RotateAroundXAxis(double angle)
        {
            angle = angle * Math.PI / 180;
            var sinAngle = Math.Sin(angle);
            var cosAngle = Math.Cos(angle);
            var newZ = this.Z * cosAngle - this.Y * sinAngle;
            var newY = this.Z * sinAngle + this.Y * cosAngle;
            this.Z = newZ;
            this.Y = newY;
        }
        public void RotateAroundYAxis(double angle)
        {
            angle = angle * Math.PI / 180;
            var sinAngle = Math.Sin(angle);
            var cosAngle = Math.Cos(angle);
            var newZ = this.Z * cosAngle + this.X * sinAngle;
            var newX = this.X * cosAngle - this.Z * sinAngle;
            this.Z = newZ;
            this.X = newX;
        }

        public void Translate(Point3D p, bool inv)
        {
            this.X += p.X * (inv ? -1 : 1);
            this.Y += p.Y * (inv ? -1 : 1);
            this.Z += p.Z * (inv ? -1 : 1);
        }

        public override string ToString()
        {
            return $"({(float)this.X} {(float)this.Y} {(float)this.Z})";
        }
        public string ToPlyFormat()
        {
            return $"{(float)this.X} {(float)this.Y} {(float)this.Z}";
        }

        public static Point3D FromLine(string line)
        {
            var splitted = line.Split(' ');
            var p3D = new Point3D
            {
                X = double.Parse(splitted[1]),
                Y = double.Parse(splitted[2]),
                Z = double.Parse(splitted[3])
            };
            return p3D;
        }


        public double DistanceTo(Point3D point3)
        {
            return Math.Sqrt(Math.Pow(X - point3.X, 2) + Math.Pow(Y - point3.Y, 2) + Math.Pow(Z - point3.Z, 2));
        }

        public override bool Equals(Object o)
        {
            Point3D p = (Point3D)o;
            if (p.Index == this.Index)
                return true;
            return false;
        }
    }
}