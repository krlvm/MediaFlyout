using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaFlyout.Utilities
{
    class ImageUtility
    {
        public static bool CreateBitmapImageFromStream(Stream stream, out BitmapSource bitmap)
        {
            bitmap = null;

            if (stream != null && stream.Length > 0)
            {
                stream.Seek(0, SeekOrigin.Current);
                bitmap = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return true;
            }

            return false;
        }
    }
}
