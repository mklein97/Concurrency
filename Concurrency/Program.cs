﻿using System;
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
        private static void Main(string[] args)
        {
            if (args.Length != 3)
                throw new ArgumentException("Must pass 3 arguments: image name, number of threads and number of blur passes");
            if (!int.TryParse(args[1], out var numThreads))
                throw new ArgumentException("Must pass number of threads as second argument!");
            if (!int.TryParse(args[2], out var numRounds))
                throw new ArgumentException("Must pass number of rounds of blurring as third argument!");
            string fname = args[0];
            Bitmap img = (Bitmap)Image.FromFile(fname);

            //convert image to byte array
            var bdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] pix = new byte[bdata.Stride * bdata.Height];
            Marshal.Copy(bdata.Scan0, pix, 0, pix.Length);
            Random r = new Random();
            //int rInt = r.Next(0, 255);
            for (var i = 0; i < pix.Length; i++)
                pix[i] += (byte)r.Next(0, 255);

            //save a modified copy
            Marshal.Copy(pix, 0, bdata.Scan0, pix.Length);
            img.UnlockBits(bdata);
            img.Save("out.png");
        }
    }
}
