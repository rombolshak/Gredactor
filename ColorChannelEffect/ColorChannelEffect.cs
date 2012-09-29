using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gredactor;
using System.Drawing;

namespace ColorChannelEffect
{
    public class ColorChannelEffect : IEffect
    {
        enum Colors { None, Red, Green, Blue };
        Colors color = Colors.None;

        public string Name
        {
            get { return "Один канал"; }
        }

        public string Description
        {
            get { return "Оставляет какой-либо один из трех цветовых каналов"; }
        }

        public bool Prepare(object obj)
        {
            try
            {
                color = (Colors)((Button)obj).Tag;
                Logger.Log("[ColorChannel] Preparing to use color " + color.ToString());
                return true;
            }
            catch { return false; }
        }
        public Bitmap Apply(Bitmap original)
        {
            if (color == Colors.None) return original;
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            for (int i = 0; i < values.Length; i += 3)
            {
                if (i + 2 >= values.Length) break;
                if (color != Colors.Red)   values[i + 2] = (values[i + 2] - 255 < 0) ? (byte)0 : (byte)(values[i + 2] - 255);//values[i+0] -= 255; if (values[i+0] < 0) values[i+0] = 0;
                if (color != Colors.Green) values[i + 1] = (values[i + 1] - 255 < 0) ? (byte)0 : (byte)(values[i + 1] - 255);
                if (color != Colors.Blue)  values[i + 0] = (values[i + 0] - 255 < 0) ? (byte)0 : (byte)(values[i + 0] - 255);
            }

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
            //Bitmap result = new System.Drawing.Bitmap(original.Width, original.Height);
            //for (int x = 0; x < original.Width; ++x)
            //    for (int y = 0; y < original.Height; ++y)
            //    {
            //        Color c = original.GetPixel(x, y);
            //        int r,g,b;
            //        if (color != Colors.Red) r = c.R - 255; else r = c.R; if (r < 0) r = 0;
            //        if (color != Colors.Green) g = c.G - 255; else g = c.G; if (g < 0) g = 0;
            //        if (color != Colors.Blue) b = c.B - 255; else b = c.B; if (b < 0) b = 0;
            //        result.SetPixel(x, y, Color.FromArgb(r, g, b));
            //    }
            //return result;
        }

        public string MenuGroup
        {
            get { return ""; }
        }

        public System.Windows.Forms.ToolStripMenuItem[] MenuItems
        {
            get { return null; }
        }

        public Button[] Buttons
        {
            get
            {
                Button[] arr = new Button[3];

                Button b = new Button();
                b.Text = "Красный";
                b.Tag = Colors.Red;
                arr[0] = b;

                b = new Button();
                b.Text = "Зеленый";
                b.Tag = Colors.Green;
                arr[1] = b;

                b = new Button();
                b.Text = "Синий";
                b.Tag = Colors.Blue;
                arr[2] = b;

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
