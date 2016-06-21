using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectLibrary._3D;
using Newtonsoft.Json;

namespace KinectLibrary
{
    public static class Variables
    {
        private static Settings _settings = null;
        private static Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = ReadSettingsFromFile();
                }
                return _settings;
            }
        }

        public static Point3D Center
        {
            get { return Settings.Center; }
            set { Settings.Center = value; SaveSettings(); }
        }
        public static int Steps
        {
            get { return Settings.Steps; }
            set { Settings.Steps = value; SaveSettings(); }
        }

        public static double MinHeight
        {
            get { return Settings.MinHeight; }
            set { Settings.MinHeight = value; SaveSettings(); }
        }

        public static string TcpServerIp
        {
            get { return Settings.TcpServerIp; }
            set { Settings.TcpServerIp = value;
                SaveSettings();
            }
        }

        private static Settings ReadSettingsFromFile()
        {
            Settings settings = null;
            if (File.Exists(settingsPath))
            {
                using (var sr = new StreamReader(settingsPath))
                {
                    var allText = sr.ReadLine();
                    if (!string.IsNullOrEmpty(allText))
                    {
                        settings = JsonConvert.DeserializeObject<Settings>(allText);
                        Console.WriteLine("loaded settings: " + JsonConvert.SerializeObject(settings));
                    }
                }
            }

            if (settings == null)
            {
                settings = new Settings();
                settings.Center = new Point3D(0.08665755, 0, -0.669247);
                settings.MinHeight = -0.007814171;
                settings.Steps = 10;
                settings.TcpServerIp = "192.169.0.123";
                _settings = settings;
                SaveSettings();
            }
            return settings;
        }

        private static void SaveSettings()
        {
            using (var sw = new StreamWriter(settingsPath))
            {
                sw.WriteLine(JsonConvert.SerializeObject(_settings));
            }
            Console.WriteLine("saved settings: " + JsonConvert.SerializeObject(_settings));
        }

        private static string settingsPath = "settings.json";
    }

    public class Settings
    {
        public Point3D Center { get; set; }
        public double MinHeight { get; set; }
        public int Steps { get; set; }
        public string TcpServerIp { get; set; }
    }
}
