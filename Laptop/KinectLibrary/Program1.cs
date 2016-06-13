using System;
using System.Collections.Generic;
using System.Linq;
using KinectLibrary._3D;

namespace KinectLibrary
{
    public class Program1
    {
        public static void Main(string[] args)
        {
            PlyParser pp = new PlyParser();
            var path = @"D:\Projects\OC\test\";

            pp.Read(path + "1.ply");

            var center = new Point3D(0.125571, -0.02198797, -0.6631655);
            Object3D obj = new Object3D(new List<Point3D>(), new List<Face>(), center);
            
//            obj.rotateObject(center, 25);

            for (var i = 1; i <= 10; i++)
            {
                Console.WriteLine("reading " + i);
                pp.Read(path + i + ".ply");
                Console.WriteLine("creating obj");
                var objCrt = new Object3D(pp.pointList, pp.faceList, center);
                Console.WriteLine("rotating ");
                objCrt.rotateObject(center, 36);
                Console.WriteLine("concat");
                obj.concatenate(objCrt);
            }

            //var highest = obj.pointList.Max(p => p.Y);
            //-0.08506382
            //            pp.Read("D:/kinect/projekt/part2.ply");
            //            Object3D obj2 = new Object3D(pp.pointList, pp.faceList, pp.pointList[0]);
            //            var cog = obj2.getCOG();
            //            obj2.rotateObject(cog,-45);
            //            obj.concatenate(obj2);
            pp.pointList = obj.pointList;
            pp.faceList = obj.faceList;

            //var z = pp.pointList.Sum(p => p.Z)/pp.pointList.Count;

            pp.Write(path + "out.ply");
        }
    }
}