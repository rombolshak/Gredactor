using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gredactor;
using FilterProcessing;
using System.Drawing;
using System.Windows.Forms;

namespace CustomFilter
{
    public class CustomFilter : IEffect
    {
        double[][] _matrix;
        public string Name
        {
            get { return "Свой фильтр"; }
        }

        public string Description
        {
            get { return ""; }
        }

        public bool Prepare(object obj)
        {
            _matrix = new double[][] {
                new double[] {0,1,0},
                new double[] {0,0,0},
                new double[] {0,-1,0}
            };
            return true;
        }

        public Bitmap Apply(Bitmap original)
        {
            FilterProcessor fp = new FilterProcessor(_matrix);
            return fp.Process(original);
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public ToolStripMenuItem[] MenuItems
        {
            get { return new ToolStripMenuItem[] { new ToolStripMenuItem(this.Name) }; }
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
