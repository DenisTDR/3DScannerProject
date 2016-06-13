namespace KinectLibrary._3D
{
    class ColoredPoint : Point3D
    {
        public Color color { get; set; }

        public ColoredPoint(double x, double y, double z, int r , int g , int b , int index = 0) : base(x, y, z, index)
        {
           color = new Color(r, g, b);
        }

        public ColoredPoint(double x, double y, double z, Color c, int index = 0) : base(x, y, z, index)
        {
            color = c;
        }

        public override string ToString()
        {
            return $"{this.ToPlyFormat()} {color}";
        }
    }
}
