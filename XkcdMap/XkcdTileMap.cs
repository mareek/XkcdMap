using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Threading;

namespace XkcdMap
{
    public class XkcdTileMap
    {
        private DirectoryInfo _tilesDir;
        private int _zoomLevel;

        public List<XkcdTile> Tiles { get; private set; }
        private Dictionary<Point, XkcdTile> _tilesByCoords;

        public int NbHorizontal { get; private set; }
        public int NbVertical { get; private set; }

        public XkcdTileMap(int zoomLevel)
        {
            _zoomLevel = zoomLevel;
            _tilesDir = new DirectoryInfo(Path.Combine(App.BaseDir.FullName, _zoomLevel.ToString()));
            Tiles = _tilesDir.GetFiles().Select(f => new XkcdTile(f)).ToList();
            _tilesByCoords = Tiles.ToDictionary(t => new Point(t.X, t.Y));
            
            NbHorizontal = Tiles.Max(t => t.X) + 1;
            NbVertical = Tiles.Max(t => t.Y) + 1;
        }

        public XkcdTile GetTile(Point point)
        {
            return GetTile((int)point.X, (int)point.Y);
        }

        public XkcdTile GetTile(int x, int y)
        {
            XkcdTile result;
            if (!_tilesByCoords.TryGetValue(new Point(x, y), out result))
            {
                result = null;
            }
            return result;
        }

        public bool CreateNextZoomLevel(long WorkingSetLimit = long.MaxValue)
        {
            var destDir = new DirectoryInfo(Path.Combine(App.BaseDir.FullName, (_zoomLevel + 1).ToString()));
            destDir.Create();

            var xMax = Tiles.Max(t => t.X);
            var xMin = Tiles.Min(t => t.X);
            var yMax = Tiles.Max(t => t.Y);
            var yMin = Tiles.Min(t => t.Y);

            for (var x = xMin; x <= xMax; x += 2)
            {
                for (var y = yMin; y <= yMax; y += 2)
                {
                    var destFile = new FileInfo(Path.Combine(destDir.FullName,
                                                             (x / 2).ToString() + "-" + (y / 2).ToString() + ".png"));
                    if (!destFile.Exists)
                    {
                        MergeTiles(destFile, GetTile(x, y), GetTile(x + 1, y), GetTile(x, y + 1), GetTile(x + 1, y + 1));
                        if (Environment.WorkingSet > WorkingSetLimit)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void MergeTiles(FileInfo targetFile, XkcdTile topLeftTile, XkcdTile topRightTile, XkcdTile bottomLeftTile, XkcdTile bottomRightTile)
        {
            if (topLeftTile != null
                || topRightTile != null
                || bottomLeftTile != null
                || bottomRightTile != null)
            {
                Func<XkcdTile, Image> TileToImage = tile => new Image { Source = XkcdTile.ToImageSource(tile) };

                var grid = new UniformGrid { Rows = 2, Columns = 2 };
                grid.Children.Add(TileToImage(topLeftTile));
                grid.Children.Add(TileToImage(topRightTile));
                grid.Children.Add(TileToImage(bottomLeftTile));
                grid.Children.Add(TileToImage(bottomRightTile));

                var viewBox = new Viewbox();
                viewBox.Child = grid;

                viewBox.Measure(new System.Windows.Size(XkcdTile.Width, XkcdTile.Height));
                viewBox.Arrange(new Rect(new System.Windows.Size(XkcdTile.Width, XkcdTile.Height)));

                var renderTarget = new RenderTargetBitmap(XkcdTile.Width, XkcdTile.Height, 96, 96, PixelFormats.Default);
                renderTarget.Render(viewBox);

                var frame = BitmapFrame.Create(renderTarget);
                var encoder = new PngBitmapEncoder { Frames = new[] { frame } };

                using (var fileStream = targetFile.Create())
                {
                    encoder.Save(fileStream);
                }
            }
        }
    }
}
