using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace XkcdMap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string MergeCommand = "Merge";
        public static DirectoryInfo BaseDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "XkcdClickAndDrag"));
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            int zoomLevel;
            if (e.Args.Length == 2
                && e.Args[0] == MergeCommand
                && int.TryParse(e.Args[1], out zoomLevel))
            {
                if (new XkcdTileMap(zoomLevel).CreateNextZoomLevel(500000000))
                {
                    Environment.Exit(0);
                }
                else 
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                new MainWindow().Show();
            }

        }
    }
}
