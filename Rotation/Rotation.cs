using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using System.Drawing;
using System.Windows.Forms;

namespace Rotation
{
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

                    int fromX = (int)Math.Round(center.X + r * Math.Cos(alpha + _angle));
                    int fromY = (int)Math.Round(center.Y + r * Math.Sin(alpha + _angle));

                    int index = (y * bmpData.Width + x) * 3;
                    int fromIndex = (fromY * bmpData.Width + fromX) * 3;
                    bool inBounds = InBounds(fromX, fromY, bmpData.Width, bmpData.Height);

                    if (index + 2 >= values.Length) break;
                    newValues[index + 2] = inBounds ? values[fromIndex + 2] : (byte)0;
                    newValues[index + 1] = inBounds ? values[fromIndex + 1] : (byte)0;
                    newValues[index + 0] = inBounds ? values[fromIndex + 0] : (byte)0;
                }

            System.Runtime.InteropServices.Marshal.Copy(newValues, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
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
