using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using FilterProcessing;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace CustomFilter
{
    public class CustomFilter : IEffect
    {
        double[][] _matrix;
        string _strmatrix = "";
        public string Name
        {
            get { return "Свой фильтр"; }
        }

        public string Description
        {
            get { return ""; }
        }

        CustomFilterForm form;
        public bool Prepare(object obj)
        {
            if (!CheckDependences()) return false;
            _strmatrix = "";
            form = new CustomFilterForm();
            form.button1.Click += new EventHandler(OK_Click);
            form.ShowDialog();
            if (_strmatrix == "") return false;
            if (!CreateMatrix(_strmatrix, out _matrix))
            {
                MessageBox.Show("Плохая (нехорошая, неправильная, некорректная) матрица. Работать не будет. Правда", "Ащибко", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            return true;
        }

        private bool CreateMatrix(string _strmatrix, out double[][] matrix)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            _strmatrix.Replace(" ", "");
            string[] lines = _strmatrix.Split(';');
            int length = lines.Length;
            if (length % 2 != 1) { matrix = null; return false; }
            double[][] tryMatrix = new double[length][];
            for (int i = 0; i < length; ++i)
            {
                string[] numbers = lines[i].Split(',');
                if (length != numbers.Length) { matrix = null; return false; }

                tryMatrix[i] = new double[numbers.Length];
                for (int j = 0; j < numbers.Length; ++j)
                {
                    double num;
                    if (!Double.TryParse(numbers[j], NumberStyles.Float, culture, out num))
                    {
                        matrix = null;
                        return false;
                    }
                    tryMatrix[i][j] = num;
                }
            }
            matrix = new double[length][];
            for (int i = 0; i < length; ++i)
                matrix[i] = new double[length];

            for (int i = 0; i < tryMatrix.Length; ++i)
                for (int j = 0; j < tryMatrix.Length; ++j)
                    matrix[tryMatrix.Length - 1 - j][tryMatrix.Length - 1 - i] = tryMatrix[i][j];
            return true;
        }

        void OK_Click(object sender, EventArgs e)
        {
            _strmatrix = form.textBox1.Text;
        }

        public Bitmap Apply(Bitmap original)
        {
            try
            {
                FilterProcessor fp = new FilterProcessor(_matrix);
                return fp.Process(original);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return original;
            }
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public ToolStripMenuItem[] MenuItems
        {
            get 
            {
                if (!CheckDependences()) return new ToolStripMenuItem[] { };
                return new ToolStripMenuItem[] { new ToolStripMenuItem(this.Name) }; 
            }
        }

        private bool CheckDependences()
        {
            try { FilterProcessor fp = new FilterProcessor(); return true; }
            catch { return false; }
        }

        public Button[] Buttons
        {
            get { return new Button[] { }; }
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
