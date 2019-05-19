using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ftp.@interface;

namespace FtpClient.controller
{
    /// <summary>
    /// FileItem.xaml 的交互逻辑
    /// </summary>
    public partial class FileItem : ListBoxItem
    {
        public static Bitmap DirIcon = Properties.Resources.dir;
        public static Bitmap NormalIcon = Properties.Resources.normal;
        public VisualFile File { get; set; }
        public FileItem(VisualFile file)
        {
            InitializeComponent();
            this.File = file;
            this.FileTypeImage.Source = file.FileType == VisualFile.FType.Directory ? BitmapToBitmapImage(DirIcon) : BitmapToBitmapImage(NormalIcon);
            this.AccessTimeLabel.Content = File.Time;
            this.FileNameLabel.Content = File.Filename;
        }

        private static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        
    }
}
