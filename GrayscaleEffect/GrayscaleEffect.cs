using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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

        public bool Prepare(object obj)
        {
            return true;
        }
        public Bitmap Apply(Bitmap original)
        {
            Bitmap newBitmap = (Bitmap)original.Clone();

            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    //get the pixel from the original image
                    Color originalColor = original.GetPixel(x, y);

                    //create the grayscale version of the pixel
                    int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59)
                        + (originalColor.B * .11));

                    //create the color object
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);

                    //set the new image's pixel to the grayscale version
                    newBitmap.SetPixel(x, y, newColor);
                }
            }

            return newBitmap;
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
