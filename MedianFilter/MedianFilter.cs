using System;
using System.Drawing;
using System.Windows.Forms;
using Gredactor;

namespace MedianFilter
{
    public class MedianFilter : IEffect
    {
        private int _radius;

        public int Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public string Name
        {
            get { return "Медианный фильтр"; }
        }

        public string Description
        {
            get { return "Усреднение по окрестности"; }
        }

        MedianFilterForm form;
        public bool Prepare(object obj, bool console = false)
        {
            _radius = 0;
            if (!console)
            {
                form = new MedianFilterForm();
                form.button2.Click += new EventHandler(OK_Click);
                form.ShowDialog();
            }
            else _radius = Int32.Parse((string)obj);
            if (_radius > 10)
            {
                if (_radius > 20)
                {
                    if (_radius > 30)
                    {
                        DialogResult dr = System.Windows.Forms.MessageBox.Show("Значение радиуса слишком большое. Программа может работать неопределенно долго. Установить значение 30? При нажатии \"Нет\" сохранится значение " + _radius, "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        if (dr == DialogResult.Yes)
                            _radius = 30;
                        else if (dr == DialogResult.Cancel)
                            _radius = 0;
                    }
                    else
                    {
                        DialogResult dr = System.Windows.Forms.MessageBox.Show("Значение радиуса слишком большое. Программа может работать неопределенно долго. Продолжить?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.No)
                            _radius = 0;
                    }
                }
                else
                {
                    DialogResult dr = System.Windows.Forms.MessageBox.Show("Значение радиуса достаточно большое. Программа может работать долго. Съешьте еще этих мягких французских булочек да выпейте чаю", "Внимание", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (dr == DialogResult.Cancel)
                        _radius = 0;
                }
            }
            return _radius != 0;
        }

        void OK_Click(object sender, EventArgs e)
        {
            _radius = (int)form.numericUpDown1.Value;
        }

        public Bitmap Apply(Bitmap original)
        {
            Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            byte[] newValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            int kernelSize = 2 * _radius + 1;
            int arrSize = kernelSize * kernelSize;
            for (int y = 0; y < bmpData.Height; ++y)
                for (int x = 0; x < bmpData.Width; ++x)
                {
                    byte[] rValues = new byte[arrSize], gValues = new byte[arrSize], bValues = new byte[arrSize];
                    int index = (x + y * bmpData.Width) * 3;
                    int pos = 0;

                    for (int j = -_radius; j <= _radius; ++j)
                        for (int i = -_radius; i <= _radius; ++i)
                        {
                            // в массив значений записываем нужный элемент из values.
                            // если пиксель за границами изображения, отражаем его, потому
                            // Math.Abs(x + i) -- смещение в строке по изображению
                            // Math.Abs(y + j) * bmpData.Width -- смещение строки в изображении                            
                            int valuesX = Math.Abs(x + i); if (valuesX >= bmpData.Width) valuesX = x - i;
                            int valuesY = Math.Abs(y + j); if (valuesY >= bmpData.Height) valuesY = y - j;

                            rValues[pos] = values[(valuesX + valuesY * bmpData.Width) * 3 + 2];
                            gValues[pos] = values[(valuesX + valuesY * bmpData.Width) * 3 + 1];
                            bValues[pos] = values[(valuesX + valuesY * bmpData.Width) * 3 + 0];
                            ++pos;
                        }

                    // the code below is too slooooooow
                    // but it's the best realization
                    // nothing to do here
                    Array.Sort(rValues);
                    newValues[index + 2] = rValues[rValues.Length / 2];

                    Array.Sort(gValues);
                    newValues[index + 1] = gValues[rValues.Length / 2];

                    Array.Sort(bValues);
                    newValues[index + 0] = bValues[rValues.Length / 2];
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
            get { return 'm'; }
        }

        public string LongConsoleKey
        {
            get { return "median"; }
        }

        public string ConsoleParams
        {
            get { return "<radius>"; }
        }
    }
}
