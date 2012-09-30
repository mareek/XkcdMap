using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace XkcdMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EnterWaitMode();
            var chrono = Stopwatch.StartNew();
            GenerateZoomLevelsByChildProcesses().ContinueWith(t =>
                {
                    chrono.Stop();
                    ExitWaitMode(chrono.Elapsed.ToString());
                });
        }

        private void EnterWaitMode()
        {
            PerformOnUIBlocking(() =>
            {
                this.IsEnabled = false;
            });
        }

        private void ExitWaitMode(string message)
        {
            PerformOnUI(() =>
            {
                this.IsEnabled = true;
                MessageBox.Show(this, message);
            });
        }

        private void PerformOnUI(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        private void PerformOnUIBlocking(Action action)
        {
            Dispatcher.Invoke(action);
        }

        private Task GenerateZoomLevelsByChildProcesses()
        {
            return Task.Factory.StartNew(() =>
                {
                    var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    for (int zoomLevel = 0; zoomLevel < 10; zoomLevel++)
                    {
                        bool succeeded = false;
                        while (!succeeded)
                        {
                            var cmdLineArguments = string.Join(" ", App.MergeCommand, zoomLevel);
                            var childProcess = Process.Start(exePath, cmdLineArguments);
                            childProcess.WaitForExit();
                            succeeded = childProcess.ExitCode == 0;
                        }
                    }
                });
        }
    }
}
