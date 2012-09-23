﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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

        public bool Prepare(object obj)
        {
            return true;
        }
        public Bitmap Apply(Bitmap original)
        {
            //Bitmap newBitmap = (Bitmap)original.Clone();

            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, original.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            for (int i = 0; i < values.Length - 1; i += 3)
            {
                byte gray = (byte)(values[i + 2] * .3 + values[i + 1] * .59 + values[i+0] * .11);
                values[i + 0] = values[i + 1] = values[i + 2] = gray;
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

        public System.Windows.Forms.ToolStripMenuItem[] MenuItems
        {
            get 
            {
                System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem();
                item.Text = this.Name;
                item.Name = "grayscaleMenuItem";
                return new System.Windows.Forms.ToolStripMenuItem[] { item };
            }
        }

        public System.Windows.Forms.Button[] Buttons
        {
            get 
            {
                System.Windows.Forms.Button[] arr = new System.Windows.Forms.Button[1];
                System.Windows.Forms.Button b = new System.Windows.Forms.Button();
                b.Text = this.Name;
                arr[0] = b;
                return arr;
            }
        }

        public char[] ShortConsoleKey
        {
            get { throw new NotImplementedException(); }
        }

        public string[] LongConsoleKey
        {
            get { throw new NotImplementedException(); }
        }
    }
}
