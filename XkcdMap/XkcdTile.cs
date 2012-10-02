using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;

namespace XkcdMap
{
    public class XkcdTile
    {
        public const int Width = 2048;
        public const int Height = 2048;

        public string ImagePath { get; private set; }
        public FileInfo File { get { return new FileInfo(ImagePath); } }

        public int X { get; private set; }
        public int Y { get; private set; }

        public XkcdTile(FileInfo tileFile)
        {
            ImagePath = tileFile.FullName;

            var cleanName = tileFile.Name.Replace(tileFile.Extension, "");
            var coords = cleanName.Split('-');
            X = int.Parse(coords[0]);
            Y = int.Parse(coords[1]);
        }

        public static ImageSource ToImageSource(XkcdTile tile)
        {
            var converter = new ImageSourceConverter();
            return tile == null ? null : (ImageSource)converter.ConvertFromString(tile.ImagePath);
        }
    }
}
