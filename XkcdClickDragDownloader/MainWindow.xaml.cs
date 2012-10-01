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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace XkcdClickDragDownloader
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
            Task.Factory.StartNew(() => DownloadFileSmartly(1))
                .ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => this.IsEnabled = true)));
            this.IsEnabled = false;
        }

        private void DownloadFileSmartly(int startDistance)
        {
            const string imgWebDir = "http://imgs.xkcd.com/clickdrag/";
            const string imgLocalDir = @"c:\temp\xkcd\";

            var webClient = new WebClient();

            int distance = startDistance;
            bool allInError = false;

            while (!allInError)
            {
                allInError = true;

                foreach (var fileName in GetImagesByDistance(distance))
                {
                    UpdateProgress(distance, fileName);
                    if (File.Exists(imgLocalDir + fileName))
                    {
                        allInError = false;
                    }
                    else
                    {
                        try
                        {
                            //http://imgs.xkcd.com/clickdrag/3n2w.png
                            webClient.DownloadFile(imgWebDir + fileName, imgLocalDir + fileName);
                            allInError = false;
                        }
                        catch { }
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }

                distance++;
            }
        }

        private IEnumerable<string> GetImagesByDistance(int distanceFromCenter)
        {
            //left
            for (var i = distanceFromCenter - 1; i > 0; i--)
            {
                yield return string.Format("{0}{1}{2}{3}.png", i, "s", distanceFromCenter, "w");
            }
            for (var i = 1; i < distanceFromCenter; i++)
            {
                yield return string.Format("{0}{1}{2}{3}.png", i, "n", distanceFromCenter, "w");
            }
            yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "n", distanceFromCenter, "w");


            //top
            for (var i = distanceFromCenter - 1; i > 0; i--)
            {
                yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "n", i, "w");
            }
            for (var i = 1; i < distanceFromCenter; i++)
            {
                yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "n", i, "e");
            }
            yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "n", distanceFromCenter, "e");


            //right
            for (var i = distanceFromCenter - 1; i > 0; i--)
            {
                yield return string.Format("{0}{1}{2}{3}.png", i, "n", distanceFromCenter, "e");
            }
            for (var i = 1; i < distanceFromCenter; i++)
            {
                yield return string.Format("{0}{1}{2}{3}.png", i, "s", distanceFromCenter, "e");
            }
            yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "s", distanceFromCenter, "e");


            //bottom
            for (var i = distanceFromCenter - 1; i > 0; i--)
            {
                yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "s", i, "e");
            }
            for (var i = 1; i < distanceFromCenter; i++)
            {
                yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "s", i, "w");
            }
            yield return string.Format("{0}{1}{2}{3}.png", distanceFromCenter, "s", distanceFromCenter, "w");
        }

        private void UpdateProgress(int distance, string fileName)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.iTextBlock.Text = "Distance : " + distance.ToString();
                    this.jTextBlock.Text = "File : " + fileName;
                }
                ));
        }
    }
}
