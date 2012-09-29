using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using System.Drawing;
using System.Windows.Forms;

namespace Rotation
{
    enum Colors { Red = 2, Green = 1, Blue = 0 }
    public class Rotation : IEffect
    {
        double _angle;
        public string Name
        {
            get { return "Поворот"; }
        }

        public string Description
        {
            get { return "Повернуть изображение на заданный угол"; }
        }

        RotationForm form;
        public bool Prepare(object obj)
        {
            _angle = 0;
            form = new RotationForm();
            form.button1.Click += new EventHandler(OK_Click);
            form.ShowDialog();
            return _angle != 0;
        }

        void OK_Click(object sender, EventArgs e)
        {
            _angle = Convert.ToDouble(form.numericUpDown1.Value);
            form.Close();
        }

        public Bitmap Apply(Bitmap original)
        {
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, original.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            byte[] newValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            Point center = new Point(bmpData.Width / 2, bmpData.Height / 2);
            _angle = _angle * Math.PI / 180;

            for (int y = 0; y < bmpData.Height; ++y)
                for (int x = 0; x < bmpData.Width; ++x)
                {
                    double h = Math.Abs(y - center.Y);
                    double r = Math.Sqrt(Math.Pow(x - center.X, 2) + Math.Pow(h, 2));
                    double alpha = Math.Atan2(y - center.Y, x - center.X);

                    double fromX = (center.X + r * Math.Cos(alpha + _angle));
                    double fromY = (center.Y + r * Math.Sin(alpha + _angle));

                    int index = (y * bmpData.Width + x) * 3;
                    //int fromIndex = (fromY * bmpData.Width + fromX) * 3;
                    //bool inBounds = InBounds(fromX, fromY, bmpData.Width, bmpData.Height);

                    if (index + 2 >= values.Length) break;
                    newValues[index + 2] = Interpolate(values, fromX, fromY, Colors.Red, bmpData.Width, bmpData.Height);
                    newValues[index + 1] = Interpolate(values, fromX, fromY, Colors.Green, bmpData.Width, bmpData.Height);
                    newValues[index + 0] = Interpolate(values, fromX, fromY, Colors.Blue, bmpData.Width, bmpData.Height);
                }

            System.Runtime.InteropServices.Marshal.Copy(newValues, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        private byte Interpolate(byte[] values, double x, double y, Colors color, int width, int height)
        {
            int x1 = (int)Math.Floor(x), x2 = (int)Math.Ceiling(x);
            int y1 = (int)Math.Floor(y), y2 = (int)Math.Ceiling(y);
            //if (!InBounds(x, y, width, height)) return (byte)0;

            double q1 = (InBounds(x1, y1, width, height)) ? values[((y1) * width + (x1)) * 3 + (int)color] : 0;
            double q2 = (InBounds(x2, y1, width, height)) ? values[((y1) * width + (x2)) * 3 + (int)color] : 0;
            double q3 = (InBounds(x1, y2, width, height)) ? values[((y2) * width + (x1)) * 3 + (int)color] : 0;
            double q4 = (InBounds(x2, y2, width, height)) ? values[((y2) * width + (x2)) * 3 + (int)color] : 0;

            double p = 0;
            if (x1 == x2)
                if (y1 == y2)
                    p = q1;
                else
                    p = q1 * (y2 - y) / (y2 - y1) + q3 * (y - y1) / (y2 - y1);
            else if (y1 == y2)
                p = q1 * (x2 - x) / (x2 - x1) + q2 * (x - x1) / (x2 - x1);
            else
            {
                double del = (x2 - x1) * (y2 - y1);
                p =
                    q1 * (x2 - x) * (y2 - y) / del +
                    q2 * (x - x1) * (y2 - y) / del +
                    q3 * (x2 - x) * (y - y1) / del +
                    q4 * (x - x1) * (y - y1) / del;
            }

            if (p > 255) p = 255;
            if (p < 0) p = 0;

            return (byte)p;
        }

        bool InBounds(int x, int y, int width, int height)
        {
            return (
                (x >= 0)     &&
                (x < width) &&
                (y >= 0)     &&
                (y < height)
            );
        }

        public string MenuGroup
        {
            get { return "Геометрия"; }
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
