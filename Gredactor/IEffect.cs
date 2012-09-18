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

        Bitmap Apply(Bitmap original);

        string MenuGroup { get; }
        ToolStripMenuItem MenuItem { get; }
        Button[] Buttons { get; }
        string ShortConsoleKey { get; }
        string LongConsoleKey { get; }
    }
}
