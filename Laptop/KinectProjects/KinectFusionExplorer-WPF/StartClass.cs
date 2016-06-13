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
            var thread = new Thread(() =>
            {
                var mw = new MainWindow();
                _mainWindow = mw;
                mw.Closed += (sender, e) =>
                {
                    ss.Release();
                    alreadyClosed = true;
                };
                mw.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
//            mw.Show();
        }


        public bool SaveToFile(string path)
        {
            return _mainWindow.SaveToPlyFile(path);
        }

        public void Reset()
        {
            _mainWindow.DoResetReconstruction();
        }

        public void Kill()
        {
            _mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(() =>
            {
                _mainWindow.Close();
            }));
        }

        public void WaitUntilClose()
        {
            if (alreadyClosed) return;
            ss.Wait();
        }
    }
}