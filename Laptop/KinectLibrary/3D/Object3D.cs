using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectLibrary._3D

{
    public class Object3D
    {
        public List<Point3D> pointList { get; set; }
        public List<Face> faceList { get; set; }
        private Point3D centerOfGravity;
        private double limit = 0.5;


        public Object3D(List<Point3D> pL, List<Face> fL, Point3D COG, double lim = 0.5)
        {
            pointList = new List<Point3D>();
            faceList = new List<Face>();

            centerOfGravity = new Point3D (COG.X,COG.Y,COG.Z,0);
            limit = lim;
            addPointsToObject(pL, fL);

        }

        public Object3D()
        {
            pointList = new List<Point3D>();
            faceList = new List<Face>();
        }

        public void addPointsToObject(List<Point3D> pL, List<Face> fL)
        {
            bool selected = true;
            while (selected)
            {
                selected = false;
                for (int  i = 0; i < pL.Count(); i++)
                {
                    if (addPointToObject(pL[i]))
                    {
                        selected = true;
                        pL.Remove(pL[i]);
                    }
        
                }
            }


            for (int i = 0; i < fL.Count(); i++)
            {
                selected = true;
                foreach (Point3D point in fL[i].Points)
                {
                    if (!pointList.Contains(point))
                        selected = false;
                }
                if (selected)
                {
                    faceList.Add(fL[i]);
                }
                else
                {
                    foreach (Point3D p in fL[i].Points)
                    {
                        pointList.Remove(p);
                    }
                }
            }

            reindex();

        }

        public Point3D getCOG()
        {
            return centerOfGravity;
        }

        public bool addPointToObject(Point3D point)
        {
            if (verifyPoint(point))
            {
                upDateCOG(point);
                pointList.Add(point);

                return true;
            }
            return false;
        }

        private bool verifyPoint(Point3D point)
        {
            return point.DistanceTo(centerOfGravity) <= limit
                && point.Y > -0.08206382;
            
        }

        public void upDateCOG(Point3D point)
        {
            if (pointList.Count == 0)
            {
                centerOfGravity.X = point.X;
                centerOfGravity.Y = point.Y;
                centerOfGravity.Z = point.Z;

                return;
            }
            centerOfGravity.X = (centerOfGravity.X * pointList.Count() + point.X) / (pointList.Count() + 1);
            centerOfGravity.Y = (centerOfGravity.Y * pointList.Count() + point.Y) / (pointList.Count() + 1);
            centerOfGravity.Z = (centerOfGravity.Z * pointList.Count() + point.Z) / (pointList.Count() + 1);


        }

        public void rotateObject(Point3D point, double angle)
        {
            for(int i = 0; i < pointList.Count();i++)
            {
                pointList[i].RotateAroundPoint(point, angle);
            }
        }

        public void reindex()
        {
            int cnt = 0;
            for (int i = 0; i < pointList.Count(); i++)
            {
                pointList[i].Index = i;
            }
        }

        public void concatenate(Object3D obj)
        {
            int offset = this.pointList.Count();
            for (int i = 0; i < obj.pointList.Count(); i++)
            {
                obj.pointList[i].Index += offset;
                this.pointList.Add(obj.pointList[i]);
            }
            for (int i = 0; i < obj.faceList.Count(); i++)
            {
                this.faceList.Add(obj.faceList[i]);
            }
        }

        public void MakeRotateAndSaveObject(PlyParser parser, Point3D center, double angle)
        {
            Console.WriteLine("adding points");
            this.addPointsToObject(parser.pointList, parser.faceList);
            Console.WriteLine("rotating obj");
            this.rotateObject(center, angle);
            parser.pointList = this.pointList;
            parser.faceList = this.faceList;
            Console.WriteLine("saving st");
            parser.Save();
            Console.WriteLine("saved st");
        }
    }
}