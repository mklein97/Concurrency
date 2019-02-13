using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Configuration;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Concurrency
{
    class Program
    {
        public static void BoxBlur(int imgW, int imgH, ref byte[] pix, Color[,] pixels, int passes)
        {
            Color[,] temp = new Color[imgW, imgH];
            for (var n = 1; n <= passes; n++)
            {
                for (var i = 0; i < imgH; i++)
                {
                    for (var j = 0; j < imgW; j++)
                    {
                        var r = 0;
                        var g = 0;
                        var b = 0;

                        if (n == 1)
                           averageRGB(j, i, ref r, ref g, ref b, pixels);
                        else
                           averageRGB(j, i, ref r, ref g, ref b, temp);

                        temp[j, i] = Color.FromArgb((byte)Math.Sqrt(r / 20.0), (byte)Math.Sqrt(g / 20.0), (byte)Math.Sqrt(b / 20.0));
                    }
                }
            }

            var count = 0;
            for (var i = 0; i < imgH; i++)
            {
                for (var j = 0; j < imgW; j++)
                {
                    pix[count] = temp[j, i].B;
                    pix[count + 1] = temp[j, i].G;
                    pix[count + 2] = temp[j, i].R;
                    count += 3;
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
                throw new ArgumentException("Must pass 3 arguments: image name, number of threads and number of blur passes");
            if (!int.TryParse(args[1], out var numThreads))
                throw new ArgumentException("Must pass number of threads as second argument!");
            if (!int.TryParse(args[2], out var numRounds))
                throw new ArgumentException("Must pass number of rounds of blurring as third argument!");
            string fname = args[0];
            Bitmap img = (Bitmap)Image.FromFile(fname);

            //get x, y coordinates of each pixel
            Color[,] pixels = new Color[img.Width, img.Height];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                    pixels[i, j] = img.GetPixel(i, j);
            }

            //convert image to byte array
            var bdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] pix = new byte[bdata.Stride * bdata.Height];
            Marshal.Copy(bdata.Scan0, pix, 0, pix.Length);

            BoxBlur(img.Width, img.Height, ref pix, pixels, numRounds);

            //save a modified copy
            Marshal.Copy(pix, 0, bdata.Scan0, pix.Length);
            img.UnlockBits(bdata);
            img.Save("out.png");
        }
        public static void averageRGB(int j, int i, ref int r, ref int g, ref int b, Color[,] pixels)
        {
            if (!pixels[j, i].Equals(Color.Black))
            {
                r += pixels[j, i].R * pixels[j, i].R;
                g += pixels[j, i].G * pixels[j, i].G;
                b += pixels[j, i].B * pixels[j, i].B;
            }

            if (j + 1 < pixels.GetLength(0) && j + 1 >= 0 && i < pixels.GetLength(1) && i >= 0)
            {
                r += pixels[j + 1, i].R * pixels[j + 1, i].R;
                g += pixels[j + 1, i].G * pixels[j + 1, i].G;
                b += pixels[j + 1, i].B * pixels[j + 1, i].B;
            }

            if (j + 2 < pixels.GetLength(0) && j + 2 >= 0 && i < pixels.GetLength(1) && i >= 0)
            {
                r += pixels[j + 2, i].R * pixels[j + 2, i].R;
                g += pixels[j + 2, i].G * pixels[j + 2, i].G;
                b += pixels[j + 2, i].B * pixels[j + 2, i].B;
            }

            if (j - 1 < pixels.GetLength(0) && j - 1 >= 0 && i < pixels.GetLength(1) && i >= 0)
            {
                r += pixels[j - 1, i].R * pixels[j - 1, i].R;
                g += pixels[j - 1, i].G * pixels[j - 1, i].G;
                b += pixels[j - 1, i].B * pixels[j - 1, i].B;
            }

            if (j - 2 < pixels.GetLength(0) && j - 2 >= 0 && i < pixels.GetLength(1) && i >= 0)
            {
                r += pixels[j - 2, i].R * pixels[j - 2, i].R;
                g += pixels[j - 2, i].G * pixels[j - 2, i].G;
                b += pixels[j - 2, i].B * pixels[j - 2, i].B;
            }

            if (j < pixels.GetLength(0) && j >= 0 && i + 1 < pixels.GetLength(1) && i + 1 >= 0)
            {
                r += pixels[j, i + 1].R * pixels[j, i + 1].R;
                g += pixels[j, i + 1].G * pixels[j, i + 1].G;
                b += pixels[j, i + 1].B * pixels[j, i + 1].B;
            }

            if (j < pixels.GetLength(0) && j >= 0 && i + 2 < pixels.GetLength(1) && i + 2 >= 0)
            {
                r += pixels[j, i + 2].R * pixels[j, i + 2].R;
                g += pixels[j, i + 2].G * pixels[j, i + 2].G;
                b += pixels[j, i + 2].B * pixels[j, i + 2].B;
            }

            if (j < pixels.GetLength(0) && j >= 0 && i - 1 < pixels.GetLength(1) && i - 1 >= 0)
            {
                r += pixels[j, i - 1].R * pixels[j, i - 1].R;
                g += pixels[j, i - 1].G * pixels[j, i - 1].G;
                b += pixels[j, i - 1].B * pixels[j, i - 1].B;
            }

            if (j < pixels.GetLength(0) && j >= 0 && i - 2 < pixels.GetLength(1) && i - 2 >= 0)
            {
                r += pixels[j, i - 2].R * pixels[j, i - 2].R;
                g += pixels[j, i - 2].G * pixels[j, i - 2].G;
                b += pixels[j, i - 2].B * pixels[j, i - 2].B;
            }

            if (j + 1 < pixels.GetLength(0) && j + 1 >= 0 && i + 1 < pixels.GetLength(1) && i + 1 >= 0)
            {
                r += pixels[j + 1, i + 1].R * pixels[j + 1, i + 1].R;
                g += pixels[j + 1, i + 1].G * pixels[j + 1, i + 1].G;
                b += pixels[j + 1, i + 1].B * pixels[j + 1, i + 1].B;
            }

            if (j + 2 < pixels.GetLength(0) && j + 2 >= 0 && i + 2 < pixels.GetLength(1) && i + 2 >= 0)
            {
                r += pixels[j + 2, i + 2].R * pixels[j + 2, i + 2].R;
                g += pixels[j + 2, i + 2].G * pixels[j + 2, i + 2].G;
                b += pixels[j + 2, i + 2].B * pixels[j + 2, i + 2].B;
            }

            if (j - 1 < pixels.GetLength(0) && j - 1 >= 0 && i - 1 < pixels.GetLength(1) && i - 1 >= 0)
            {
                r += pixels[j - 1, i - 1].R * pixels[j - 1, i - 1].R;
                g += pixels[j - 1, i - 1].G * pixels[j - 1, i - 1].G;
                b += pixels[j - 1, i - 1].B * pixels[j - 1, i - 1].B;
            }

            if (j - 2 < pixels.GetLength(0) && j - 2 >= 0 && i - 2 < pixels.GetLength(1) && i - 2 >= 0)
            {
                r += pixels[j - 2, i - 2].R * pixels[j - 2, i - 2].R;
                g += pixels[j - 2, i - 2].G * pixels[j - 2, i - 2].G;
                b += pixels[j - 2, i - 2].B * pixels[j - 2, i - 2].B;
            }

            if (j + 1 < pixels.GetLength(0) && j + 1 >= 0 && i + 2 < pixels.GetLength(1) && i + 2 >= 0)
            {
                r += pixels[j + 1, i + 2].R * pixels[j + 1, i + 2].R;
                g += pixels[j + 1, i + 2].G * pixels[j + 1, i + 2].G;
                b += pixels[j + 1, i + 2].B * pixels[j + 1, i + 2].B;
            }

            if (j - 1 < pixels.GetLength(0) && j - 1 >= 0 && i + 2 < pixels.GetLength(1) && i + 2 >= 0)
            {
                r += pixels[j - 1, i + 2].R * pixels[j - 1, i + 2].R;
                g += pixels[j - 1, i + 2].G * pixels[j - 1, i + 2].G;
                b += pixels[j - 1, i + 2].B * pixels[j - 1, i + 2].B;
            }

            if (j + 1 < pixels.GetLength(0) && j + 1 >= 0 && i - 2 < pixels.GetLength(1) && i - 2 >= 0)
            {
                r += pixels[j + 1, i - 2].R * pixels[j + 1, i - 2].R;
                g += pixels[j + 1, i - 2].G * pixels[j + 1, i - 2].G;
                b += pixels[j + 1, i - 2].B * pixels[j + 1, i - 2].B;
            }

            if (j + 2 < pixels.GetLength(0) && j + 2 >= 0 && i + 1 < pixels.GetLength(1) && i + 1 >= 0)
            {
                r += pixels[j + 2, i + 1].R * pixels[j + 2, i + 1].R;
                g += pixels[j + 2, i + 1].G * pixels[j + 2, i + 1].G;
                b += pixels[j + 2, i + 1].B * pixels[j + 2, i + 1].B;
            }

            if (j + 2 < pixels.GetLength(0) && j + 2 >= 0 && i - 1 < pixels.GetLength(1) && i - 1 >= 0)
            {
                r += pixels[j + 2, i - 1].R * pixels[j + 2, i - 1].R;
                g += pixels[j + 2, i - 1].G * pixels[j + 2, i - 1].G;
                b += pixels[j + 2, i - 1].B * pixels[j + 2, i - 1].B;
            }

            if (j - 2 < pixels.GetLength(0) && j - 2 >= 0 && i + 1 < pixels.GetLength(1) && i + 1 >= 0)
            {
                r += pixels[j - 2, i + 1].R * pixels[j - 2, i + 1].R;
                g += pixels[j - 2, i + 1].G * pixels[j - 2, i + 1].G;
                b += pixels[j - 2, i + 1].B * pixels[j - 2, i + 1].B;
            }

            if (j - 2 < pixels.GetLength(0) && j - 2 >= 0 && i - 1 < pixels.GetLength(1) && i - 1 >= 0)
            {
                r += pixels[j - 2, i - 1].R * pixels[j - 2, i - 1].R;
                g += pixels[j - 2, i - 1].G * pixels[j - 2, i - 1].G;
                b += pixels[j - 2, i - 1].B * pixels[j - 2, i - 1].B;
            }
        }
    }
}
