using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Gredactor;

namespace GrayworldEffect
{
    public class GrayworldEffect : IEffect
    {
        public string Name
        {
            get { return "Серый мир"; }
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
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            float rAvg = 0, gAvg = 0, bAvg = 0; int N = 0;
            for (int i = 0; i < values.Length; i += 3)
            {
                if (i + 2 >= values.Length) break;
                N += 1;
                rAvg += values[i + 2];
                gAvg += values[i + 1];
                bAvg += values[i + 0];
            }
            rAvg /= N;
            gAvg /= N;
            bAvg /= N;
            float avg = (rAvg + gAvg + bAvg) / 3;

            float rCoef = avg / rAvg;
            float gCoef = avg / gAvg;
            float bCoef = avg / bAvg;

            for (int i = 0; i < values.Length; i += 3)
            {
                if (i + 2 >= values.Length) break;
                int newR = (int)((float)values[i + 2] * rCoef);
                int newG = (int)((float)values[i + 1] * gCoef);
                int newB = (int)((float)values[i + 0] * gCoef);
                values[i + 2] = (newR > 255) ? (byte)255 : ((newR < 0) ? (byte)0 : (byte)newR);
                values[i + 1] = (newG > 255) ? (byte)255 : ((newG < 0) ? (byte)0 : (byte)newG);
                values[i + 0] = (newB > 255) ? (byte)255 : ((newB < 0) ? (byte)0 : (byte)newB);
            }

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        public string MenuGroup
        {
            get { return "Контрастность"; }
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
