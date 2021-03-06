﻿using System;
using System.Windows.Forms;
using FilterProcessing;
using Gredactor;

namespace GaussBlurEffect
{
    public class GaussBlurEffect : IEffect
    {
        public double Sigma
        {
            get { return _sigma; }
            set { _sigma = value; }
        }
        public string Name
        {
            get { return "Фильтр Гаусса"; }
        }

        public string Description
        {
            get { return "Фильтр Гаусса"; }
        }


        double _sigma;
        double[][] _matrix;
        GaussEffectForm form;

        public bool Prepare(object obj, bool console = false)
        {
            if (!CheckDependencies()) return false;
            _sigma = 0;
            if (!console)
            {
                form = new GaussEffectForm();
                form.button2.Click += new EventHandler(OK_Click);
                form.ShowDialog();
            }
            else _sigma = Int32.Parse((string)obj);
            if (_sigma > 100)
            {
                if (_sigma > 200)
                {
                    if (_sigma > 300)
                    {
                        DialogResult dr = System.Windows.Forms.MessageBox.Show("Значение слишком большое. Программа может работать неопределенно долго. Установить значение 200? При нажатии \"Нет\" сохранится значение " + _sigma, "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        if (dr == DialogResult.Yes)
                            _sigma = 200;
                        else if (dr == DialogResult.Cancel)
                            _sigma = 0;
                    }
                    else
                    {
                        DialogResult dr = System.Windows.Forms.MessageBox.Show("Значение слишком большое. Программа может работать неопределенно долго. Продолжить?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.No)
                            _sigma = 0;
                    }
                }
                else
                {
                    DialogResult dr = System.Windows.Forms.MessageBox.Show("Значение достаточно большое. Программа может работать долго. Съешьте еще этих мягких французских булочек да выпейте чаю", "Внимание", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (dr == DialogResult.Cancel)
                        _sigma = 0;
                }
            }
            return _sigma != 0;
        }

        private bool CheckDependencies()
        {
            try { new FilterProcessor(); return true; }
            catch { return false; }
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
            CalculateMatrix();
            FilterProcessor processor = new FilterProcessor(_matrix, true);
            return processor.Process(original);
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
            get { return 'g'; }
        }

        public string LongConsoleKey
        {
            get { return "gaussian"; }
        }

        public string ConsoleParams
        {
            get { return "<sigma>"; }
        }
    }
}
