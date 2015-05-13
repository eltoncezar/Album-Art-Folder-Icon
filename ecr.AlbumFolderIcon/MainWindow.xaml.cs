using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ecr.AlbumFolderIcon.Core;
using ecr.AlbumFolderIcon.Utils;

namespace ecr.AlbumFolderIcon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StringCollection log = new StringCollection();

        public MainWindow()
        {
            InitializeComponent();

            rtbxErrors.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            rtbxErrors.Document.PageWidth = 1000;
        }

        private void btnSearchPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                txtPath.Text = dialog.SelectedPath;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Insert validation here

            if (!Directory.Exists(txtPath.Text))
            {
                System.Windows.Forms.MessageBox.Show("Directory doesn't exist.");
                return;
            }
            Main(txtPath.Text);
        }

        public void Main(string path)
        {
            DirectoryInfo rootDir = new DirectoryInfo(path);
            WalkDirectoryTree(rootDir);

            // Write out all the files that could not be processed.
            rtbxErrors.AppendText("\r\r Files with error: \r");
            foreach (string s in log)
            {
                rtbxErrors.AppendText(s + "\r");
            }
            rtbxErrors.ScrollToEnd();
        }

        public void WalkDirectoryTree(DirectoryInfo root)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            string folderPath = root.FullName + "\\" + txbFileName.Text + txbFileExtension.Text;

            if (File.Exists(folderPath))
            {
                // Update the folder attributes
                DirectoryInfo dir = new DirectoryInfo(root.FullName);
                dir.Attributes = dir.Attributes | FileAttributes.System;

                CreateIcon(folderPath);
                CreateDesktopIni(root.FullName);

                rtbxErrors.AppendText(root.FullName + "\r");
            }

            if (ckbIncludeSubfolders.IsChecked == true)
            {
                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }

        public void CreateIcon(string path)
        {
            FileInfo file = new FileInfo(path);
            string fileName = file.DirectoryName + "\\folder.ico";

            if (!File.Exists(fileName))
            {
                try
                {
                    System.Drawing.Image src = System.Drawing.Image.FromFile(path);

                    ImageOrientation orientation = src.Height < src.Width ? ImageOrientation.Landscape : ImageOrientation.Portrait;
                    int size = orientation == ImageOrientation.Landscape ? src.Height : src.Width;
                    int sizeToCrop = orientation == ImageOrientation.Landscape ? src.Width - src.Height : src.Height - src.Width;
                    System.Drawing.Image croppedImage = ImageUtils.CropImage(src, size, size, (orientation == ImageOrientation.Landscape ? sizeToCrop / 2 : 0), (orientation == ImageOrientation.Portrait ? sizeToCrop / 2 : 0));
                    System.Drawing.Image resizedImage = ImageUtils.ResizeImage(256, 256, croppedImage);

                    Icon icon = System.Drawing.Icon.FromHandle(new Bitmap(resizedImage).GetHicon());
                    IconEx Iconex = new IconEx(icon);
                    Iconex.Items.RemoveAt(0);

                    // 256x256
                    IconDeviceImage iconDeviceImage = new IconDeviceImage(new System.Drawing.Size(256, 256), System.Windows.Forms.ColorDepth.Depth32Bit);
                    iconDeviceImage.IconImage = new Bitmap(ImageUtils.ResizeImage(256, 256, resizedImage));
                    Iconex.Items.Add(iconDeviceImage);

                    // 48x48
                    iconDeviceImage = new IconDeviceImage(new System.Drawing.Size(48, 48), System.Windows.Forms.ColorDepth.Depth32Bit);
                    iconDeviceImage.IconImage = new Bitmap(ImageUtils.ResizeImage(48, 48, resizedImage));
                    Iconex.Items.Add(iconDeviceImage);

                    // 32x32
                    iconDeviceImage = new IconDeviceImage(new System.Drawing.Size(32, 32), System.Windows.Forms.ColorDepth.Depth32Bit);
                    iconDeviceImage.IconImage = new Bitmap(ImageUtils.ResizeImage(32, 32, resizedImage));
                    Iconex.Items.Add(iconDeviceImage);

                    // 24x24
                    iconDeviceImage = new IconDeviceImage(new System.Drawing.Size(24, 24), System.Windows.Forms.ColorDepth.Depth32Bit);
                    iconDeviceImage.IconImage = new Bitmap(ImageUtils.ResizeImage(24, 24, resizedImage));
                    Iconex.Items.Add(iconDeviceImage);

                    // 16x16
                    iconDeviceImage = new IconDeviceImage(new System.Drawing.Size(16, 16), System.Windows.Forms.ColorDepth.Depth32Bit);
                    iconDeviceImage.IconImage = new Bitmap(ImageUtils.ResizeImage(16, 16, resizedImage));
                    Iconex.Items.Add(iconDeviceImage);
                    Iconex.Save(fileName);

                    // "Hide" the folder.ico
                    File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.Hidden);

                    //imgRect.Source = ImageUtils.ConvertImageToBitmapImage(resizedImage);
                }
                catch (Exception)
                {
                    log.Add(path);
                }
            }
        }

        public void CreateDesktopIni(string path)
        {
            string fileName = path + "\\Desktop.ini";

            if (!File.Exists(fileName))
            {
                try
                {
                    StringBuilder str = new StringBuilder();
                    str.AppendLine("[.ShellClassInfo]");
                    str.AppendLine("IconResource=folder.ico,0");

                    File.WriteAllText(fileName, str.ToString());

                    // "Hide" the desktop.ini
                    File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.Hidden);
                }
                catch (Exception)
                {
                    log.Add(path);
                }
            }
        }
    }

    public enum ImageOrientation
    {
        Landscape,
        Portrait
    }
}