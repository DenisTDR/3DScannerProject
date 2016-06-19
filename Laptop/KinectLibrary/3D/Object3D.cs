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
        public bool SkipCheckPoint { get; set; }
        public bool SkipCheckNearCOG { get; set; }

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
            var c = 0;
            while (selected)
            {
                c++;
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

        public void setCOG(Point3D cog)
        {
            centerOfGravity = cog;
        }

        public bool addPointToObject(Point3D point)
        {
            if (SkipCheckPoint || verifyPoint(point))
            {
                upDateCOG(point);
                pointList.Add(point);

                return true;
            }
            return false;
        }

        private bool verifyPoint(Point3D point)
        {
            return (SkipCheckNearCOG || point.DistanceTo(centerOfGravity) <= limit)
                && point.Y > Variables.MinHeight;
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
            this.setCOG(parser.pointList[0]);
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

        public void IgnoreLowerPoints(string filePath, bool saveBack = false)
        {

            var parser = new PlyParser();
            Console.WriteLine("reading from file");
            parser.Read(filePath);
            this.pointList = new List<Point3D>();
            this.faceList = new List<Face>();
            Console.WriteLine("read. processing object");

            this.setCOG(parser.pointList[0]);
            this.SkipCheckNearCOG = true;
            this.addPointsToObject(parser.pointList, parser.faceList);


            parser.pointList.Clear();
            parser.faceList.Clear();
            if (saveBack)
            {

                Console.WriteLine("processed. saving to file");
                parser.LoadFromObject3D(this);

                parser.Write(filePath.Replace(".ply", "_ok.ply"));
                Console.WriteLine("saved");
                parser.pointList.Clear();
                parser.faceList.Clear();
            }
            Console.WriteLine("done ignoring lower points!");
        }

        public Point3D getMaxHeight(string filePath)
        {
            var parser = new PlyParser();
            Console.WriteLine("reading from file");
            parser.Read(filePath);
            parser.faceList.Clear();
            var maxHeightPoint = new Point3D(0, -100, 0);

            foreach (var point3D in parser.pointList)
            {
                if (point3D.Y > maxHeightPoint.Y)
                {
                    maxHeightPoint = point3D;
                }
            }

            parser.pointList.Clear();
            Variables.MinHeight = maxHeightPoint.Y;
            return maxHeightPoint;
        }

        public Point3D RecalculateCenter()
        {
            var tmpCog = new Point3D(0, 0, 0);
            foreach (var point3D in pointList)
            {
                tmpCog.X += point3D.X/pointList.Count;
                tmpCog.Z += point3D.Z/pointList.Count;
            }
            Variables.Center = tmpCog;
            return tmpCog;
        }

        public void LoadFromParser(PlyParser parser)
        {
            this.pointList = parser.pointList;
            this.faceList = parser.faceList;
        }

        public void Dispose()
        {
            this.pointList.Clear();
            this.faceList.Clear();
        }
    }
}