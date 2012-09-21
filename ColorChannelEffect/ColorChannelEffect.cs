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
        enum Colors {None, Red, Green, Blue };
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
            Bitmap result = new System.Drawing.Bitmap(original.Width, original.Height);
            for (int x = 0; x < original.Width; ++x)
                for (int y = 0; y < original.Height; ++y)
                {
                    Color c = original.GetPixel(x, y);
                    int r,g,b;
                    if (color != Colors.Red) r = c.R - 255; else r = c.R; if (r < 0) r = 0;
                    if (color != Colors.Green) g = c.G - 255; else g = c.G; if (g < 0) g = 0;
                    if (color != Colors.Blue) b = c.B - 255; else b = c.B; if (b < 0) b = 0;
                    result.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            return result;
        }

        public string MenuGroup
        {
            get { return ""; }
        }

        public System.Windows.Forms.ToolStripMenuItem MenuItem
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

        public string ShortConsoleKey
        {
            get { throw new NotImplementedException(); }
        }

        public string LongConsoleKey
        {
            get { throw new NotImplementedException(); }
        }
    }
}
