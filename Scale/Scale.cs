using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using System.Drawing;
using System.Windows.Forms;

namespace Scale
{
    enum Colors { Red = 2, Green = 1, Blue = 0 }
    public class Scale : IEffect
    {
        double _scale;

        public string Name
        {
            get { return "Масштабирование"; }
        }

        public string Description
        {
            get { return "Увеличивает или уменьшает изображение"; }
        }

        ScaleForm form;
        public bool Prepare(object obj, bool console = false)
        {
            _scale = 0;
            if (!console)
            {
                form = new ScaleForm();
                form.button1.Click += new EventHandler(OK_Click);
                form.ShowDialog();                
            }
            else
                _scale = Int32.Parse((string)obj);
            return _scale != 0;
        }

        void OK_Click(object sender, EventArgs e)
        {
            _scale = (double)form.numericUpDown1.Value;
            form.Close();
        }

        public Bitmap Apply(Bitmap original, System.ComponentModel.BackgroundWorker worker)
        {
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);
            original.UnlockBits(bmpData);

            Bitmap result = new Bitmap((int)(original.Width * _scale), (int)(original.Height * _scale));
            rect = new Rectangle(0, 0, result.Width, result.Height);
            bmpData = result.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            ptr = bmpData.Scan0;
            bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] newValues = new byte[bytes];
           // System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            int newWidth = bmpData.Width, newHeight = bmpData.Height;
            for (int y = 0; y < newHeight; ++y)
                for (int x = 0; x < newWidth; ++x)
                {
                    if (worker != null)
                        if (worker.CancellationPending)
                        {
                            original.UnlockBits(bmpData);
                            return original;
                        }

                    int newIndex = (y * newWidth + x) * 3;
                    if (newIndex + 2 >= newValues.Length) break;

                    double fromX = x / _scale, fromY = y / _scale;
                    newValues[newIndex + 2] = Interpolate(values, fromX, fromY, Colors.Red, original.Width, original.Height);
                    newValues[newIndex + 1] = Interpolate(values, fromX, fromY, Colors.Green, original.Width, original.Height);
                    newValues[newIndex + 0] = Interpolate(values, fromX, fromY, Colors.Blue, original.Width, original.Height);

                    if (worker != null)
                        worker.ReportProgress(newIndex / bytes * 100);
                }

            System.Runtime.InteropServices.Marshal.Copy(newValues, 0, ptr, newValues.Length);
            result.UnlockBits(bmpData);
            return result;
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
                (x >= 0) &&
                (x < width) &&
                (y >= 0) &&
                (y < height)
            );
        }

        public string MenuGroup
        {
            get { return "Геометрия"; }
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
                Button b = new Button(); b.Text = this.Name; return b;
            }
        }

        public char ShortConsoleKey
        {
            get { return 'z'; }
        }

        public string LongConsoleKey
        {
            get { return "resize"; }
        }

        public string ConsoleParams
        {
            get { return "<scale>"; }
        }
    }
}
