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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cranium.Lib.Data.Preprocessing;

namespace Cranium.Lib.Test.SupportClasses
{
    public static class ImageLoader
    {
        public static ImageNormalizer.Image GetImageByteArray(String filename, Int32 newWidth, Int32 newHeight)
        {
            ImageNormalizer.Image img = new ImageNormalizer.Image();

            //string loc = Directory.GetCurrentDirectory() + "\\" + filename.Replace('/', '\\');
            //loc = loc.Replace('\\', '/');
            //Process proc = new Process();
            //proc.StartInfo.WorkingDirectory = "C:/Program Files/ImageMagick-6.9.3-Q16/";
            //proc.StartInfo.FileName = "C:/Program Files/ImageMagick-6.9.3-Q16/convert.exe";
            //proc.StartInfo.Arguments = $"\"{loc}\" -trim \"{loc}\"";
            //proc.StartInfo.RedirectStandardError = true;
            //proc.StartInfo.RedirectStandardOutput = true;
            //proc.StartInfo.RedirectStandardInput = true;
            //proc.StartInfo.UseShellExecute = false;
            //proc.StartInfo.CreateNoWindow = true;
            //proc.Start();
            //proc.WaitForExit();

            //string s = proc.StandardError.ReadToEnd();

            using (Bitmap bmp = new Bitmap(filename))
            {
                using (Bitmap bmp2 = new Bitmap(newWidth, newHeight))
                {
                    Graphics graph = Graphics.FromImage(bmp2);

                    graph.FillRectangle(new SolidBrush(Color.Black), new RectangleF(0, 0, newWidth, newHeight));
                    graph.DrawImage(bmp, new Rectangle(0, 0, newWidth, newHeight));
                    graph.Save();

                    Int32 bytes = bmp2.Width * bmp2.Height * 3;
                    img.Width = bmp2.Width;
                    img.Height = bmp2.Height;
                    img.Data = new Byte[bytes];
                    img.FileName = filename;
                    BitmapData dat = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    IntPtr ptr = dat.Scan0;
                    Byte[] byteArray = new Byte[dat.Stride * img.Height];
                    Marshal.Copy(ptr, byteArray, 0, byteArray.Length);

                    for (Int32 y = 0; y < img.Height; y++) Array.ConstrainedCopy(byteArray, y * dat.Stride, img.Data, y * img.Width * 3, img.Width * 3);

                    bmp2.UnlockBits(dat);
                }
            }

            return img;
        }

        public static void SaveBwImage(String fileName, Int32 width, Int32 height, Byte[] data)
        {
            Byte[] outData = new Byte[width * height * 3];

            for (Int32 i = 0; i < width * height * 3; i++)
            {
                outData[i] = data[i / 3];
            }

            Bitmap bmp = new Bitmap(width, height);
            BitmapData dat = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = dat.Scan0;

            Marshal.Copy(outData, 0, ptr, outData.Length);
            bmp.UnlockBits(dat);
            bmp.Save(fileName);
        }

        public static void SaveImage(String fileName, Int32 width, Int32 height, Byte[] data)
        {
            Bitmap bmp = new Bitmap(width, height);
            BitmapData dat = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr ptr = dat.Scan0;
            Marshal.Copy(data, 0, ptr, data.Length);
            bmp.UnlockBits(dat);
            bmp.Save(fileName);
        }

        public static IEnumerable<ImageNormalizer.Image> GetImagesInFolder(String folderName, Int32 newWidth, Int32 newHeight)
        {
            return (from file in Directory.EnumerateFiles(folderName) where file.ToLowerInvariant().EndsWith(".png") select GetImageByteArray(file, newWidth, newHeight)).ToList();
        }
    }
}