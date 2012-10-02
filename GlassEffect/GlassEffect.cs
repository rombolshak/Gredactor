using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using System.Windows.Forms;
using System.Drawing;

namespace GlassEffect
{
    public class GlassEffect : IEffect
    {
        public string Name
        {
            get { return "Стекло"; }
        }

        public string Description
        {
            get { return "Эффект стекла"; }
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
            byte[] newValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);


            Random r = new Random();
            for (int y = 0; y < bmpData.Height; ++y)
                for (int x = 0; x < bmpData.Width; ++x)
                {
                    if (worker != null)
                        if (worker.CancellationPending)
                        {
                            original.UnlockBits(bmpData);
                            return original;
                        }

                    int index = (y * bmpData.Width + x) * 3;
                    if (index + 2 >= bytes) break;

                    int fromX = (int)(x - 10 * (r.NextDouble() - .5));
                    int fromY = (int)(y - 10 * (r.NextDouble() - .5));
                    if ((fromX < 0) || (fromX >= bmpData.Width)) fromX = x;
                    if ((fromY < 0) || (fromY >= bmpData.Height)) fromY = y;
                    int fromIndex = (fromY * bmpData.Width + fromX) * 3;

                    newValues[index + 2] = values[fromIndex + 2];
                    newValues[index + 1] = values[fromIndex + 1];
                    newValues[index + 0] = values[fromIndex + 0];

                    if (worker != null)
                        worker.ReportProgress((int)(100.0 * (double)index / (double)bytes));
                }

            System.Runtime.InteropServices.Marshal.Copy(newValues, 0, ptr, bytes);
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
            get { return 'G'; }
        }

        public string LongConsoleKey
        {
            get { return "glass"; }
        }

        public string ConsoleParams
        {
            get { return ""; }
        }
    }
}
