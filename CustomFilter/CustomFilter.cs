using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using FilterProcessing;
using Gredactor;

namespace CustomFilter
{
    public class CustomFilter : IEffect
    {
        double[][] _matrix;
        string _strmatrix = "";
        bool _normalize = true;
        public string Name
        {
            get { return "Свой фильтр"; }
        }

        public string Description
        {
            get { return "Произвольный фильтр"; }
        }

        CustomFilterForm form;
        public bool Prepare(object obj, bool console = false)
        {
            if (!CheckDependences()) return false;
            _strmatrix = "";
            _normalize = true;
            if (!console)
            {
                form = new CustomFilterForm();
                form.button1.Click += new EventHandler(OK_Click);
                form.ShowDialog();
            }
            else _strmatrix = (string)obj;
            if (_strmatrix == "") return false;
            if (!CreateMatrix(_strmatrix, out _matrix))
            {
                MessageBox.Show("Плохая (нехорошая, неправильная, некорректная) матрица. Работать не будет. Правда.", "Ащибко", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
                    matrix[/*tryMatrix.Length - 1 - j*/i][/*tryMatrix.Length - 1 - i*/j] = tryMatrix[i][j]; // так и не понял, надо ли переворачивать матрицу или нет
            return true;
        }

        void OK_Click(object sender, EventArgs e)
        {
            _strmatrix = form.textBox1.Text;
            _normalize = form.checkBox1.Checked;
        }

        public Bitmap Apply(Bitmap original)
        {
            try
            {
                FilterProcessor fp = new FilterProcessor(_matrix);
                return fp.Process(original, _normalize);
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

        public ToolStripMenuItem MenuItem
        {
            get
            {
                if (!CheckDependences()) return null;
                return new ToolStripMenuItem(this.Name);
            }
        }

        public Button Button
        {
            get
            {
                if (!CheckDependences()) return null;
                Button b = new Button(); b.Text = this.Name; return b;
            }
        }

        public char ShortConsoleKey
        {
            get { return 'k'; }
        }

        public string LongConsoleKey
        {
            get { return "custom"; }
        }

        public string ConsoleParams
        {
            get { return "<kernel>"; }
        }

        private bool CheckDependences()
        {
            try { FilterProcessor fp = new FilterProcessor(); return true; }
            catch { return false; }
        }
    }
}
