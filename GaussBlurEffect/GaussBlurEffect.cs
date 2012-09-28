using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using FilterProcessing;

namespace GaussBlurEffect
{
    public class GaussBlurEffect : IEffect
    {
        public string Name
        {
            get { return "Фильтр Гаусса"; }
        }

        public string Description
        {
            get { return ""; }
        }


        double _sigma;
        double[][] _matrix;
        GaussEffectForm form;

        public bool Prepare(object obj)
        {
            _sigma = 0;
            form = new GaussEffectForm();
            form.button2.Click += new EventHandler(OK_Click);
            form.ShowDialog();
            if (_sigma == 0) return false;
            CalculateMatrix();
            return true;
        }

        private void CalculateMatrix()
        {            
            int length = (int)Math.Ceiling(6 * _sigma);
            if (length % 2 == 0) length += 1;
            double[] res = new double[length];
            int y = length / 2;
            for (int x = -length / 2; x <= length / 2; ++x)
                res[x + length / 2] = 1 / (2 * Math.PI * Math.Pow(_sigma, 2)) * Math.Exp(-(Math.Pow(x, 2) + Math.Pow(y, 2)) / (2 * Math.Pow(_sigma, 2)));
            _matrix = new double[][] { res };
        }

        void OK_Click(object sender, EventArgs e)
        {
            _sigma = (double)form.numericUpDown1.Value;
        }

        public System.Drawing.Bitmap Apply(System.Drawing.Bitmap original)
        {
            FilterProcessor processor = new FilterProcessor(_matrix, true);
            return processor.Process(original);
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public System.Windows.Forms.ToolStripMenuItem[] MenuItems
        {
            get { return new System.Windows.Forms.ToolStripMenuItem[] { new System.Windows.Forms.ToolStripMenuItem(this.Name) }; }
        }

        public System.Windows.Forms.Button[] Buttons
        {
            get { System.Windows.Forms.Button b = new System.Windows.Forms.Button(); b.Text = this.Name; return new System.Windows.Forms.Button[] { b }; }
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
