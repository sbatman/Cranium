// //////////////////////
//  
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// This work is covered under the Creative Commons Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0) licence.
// More information can be found about the liecence here http://creativecommons.org/licenses/by-sa/3.0/
// If you wish to discuss the licencing terms please contact Steven Batchelor-Manning
// 
// //////////////////////

using System;

namespace Cranium.Lib.Data.Preprocessing
{
    public static class ImageNormalizer
    {
        public class Image
        {
            public Byte[] Data;
            public String FileName;
            public Int32 Height;
            public String Tag;
            public Int32 Width;
        }

        public class PreProcessedImage : Image
        {
            public Byte[] BWMap;
            public Int32 ProcessedHeight;
            public Int32 ProcessedWidth;
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
                    returnImage.BWMap[x + y * targetWidth] = (Byte) (255 - target[0]);
                }
            }

            return returnImage;
        }

        public static Byte[] PickPixel(Int32 x, Int32 y, Image image)
        {
            if (x >= image.Width || y >= image.Height || x < 0 || y < 0) return null;
            Byte[] returnArray = new Byte[3];
            Int32 offset = (image.Width * y + x) * 3;

            Array.Copy(image.Data, offset, returnArray, 0, 3);
            return returnArray;
        }
    }
}