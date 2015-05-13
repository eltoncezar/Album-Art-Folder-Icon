//http://jasonjano.wordpress.com/2010/02/13/image-resizing-and-cropping-in-c/
//http://tech.pro/tutorial/620/csharp-tutorial-image-editing-saving-cropping-and-resizing

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ecr.AlbumFolderIcon.Utils
{
    public class ImageUtils
    {
        #region Cropping

        //Overload for crop that default starts top left of the image.
        public static System.Drawing.Image CropImage(System.Drawing.Image Image, int Height, int Width)
        {
            return CropImage(Image, Height, Width, 0, 0);
        }

        //The crop image sub
        public static System.Drawing.Image CropImage(System.Drawing.Image Image, int Height, int Width, int StartAtX, int StartAtY)
        {
            Image outimage;
            MemoryStream mm = null;
            try
            {
                //check the image height against our desired image height
                if (Image.Height < Height)
                {
                    Height = Image.Height;
                }
                if (Image.Width < Width)
                {
                    Width = Image.Width;
                }

                //create a bitmap window for cropping
                Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(72, 72);

                //create a new graphics object from our image and set properties
                Graphics grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;

                //now do the crop
                grPhoto.DrawImage(Image, new Rectangle(0, 0, Width, Height), StartAtX, StartAtY, Width, Height, GraphicsUnit.Pixel);

                // Save out to memory and get an image from it to send back out the method.
                mm = new MemoryStream();
                bmPhoto.Save(mm, System.Drawing.Imaging.ImageFormat.Jpeg);
                Image.Dispose();
                bmPhoto.Dispose();
                grPhoto.Dispose();
                outimage = Image.FromStream(mm);
                return outimage;
            }
            catch (Exception ex)
            {
                throw new Exception("Error cropping image, the error was: " + ex.Message);
            }
        }

        #endregion Cropping

        #region Resizing

        //Hard resize attempts to resize as close as it can to the desired size and then crops the excess
        public static System.Drawing.Image HardResizeImage(int Width, int Height, System.Drawing.Image Image)
        {
            int width = Image.Width;
            int height = Image.Height;
            Image resized = null;
            if (Width > Height)
            {
                resized = ResizeImage(Width, Width, Image);
            }
            else
            {
                resized = ResizeImage(Height, Height, Image);
            }
            Image output = CropImage(resized, Height, Width);

            //return the original resized image
            return output;
        }

        //Image resizing
        public static System.Drawing.Image ResizeImage(int maxWidth, int maxHeight, System.Drawing.Image Image)
        {
            int width = Image.Width;
            int height = Image.Height;
            //if (width > maxWidth || height > maxHeight)
            //{
            //The flips are in here to prevent any embedded image thumbnails -- usually from cameras
            //from displaying as the thumbnail image later, in other words, we want a clean
            //resize, not a grainy one.
            Image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
            Image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
            float ratio = 0;
            if (width > height)
            {
                ratio = (float)width / (float)height;
                width = maxWidth;
                height = Convert.ToInt32(Math.Round((float)width / ratio));
            }
            else
            {
                ratio = (float)height / (float)width;
                height = maxHeight;
                width = Convert.ToInt32(Math.Round((float)height / ratio));
            }
            //return the resized image
            return Image.GetThumbnailImage(width, height, null, IntPtr.Zero);
            //}
            //return the original resized image
            //return Image;
        }

        #endregion Resizing

        #region Converters

        public static BitmapImage ConvertImageToBitmapImage(Image img)
        {
            // Convert the image in resources to a Stream
            Stream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);

            // Create a BitmapImage with the Stream.
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            return bitmap;
        }

        public static BitmapImage ConvertIconToBitmapImage(Icon icon)
        {
            // Convert the image in resources to a Stream
            Stream ms = new MemoryStream();
            icon.Save(ms);

            // Create a BitmapImage with the Stream.
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            return bitmap;
        }

        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        #endregion Converters
    }
}