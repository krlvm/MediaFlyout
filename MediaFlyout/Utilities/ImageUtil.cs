using System.IO;
using System.Windows.Media.Imaging;

namespace MediaFlyout.Utilities
{
    class ImageUtil
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
