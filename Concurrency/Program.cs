using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Concurrency
{
    class Program
    {
        public static void BoxBlur(int imgW, int imgH, ref byte[] pix, Color[,] pixels)
        {
            var count = 0;
            for (var i = 0; i < imgW; i++)
            {
                count += 3;
                for (var j = 0; j < imgH; j++)
                {
                    pix[count] = pixels[i, j].R;
                    pix[count + 1] = pixels[i, j].R;
                    pix[count + 2] = pixels[i, j].R;
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

            BoxBlur(img.Width, img.Height, ref pix, pixels);

            //save a modified copy
            Marshal.Copy(pix, 0, bdata.Scan0, pix.Length);
            img.UnlockBits(bdata);
            img.Save("out.png");
        }
    }
}
