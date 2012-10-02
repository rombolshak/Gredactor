using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GrayscaleEffect
{
    public class Grasycale : Gredactor.IEffect
    {
        public string Name
        {
            get { return "Оттенки серого"; }
        }

        public string Description
        {
            get { return "Оттенки серого"; }
        }

        public bool Prepare(object obj, bool console = false)
        {
            return true;
        }
        public Bitmap Apply(Bitmap original, System.ComponentModel.BackgroundWorker worker)
        {
            //Bitmap newBitmap = (Bitmap)original.Clone();

            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            for (int i = 0; i < values.Length; i += 3)
            {
                if (worker != null)
                    if (worker.CancellationPending)
                    {
                        original.UnlockBits(bmpData);
                        return original;
                    }

                if (i + 2 >= values.Length) break;
                byte gray = (byte)(values[i + 2] * .3 + values[i + 1] * .59 + values[i+0] * .11);
                values[i + 0] = values[i + 1] = values[i + 2] = gray;

                if (worker != null)
                    worker.ReportProgress(i / bytes * 100);
            }

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
            //for (int x = 0; x < original.Width; x++)
            //{
            //    for (int y = 0; y < original.Height; y++)
            //    {
            //        //get the pixel from the original image
            //        Color originalColor = original.GetPixel(x, y);

            //        //create the grayscale version of the pixel
            //        int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59)
            //            + (originalColor.B * .11));

            //        //create the color object
            //        Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);

            //        //set the new image's pixel to the grayscale version
            //        newBitmap.SetPixel(x, y, newColor);
            //    }
            //}

            //return newBitmap;
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public ToolStripMenuItem MenuItem
        {
            get
            {
                return new ToolStripMenuItem(this.Name);
            }
        }

        public Button Button
        {
            get
            {
                Button b = new Button(); b.Text = this.Name; return b;
            }
        }

        public char ShortConsoleKey
        {
            get { return 'y'; }
        }

        public string LongConsoleKey
        {
            get { return "grayscale"; }
        }

        public string ConsoleParams
        {
            get { return ""; }
        }
    }
}
