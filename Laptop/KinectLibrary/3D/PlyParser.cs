using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KinectLibrary._3D

{
    public class PlyParser
    {
        public List<Point3D> pointList { get; set; }
        public List<Face> faceList { get; set; }
        private List<string> list1 = new List<string>();
        private bool isColored = true;

        private string file;

        public PlyParser()
        {
            pointList = new List<Point3D>();
            faceList = new List<Face>();
        }

        public void Read(string input)
        {
            file = input;
            pointList.Clear();
            faceList.Clear();
            int vertex, faces;
            string line;
            using (var sr = new StreamReader(input))
            {
                line = skipLines(sr, "ply", list1);
                list1.Add(line);
                line = skipLines(sr, "comment", list1);
                vertex = int.Parse(line.Split(' ')[2]);
                line = skipLines(sr, "property", list1);
                faces = int.Parse(line.Split(' ')[2]);
                line = skipLines(sr, "property", list1);
                list1.Add(line);

                for (var i = 0; i < vertex; i++)
                {
                    line = sr.ReadLine();
                    var splitted = line.Split(' ');
                    Point3D point;
                    if (splitted.Length == 6)
                    {
                        
                        point = new ColoredPoint(
                            double.Parse(splitted[0]), double.Parse(splitted[1]), double.Parse(splitted[2]),
                            int.Parse(splitted[3]), int.Parse(splitted[4]), int.Parse(splitted[5]), i);
                       
                    }
                    else
                    {
                        isColored = false;
                        point = new Point3D(double.Parse(splitted[0]), double.Parse(splitted[1]), double.Parse(splitted[2]),i);
                    }
                    pointList.Add(point);

                }

                for (int i = 0; i < faces; i++)
                {
                    var splitted = sr.ReadLine().Split(' ');

                    var face = new Face(pointList[int.Parse(splitted[1])],
                        pointList[int.Parse(splitted[2])],
                        pointList[int.Parse(splitted[3])]);
                    faceList.Add(face);
                }
            }

        }

        public void Write(string output)
        {
            using (var sw = new StreamWriter(output))
            {
                
                list1.Take(3).ToList().ForEach(line => sw.WriteLine(line));
                sw.WriteLine("element vertex " + pointList.Count());
                int skip;
                if (isColored)
                {
                    list1.Skip(3).Take(6).ToList().ForEach(line => sw.WriteLine(line));
                    skip = 9;
                }
                else {
                    list1.Skip(3).Take(3).ToList().ForEach(line => sw.WriteLine(line));
                    skip = 6;
                }
                sw.WriteLine("element face " + faceList.Count());

                list1.Skip(skip).Take(2).ToList().ForEach(line => sw.WriteLine(line));



                foreach (var coloredPoint in pointList)
                {
                    sw.WriteLine(coloredPoint.ToString());
                }
                foreach (var face in faceList)
                {
                    sw.WriteLine(face.ToString());
                }
            }
        }

        public void Save()
        {
            var path = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + "-ok.ply";
            this.Write(path);
        }

        private string skipLines(StreamReader sr, string wordToSkip, List<string> list1)
        {
            string line = sr.ReadLine();
            while (line.Contains(wordToSkip))
            {
                list1.Add(line);
                line = sr.ReadLine();

            }
            return line;
        }


    }
}
