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

        bool Prepare(object obj, bool console = false);
        Bitmap Apply(Bitmap original, System.ComponentModel.BackgroundWorker worker = null);

        string MenuGroup { get; }
        ToolStripMenuItem MenuItem { get; }
        Button Button { get; }
        char ShortConsoleKey { get; }
        string LongConsoleKey { get; }
        string ConsoleParams { get; }
    }
}
