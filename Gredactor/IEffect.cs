using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Gredactor
{
    public interface IEffect
    {
        string Name { get; }
        string Description { get; }

        bool Prepare(object obj);
        Bitmap Apply(Bitmap original);

        string MenuGroup { get; }
        ToolStripMenuItem[] MenuItems { get; }
        Button[] Buttons { get; }
        char[] ShortConsoleKey { get; }
        string[] LongConsoleKey { get; }
    }
}
