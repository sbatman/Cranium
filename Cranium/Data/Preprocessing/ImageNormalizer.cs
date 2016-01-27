using System;

namespace Cranium.Lib.Data.Preprocessing
{
    public static class ImageNormalizer
    {
        public class Image
        {
            public Byte[] Data;
            public Int32 Width;
            public Int32 Height;
            public String FileName;
            public String Tag;
        }

        public class PreProcessedImage : Image
        {
            public Byte[] BWMap;
            public Int32 ProcessedWidth;
            public Int32 ProcessedHeight;
        }

        public static PreProcessedImage ProcessImage(Image input, Int32 targetWidth, Int32 targetHeight)
        {
            PreProcessedImage returnImage = new PreProcessedImage
            {
                Width = input.Width,
                Height = input.Height,
                Data = input.Data,
                FileName = input.FileName,
                ProcessedWidth = targetWidth,
                ProcessedHeight = targetHeight,
                BWMap = new Byte[targetWidth * targetHeight]
            };
            
            for (Int32 x = 0; x < targetWidth; x++)
            {
                for (Int32 y = 0; y < targetHeight; y++)
                {
                    Byte[] target = PickPixel(x, y, returnImage);
                    returnImage.BWMap[x + (y * targetWidth)] = (Byte)(255 - target[0]);
                }
            }

            return returnImage;
        }


        public static Byte[] PickPixel(Int32 x, Int32 y, Image image)
        {
            if (x >= image.Width || y >= image.Height || x < 0 || y < 0) return null;
            Byte[] returnArray = new Byte[3];
            Int32 offset = ((image.Width * y) + x) * 3;

            Array.Copy(image.Data, offset, returnArray, 0, 3);
            return returnArray;
        }
    }
}
