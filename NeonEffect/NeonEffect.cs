﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gredactor;

namespace NeonEffect
{
    public class NeonEffect : IEffect
    {
        public string Name
        {
            get { return "Декоративный фильтр"; }
        }

        public string Description
        {
            get { return "Декоративный фильтр"; }
        }

        public bool Prepare(object obj, bool console = false)
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

        public ToolStripMenuItem MenuItem
        {
            get
            {
                if (!CheckDependencies()) return null;
                return new ToolStripMenuItem(this.Name);
            }
        }

        public Button Button
        {
            get
            {
                if (!CheckDependencies()) return null;
                Button b = new Button(); b.Text = this.Name; return b;
            }
        }

        public char ShortConsoleKey
        {
            get { return 'n'; }
        }

        public string LongConsoleKey
        {
            get { return "neon"; }
        }

        public string ConsoleParams
        {
            get { return ""; }
        }
    }
}
