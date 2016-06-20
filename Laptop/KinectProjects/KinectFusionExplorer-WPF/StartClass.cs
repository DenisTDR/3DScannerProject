using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace Microsoft.Samples.Kinect.KinectFusionExplorer
{
    public class StartClass
    {
        private SemaphoreSlim ss = new SemaphoreSlim(0);
        private bool alreadyClosed = false;
        private MainWindow _mainWindow;
        public void OpenWindow()
        {
            if(_mainWindow != null)
                return;
            alreadyClosed = false;
            var thread = new Thread(() =>
            {
                _mainWindow = new MainWindow();
                _mainWindow.Closed += (sender, e) =>
                {
                    Console.WriteLine("MainWindow Closed!");
                    alreadyClosed = true;
                    ss.Release();
                };
                _mainWindow.ShowDialog();
                _mainWindow = null;
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

        }


        public bool SaveToFile(string path)
        {
            return _mainWindow.SaveToPlyFile(path);
        }

        public void Reset()
        {
            Console.WriteLine("Reset reconstruction!");
            _mainWindow.DoResetReconstruction();
        }

        public void Kill()
        {
            if (alreadyClosed) return;
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(() =>
            {
                _mainWindow.Close();
            }));
        }

        public void WaitUntilClose()
        {
            if (!alreadyClosed)
                ss.Wait();
        }
    }
}