using System.Collections.Generic;

namespace KinectLibrary._3D

{
    public class Face
    {
        public List<Point3D> Points { get; set; } = new List<Point3D>();
        
        public Face(List<Point3D> facePointList)
        {
            Points.AddRange(facePointList);
        }

        public Face(Point3D p1, Point3D p2, Point3D p3)
        {
            Points.Add(p1);
            Points.Add(p2);
            Points.Add(p3);
        }

        public override string ToString()
        { 
            return $"3 {Points[0].Index} {Points[1].Index} {Points[2].Index}";
        }
    }
}