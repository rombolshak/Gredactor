using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Gredactor;
using FilterProcessing;

namespace NeonEffect
{
    public class NeonEffect : IEffect
    {
        public string Name
        {
            get { return "Неоновая подсветка"; }
        }

        public string Description
        {
            get { return "Декоративный фильтр"; }
        }

        public bool Prepare(object obj)
        {
            if (!CheckDependencies()) return false;
            return true;
        }

        private bool CheckDependencies()
        {
            try { new GaussBlurEffect.GaussBlurEffect(); new SobelFilter.SobelFilter(); return true; }
            catch { return false; }
        }

        public Bitmap Apply(Bitmap original)
        {
            //MedianFilter.MedianFilter mf = new MedianFilter.MedianFilter();
            //mf.Radius = 1;
            //original = mf.Apply(original);
            GaussBlurEffect.GaussBlurEffect gf = new GaussBlurEffect.GaussBlurEffect();
            gf.Sigma = 1;
            original = gf.Apply(original);

            SobelFilter.SobelFilter sf = new SobelFilter.SobelFilter();
            sf.notGray = true;
            original = sf.Apply(original);

            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            byte[] rValues = new byte[256], gValues = new byte[256], bValues = new byte[256];
            for (int i = 0; i < values.Length - 1; i += 3)
            {
                if (i + 2 >= values.Length) break;
                ++rValues[values[i + 2]];
                ++gValues[values[i + 1]];
                ++bValues[values[i + 0]];
            }

            byte rMax = rValues.Last(x => x == rValues.Max());
            byte gMax = gValues.Last(x => x == gValues.Max());
            byte bMax = bValues.Last(x => x == bValues.Max());

            for (int i = 0; i < values.Length - 1; i += 3)
            {
                if (i + 2 >= values.Length) break;
                if (values[i + 2] > 128) values[i + 2] = rMax;
                if (values[i + 1] > 128) values[i + 1] = gMax;
                if (values[i + 0] > 128) values[i + 0] = bMax;
            }

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public ToolStripMenuItem[] MenuItems
        {
            get
            {
                if (!CheckDependencies()) return new ToolStripMenuItem[] { };
                return new ToolStripMenuItem[] { new ToolStripMenuItem(this.Name) }; 
            }
        }

        public Button[] Buttons
        {
            get
            {
                if (!CheckDependencies()) return new Button[] { };
                Button b = new Button(); b.Text = this.Name; return new Button[] { b }; 
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
