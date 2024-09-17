using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool TrimImagePng(string filePath)
        {
            LogInfo($"Trimming png image");

            Image trimmedImage = TrimImage(filePath);
            trimmedImage.Save(filePath, ImageFormat.Png);
            trimmedImage.Dispose();

            LogInfo($"Sucessfully trimmed png");
            return true;
        }

        private Image TrimImage(string imagePath)
        {
            Bitmap originalImage = new Bitmap(imagePath);
            Rectangle cropRect = GetImageBounds(originalImage);
            Bitmap trimmedImage = CropImage(originalImage, cropRect);
            originalImage.Dispose();
            return trimmedImage;
        }    

        private Rectangle GetImageBounds(Bitmap img)
        {
            int x1 = img.Width, x2 = 0, y1 = img.Height, y2 = 0;
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color pixel = img.GetPixel(x, y);
                    if (pixel.A != 0)
                    {
                        if (x < x1)
                            x1 = x;
                        if (x > x2)
                            x2 = x;
                        if (y < y1)
                            y1 = y;
                        if (y > y2)
                            y2 = y;
                    }
                }
            }

            if (x1 > x2 || y1 > y2) 
                return new Rectangle(0, 0, img.Width, img.Height);
            return new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
        }

        private Bitmap CropImage(Bitmap img, Rectangle cropArea)
        {
            return img.Clone(cropArea, img.PixelFormat);
        }


    }
}