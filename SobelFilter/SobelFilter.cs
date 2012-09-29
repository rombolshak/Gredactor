using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using FilterProcessing;
using System.Windows.Forms;
using System.Drawing;

namespace SobelFilter
{
    public class SobelFilter : IEffect
    {
        public string Name
        {
            get { return "Оператор Собеля"; }
        }

        public string Description
        {
            get { return ""; }
        }

        public bool Prepare(object obj)
        {
            return true;
        }

        public Bitmap Apply(Bitmap original)
        {
            GrayscaleEffect.Grasycale gray = new GrayscaleEffect.Grasycale();
            original = gray.Apply(original);

            FilterProcessor processor = new FilterProcessor();

            processor.SetMatrix(new double[][] { new double[] { 1, 2, 1 }, new double[] { 0, 0, 0 }, new double[] { -1, -2, -1 } });
            Bitmap Gx = processor.Process((Bitmap)original.Clone());

            processor.SetMatrix(new double[][] { new double[] { 1, 0, -1 }, new double[] { 2, 0, -2 }, new double[] { 1, 0, -1 } });
            Bitmap Gy = processor.Process((Bitmap)original.Clone());
            
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);

            #region Initialization
            System.Drawing.Imaging.BitmapData bmpDataX = Gx.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptrX = bmpDataX.Scan0;
            int bytesX = Math.Abs(bmpDataX.Stride) * bmpDataX.Height;
            byte[] valuesGx = new byte[bytesX];
            System.Runtime.InteropServices.Marshal.Copy(ptrX, valuesGx, 0, bytesX);
            Gx.UnlockBits(bmpDataX);

            System.Drawing.Imaging.BitmapData bmpDataY = Gy.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptrY = bmpDataY.Scan0;
            int bytesY = Math.Abs(bmpDataY.Stride) * bmpDataY.Height;
            byte[] valuesGy = new byte[bytesY];
            System.Runtime.InteropServices.Marshal.Copy(ptrY, valuesGy, 0, bytesY);
            Gy.UnlockBits(bmpDataY);

            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);
            #endregion

            for (int i = 0; i < values.Length; i += 3)
            {
                if (i + 2 >= values.Length) break;
                double r = Math.Sqrt((double)valuesGx[i] * (double)valuesGx[i] + (double)valuesGy[i] * (double)valuesGy[i]);
                values[i] = (byte)(r/Math.Sqrt(2));//(r > 300) ? (byte)255 : (byte)0;
                values[i + 1] = (byte)(r / Math.Sqrt(2));//(r > 300) ? (byte)255 : (byte)0;
                values[i + 2] = (byte)(r / Math.Sqrt(2));//(r > 300) ? (byte)255 : (byte)0;
            }

            #region Disposing
            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);

            //System.Runtime.InteropServices.Marshal.Copy(valuesGy, 0, ptrY, bytesY);
            //Gy.UnlockBits(bmpDataY);

            //System.Runtime.InteropServices.Marshal.Copy(valuesGx, 0, ptrX, bytesX);
            //Gx.UnlockBits(bmpDataX);
            #endregion
            return original;
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public ToolStripMenuItem[] MenuItems
        {
            get { return new ToolStripMenuItem[] { new ToolStripMenuItem(this.Name) }; }
        }

        public Button[] Buttons
        {
            get { Button b = new Button(); b.Text = this.Name; return new Button[] { b }; }
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
