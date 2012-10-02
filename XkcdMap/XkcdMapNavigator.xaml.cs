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

namespace XkcdMap
{
    /// <summary>
    /// Interaction logic for MapNavigator.xaml
    /// </summary>
    public partial class XkcdMapNavigator : UserControl
    {
        private List<XkcdTileMap> _mapsByZoomLevel;
        private sbyte _zoomLevel = 0;
        private bool _moving = false;
        private Point _mapOrigin;
        private Point _mouseOrigin;

        public XkcdMapNavigator()
        {
            InitializeComponent();

            _mapsByZoomLevel = Enumerable.Range(0, 7).Select(i => new XkcdTileMap(i)).ToList();

            MapControl.TileMap = new XkcdTileMap(0);
            MapControl.Top = 25720;
            MapControl.Left = 67644;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Capture(this))
            {
                _moving = true;
                _mapOrigin = new Point(MapControl.Left, MapControl.Top);
                _mouseOrigin = Mouse.GetPosition(this);
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_moving)
            {
                _moving = false;
                Mouse.Capture(null);
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_moving)
            {
                var mousePos = Mouse.GetPosition(this);
                var movingVector = new Vector(_mouseOrigin.X - mousePos.X, _mouseOrigin.Y - mousePos.Y);
                this.MapControl.Top = (int)(_mapOrigin.Y + movingVector.Y);
                this.MapControl.Left = (int)(_mapOrigin.X + movingVector.X);

                DebugLabel.Content = string.Format("Top : {0}  ; Left : {1}", this.MapControl.Top, this.MapControl.Left);
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ZoomIn(GetCenter());
            }
            else
            {
                ZoomOut(GetCenter());
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ZoomIn(GetMousePosOnMap());
        }

        private Point GetCenter()
        {
            return new Point(MapControl.Left + this.ActualWidth / 2,
                             MapControl.Top + this.ActualHeight / 2);
        }

        private Point GetMousePosOnMap()
        {
            var mousePos = Mouse.GetPosition(this);

            return new Point(MapControl.Left + mousePos.X,
                             MapControl.Top + mousePos.Y);
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ZoomOut(GetCenter());
        }

        private void ZoomIn(Point center)
        {
            if (_zoomLevel > 0)
            {
                GenericZoom(-1, new Point(center.X * 2, center.Y * 2));
            }
        }

        private void ZoomOut(Point center)
        {
            if (_zoomLevel < (_mapsByZoomLevel.Count - 1))
            {
                GenericZoom(1, new Point(center.X / 2, center.Y / 2));
            }
        }

        private void GenericZoom(sbyte delta, Point newCenter)
        {
            _zoomLevel += delta;

            MapControl.SetProperties((int)(newCenter.Y - this.ActualHeight / 2),
                                     (int)(newCenter.X - this.ActualWidth / 2),
                                     _mapsByZoomLevel[_zoomLevel]);
        }
    }
}
