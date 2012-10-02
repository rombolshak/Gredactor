using System;
using System.Drawing;
using System.Windows.Forms;
using Gredactor;

namespace ContrastEffect
{
    class Helper
    {
        /// <summary>
        /// Не совсем линейное растяжение, здесь используется нормализация гистограммы. Выкидывается по меньшей мере 5% изображения с обоих концов гистограммы (самые темные и самые светлые)
        /// </summary>
        /// <param name="cValues"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void FindMaxMin(byte[] cValues, out int min, out int max)
        {
            uint length = 0; for (byte i = 0; i < 255; ++i) length += cValues[i + 0]; // общее число элементов

            double sum = 0; int pos = 0;
            while (sum / length < .05) // откидываем 5% слева
                sum += cValues[pos++];
            min = pos;

            sum = 0; pos = 255;
            while (sum / length < .05) // откидываем 5% справа
                sum += cValues[pos--];
            max = pos;
        }

        /// <summary>
        /// Линейный сдвиг, основываясь на концах растягиваемого отрезка
        /// </summary>
        /// <param name="old"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static byte CalculateNewValue(byte old, int min, int max)
        {
            double res = (double)(old - min) / (max - min) * 255;
            if (res < 0) res = 0; if (res > 255) res = 255;
            return (byte)res;
        }
    }

    public class AutoContrastEffect : IEffect
    {
        public string Name
        {
            get { return "Автоконтраст"; }
        }

        public string Description
        {
            get { return "Линейное растяжение гистограммы яркости"; }
        }

        public bool Prepare(object obj, bool console = false)
        {
            return true;
        }

        public Bitmap Apply(Bitmap original, System.ComponentModel.BackgroundWorker worker)
        {
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            byte[] cValues = new byte[256];
            double[] hsv = RGB2HSV(values); // переводим в HSV
            for (int i = 2; i < hsv.Length - 1; i += 3)
                ++cValues[Convert.ToInt32(hsv[i] * 255)]; // гистограмма. В HSV яркость представлена от 0 до 1, поэтому здесь линейно растягиваем на [0; 255]
            int cMin, cMax;
            Helper.FindMaxMin(cValues, out cMin, out cMax);
            for (int i = 2; i < hsv.Length - 1; i += 3)
                hsv[i] = Helper.CalculateNewValue((byte)(hsv[i] * 255), cMin, cMax) / 255d;
            values = HSV2RGB(hsv);

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        /// <summary>
        /// Переводит "сырой" массив значений HSV в "сырой" массив RGB
        /// </summary>
        /// <param name="hsv">Массив, в котором элемент #(3n) = hue, #(3n+1) = saturation, #(3n+2) = value</param>
        /// <returns>Массив, в котором элемент #(3n) = blue, #(3n+1) = green, #(3n+2) = red</returns>
        private byte[] HSV2RGB(double[] hsv)
        {
            byte[] rgb = new byte[hsv.Length];
            for (int i = 0; i < hsv.Length - 1; i += 3)
            {
                if (i + 2 >= hsv.Length) break;
                Color c = ColorFromHSV(hsv[i + 0], hsv[i + 1], hsv[i + 2]);
                rgb[i + 2] = c.R;
                rgb[i + 1] = c.G;
                rgb[i + 0] = c.B;
            }
            return rgb;
        }

        /// <summary>
        /// Переводит "сырой" массив значений RGB в "сырой" массив HSV
        /// </summary>
        /// <param name="values">Массив, в котором элемент #(3n) = blue, #(3n+1) = green, #(3n+2) = red</param>
        /// <returns>Массив, в котором элемент #(3n) = hue, #(3n+1) = saturation, #(3n+2) = value</returns>
        private double[] RGB2HSV(byte[] values)
        {
            double[] hsv = new double[values.Length];
            for (int i = 0; i < values.Length - 1; i += 3)
            {
                if (i + 2 >= hsv.Length) break;
                double hue, saturation, value;
                ColorToHSV(Color.FromArgb(values[i + 2], values[i + 1], values[i + 0]), out hue, out saturation, out value);
                hsv[i + 0] = hue;
                hsv[i + 1] = saturation;
                hsv[i + 2] = value;
            }
            return hsv;
        }

        /// <summary>
        /// Перевод одного конкретного цвета RGB в HSV
        /// </summary>
        /// <param name="color">Цвет в RGB</param>
        /// <param name="hue">Тон</param>
        /// <param name="saturation">Насыщенность</param>
        /// <param name="value">Яркость</param>
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

        public string MenuGroup
        {
            get { return "Контрастность"; }
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
                Button b1 = new Button();
                b1.Text = "Автоконтраст";
                return b1;
            }
        }

        public char ShortConsoleKey
        {
            get { return 'c'; }
        }

        public string LongConsoleKey
        {
            get { return "autocontrast"; }
        }

        public string ConsoleParams
        {
            get { return ""; }
        }
    }

    public class AutoLevelsEffect : IEffect
    {

        public string Name
        {
            get { return "Autolevels"; }
        }

        public string Description
        {
            get { return "Поканальное растяжение яркости"; }
        }

        public bool Prepare(object obj, bool console = false)
        {
            return true;
        }

        public Bitmap Apply(Bitmap original, System.ComponentModel.BackgroundWorker worker)
        {
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            // строим гистограммы яркости
            byte[] rValues = new byte[256], gValues = new byte[256], bValues = new byte[256];
            for (int i = 0; i < values.Length - 1; i += 3)
            {
                if (i + 2 >= values.Length) break;
                ++rValues[values[i + 2]];
                ++gValues[values[i + 1]];
                ++bValues[values[i + 0]];
            }

            // находим максимум и минимум, откуда будем растягивать для каждого канала
            int rMin, rMax, gMin, gMax, bMin, bMax;
            Helper.FindMaxMin(rValues, out rMin, out rMax);
            Helper.FindMaxMin(gValues, out gMin, out gMax);
            Helper.FindMaxMin(bValues, out bMin, out bMax);

            // делаем линейный сдвиг и радуемся
            for (int i = 0; i < values.Length - 1; i += 3)
            {
                if (i + 2 >= values.Length) break;
                values[i + 2] = Helper.CalculateNewValue(values[i + 2], rMin, rMax);
                values[i + 1] = Helper.CalculateNewValue(values[i + 1], gMin, gMax);
                values[i + 0] = Helper.CalculateNewValue(values[i + 0], bMin, bMax);
            }

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        public string MenuGroup
        {
            get { return "Контрастность"; }
        }

        public ToolStripMenuItem MenuItem
        {
            get { return new ToolStripMenuItem(this.Name); }
        }

        public Button Button
        {
            get { Button b = new Button(); b.Text = this.Name; return b; }
        }

        public char ShortConsoleKey
        {
            get { return 'l'; }
        }

        public string LongConsoleKey
        {
            get { return "autolevels"; }
        }

        public string ConsoleParams
        {
            get { return ""; }
        }
    }
}
