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
    /// Interaction logic for XkcdMapControl.xaml
    /// </summary>
    public partial class XkcdMapControl : UserControl
    {
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(int), typeof(XkcdMapControl),
                                        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisplayChanged)));

        public int Top
        {
            get { return (int)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(int), typeof(XkcdMapControl),
                                        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisplayChanged)));

        public int Left
        {
            get { return (int)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public static readonly DependencyProperty TileMapProperty =
            DependencyProperty.Register("TileMap", typeof(XkcdTileMap), typeof(XkcdMapControl),
                                        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDisplayChanged)));

        public XkcdTileMap TileMap
        {
            get { return (XkcdTileMap)GetValue(TileMapProperty); }
            set { SetValue(TileMapProperty, value); }
        }

        private static void OnDisplayChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var mapControl = o as XkcdMapControl;
            mapControl.InitImages();
            mapControl.Refresh();
        }

        private class TileImageCouple
        {
            public XkcdTile Tile { get; set; }
            public Image Image { get; private set; }
            public TileImageCouple(XkcdTile tile, Image image)
            {
                Tile = tile;
                Image = image;
            }
        }
        private List<TileImageCouple> _imageCouples = new List<TileImageCouple>();

        public XkcdMapControl()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitImages();
            Refresh();
        }

        public void SetProperties(int top, int left, XkcdTileMap map)
        {
            _refreshing = true;

            this.SetCurrentValue(XkcdMapControl.TopProperty, top);
            this.SetCurrentValue(XkcdMapControl.LeftProperty, left);
            this.SetCurrentValue(XkcdMapControl.TileMapProperty, map);

            _refreshing = false;

            Refresh();

        }

        private bool _refreshing = false;
        private void Refresh()
        {
            if (!_refreshing)
            {
                _refreshing = true;
                var tileMap = this.TileMap;

                var width = (int)this.ActualWidth;
                var height = (int)this.ActualWidth;

                var maxTop = tileMap.NbVertical * XkcdTile.Height - height;
                if (this.Top < 0) this.SetCurrentValue(XkcdMapControl.TopProperty, 0);
                if (this.Top > maxTop) this.SetCurrentValue(XkcdMapControl.TopProperty, maxTop);

                var maxLeft = tileMap.NbHorizontal * XkcdTile.Width - width;
                if (this.Left < 0) this.SetCurrentValue(XkcdMapControl.LeftProperty, 0);
                if (this.Left > maxLeft) this.SetCurrentValue(XkcdMapControl.LeftProperty, maxLeft);

                var top = this.Top;
                var left = this.Left;

                var yMin = top / XkcdTile.Height;
                var yMax = (top + height) / XkcdTile.Height;
                var xMin = left / XkcdTile.Width;
                var xMax = (left + width) / XkcdTile.Width;

                var points = (from x in Enumerable.Range(xMin, xMax - xMin + 1)
                              from y in Enumerable.Range(yMin, yMax - yMin + 1)
                              select new Point(x, y)).ToArray();

                var reusedImages = (from point in points
                                    select tileMap.GetTile(point) into tile
                                    where tile != null
                                    join image in _imageCouples on tile equals image.Tile
                                    select image).ToList();

                var freeImages = new Queue<TileImageCouple>(_imageCouples.Except(reusedImages));


                foreach (var point in points)
                {
                    var tile = tileMap.GetTile(point);
                    if (tile != null)
                    {
                        TileImageCouple imageCouple = reusedImages.FirstOrDefault(tic => tic.Tile == tile);
                        if (imageCouple == null)
                        {
                            imageCouple = freeImages.Dequeue();
                            imageCouple.Tile = tile;
                            imageCouple.Image.Source = XkcdTile.ToImageSource(tile);
                        }

                        Canvas.SetTop(imageCouple.Image, point.Y * XkcdTile.Height - top);
                        Canvas.SetLeft(imageCouple.Image, point.X * XkcdTile.Width - left);

                        imageCouple.Image.Visibility = Visibility.Visible;

                        var curIndex = _imageCouples.IndexOf(imageCouple);
                        _imageCouples[curIndex] = _imageCouples[0];
                        _imageCouples[0] = imageCouple;
                    }
                }

                foreach (var couple in freeImages)
                {
                    couple.Image.Visibility = Visibility.Collapsed;
                }

                _refreshing = false;
            }
        }

        private void InitImages()
        {
            var nbTilesHorizontal = 2 + ((int)this.ActualWidth) / XkcdTile.Width;
            var nbTilesVertical = 2 + ((int)this.ActualHeight) / XkcdTile.Height;
            var nbTiles = nbTilesHorizontal * nbTilesVertical;

            while (_imageCouples.Count < nbTiles)
            {
                var image = new Image()
                {
                    Height = XkcdTile.Height,
                    Width = XkcdTile.Width
                };

                MapCanvas.Children.Add(image);
                _imageCouples.Add(new TileImageCouple(null, image));
            }
        }
    }
}
