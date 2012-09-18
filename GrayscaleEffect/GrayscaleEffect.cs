using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrayscaleEffect
{
    public class Grasycale : Gredactor.IEffect
    {
        public string Name
        {
            get { return "Grayscale"; }
        }

        public string Description
        {
            get { return "Оттенки серого"; }
        }

        public System.Drawing.Bitmap Apply(System.Drawing.Bitmap original)
        {
            original.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
            return original;
        }

        public string MenuGroup
        {
            get { return "Фильтры"; }
        }

        public System.Windows.Forms.ToolStripMenuItem MenuItem
        {
            get 
            {
                System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem();
                item.Text = "Grayscale";
                item.Name = "grayscaleMenuItem";
                return item;
            }
        }

        public System.Windows.Forms.Button[] Buttons
        {
            get 
            {
                System.Windows.Forms.Button[] arr = new System.Windows.Forms.Button[1];
                System.Windows.Forms.Button b = new System.Windows.Forms.Button();
                b.Text = "Grayscale";
                arr[0] = b;
                return arr;
            }
        }

        public string ShortConsoleKey
        {
            get { throw new NotImplementedException(); }
        }

        public string LongConsoleKey
        {
            get { throw new NotImplementedException(); }
        }
    }
}
