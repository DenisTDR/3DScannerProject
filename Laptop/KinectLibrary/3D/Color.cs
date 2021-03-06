namespace KinectLibrary._3D
{
    class Color
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Color()
        {

        }
        public Color(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
        public override string ToString()
        {
            return $"{R} {G} {B}";
        }
    }
}
