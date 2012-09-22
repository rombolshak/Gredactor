using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using System.Drawing;
using System.Windows.Forms;

namespace AutolevelsEffect
{
    public class AutolevelsEffect : IEffect
    {
        enum Operation { None, Autocontrast, Autolevels };
        Operation operation = Operation.None;
        public string Name
        {
            get { return "Contrast"; }
        }

        public string Description
        {
            get { return "Поканальное и линейное растяжение гистограммы яркости"; }
        }

        public bool Prepare(object obj)
        {
            try
            {
                operation = (Operation)((Button)obj).Tag;
                return true;
            }
            catch (InvalidCastException)
            {
                operation = (Operation)((ToolStripMenuItem)obj).Tag;
                return true;
            }
            catch
            {
                return false;
            }
        }


        static bool normalization = true;
        public Bitmap Apply(Bitmap original)
        {
            if (operation == Operation.None) return original;
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, original.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);
            
            if (operation == Operation.Autolevels)
            {
                byte[] rValues = new byte[256], gValues = new byte[256], bValues = new byte[256];
                for (int i = 0; i < values.Length; i += 3)
                {
                    ++rValues[values[i + 2]];
                    ++gValues[values[i + 1]];
                    ++bValues[values[i]];
                }
                int rMin, rMax, gMin, gMax, bMin, bMax;
                normalization = true;
                FindMaxMin(rValues, out rMin, out rMax);
                FindMaxMin(gValues, out gMin, out gMax);
                FindMaxMin(bValues, out bMin, out bMax);
                if ((rMin == rMax) || (gMin == gMax) || (bMin == bMax)) return original;
                for (int i = 0; i < values.Length; i += 3)
                {
                    values[i + 2] = CalculateNewValue(values[i + 2], rMin, rMax);
                    values[i + 1] = CalculateNewValue(values[i + 1], gMin, gMax);
                    values[i] = CalculateNewValue(values[i], bMin, bMax);
                }
            }
            else
            {
                byte[] cValues = new byte[256];
                double[] hsv = RGB2HSV(values);
                for (int i = 2; i < hsv.Length; i += 3)
                    ++cValues[Convert.ToInt32(hsv[i] * 255)];
                int cMin, cMax;
                FindMaxMin(cValues, out cMin, out cMax);
                for (int i = 2; i < hsv.Length; i += 3)
                    hsv[i] = CalculateNewValue((byte)(hsv[i] * 255), cMin, cMax) / 255d;
                values = HSV2RGB(hsv);
            }
            

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        private byte[] HSV2RGB(double[] hsv)
        {
            byte[] rgb = new byte[hsv.Length];
            for (int i = 0; i < hsv.Length; i += 3)
            {
                Color c = ColorFromHSV(hsv[i], hsv[i + 1], hsv[i + 2]);
                rgb[i + 2] = c.R;
                rgb[i + 1] = c.G;
                rgb[i] = c.B;
            }
            return rgb;
        }

        private double[] RGB2HSV(byte[] values)
        {
            double[] hsv = new double[values.Length];
            for (int i = 0; i < values.Length; i += 3)
            {
                double hue, saturation, value;
                ColorToHSV(Color.FromArgb(values[i + 2], values[i + 1], values[i]), out hue, out saturation, out value);
                hsv[i] = hue;
                hsv[i + 1] = saturation;
                hsv[i + 2] = value;
            }
            return hsv;
        }

        void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        byte CalculateNewValue(byte old, int min, int max)
        {
            double res = (double)(old - min) / (max - min) * 255;
            if (res < 0) res = 0; if (res > 255) res = 255;
            return (byte)res;
        }

        private static void FindMaxMin(byte[] cValues, out int min, out int max)
        {
            int minPos, maxPos;
            if (normalization)
            {
                uint length = 0; for (byte i = 0; i < 255; ++i) length += cValues[i];
                double sum = 0; int pos = 0;
                while ((sum / length < .05) && (pos < cValues.Length - 1))
                    sum += cValues[pos++];
                minPos = pos;
                sum = 0; pos = 255;
                while ((sum / length < .05) && (pos > 0))
                    sum += cValues[pos--];
                maxPos = pos;
                if (maxPos - minPos < 1)
                {
                    normalization = false;
                    FindMinMaxNoNormalization(cValues, out minPos, out maxPos);
                }
            }
            else FindMinMaxNoNormalization(cValues, out minPos, out maxPos);
            min = minPos; max = maxPos;
        }

        private static void FindMinMaxNoNormalization(byte[] cValues, out int minPos, out int maxPos)
        {
            int findPos = 0;
            while (cValues[findPos++] == 0) ;
            minPos = findPos;
            findPos = 255;
            while (cValues[findPos--] == 0) ;
            maxPos = findPos;
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public ToolStripMenuItem[] MenuItems
        {
            get
            {
                ToolStripMenuItem b1 = new ToolStripMenuItem(), b2 = new ToolStripMenuItem();
                b1.Text = "Autocontrast"; b2.Text = "Autolevels";
                b1.Tag = Operation.Autocontrast; b2.Tag = Operation.Autolevels;
                return new ToolStripMenuItem[] { b1, b2 };
            }
        }

        public Button[] Buttons
        {
            get {
                Button b1 = new Button(), b2 = new Button();
                b1.Text = "Autocontrast"; b2.Text = "Autolevels";
                b1.Tag = Operation.Autocontrast; b2.Tag = Operation.Autolevels;
                return new Button[] {b1, b2};
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
