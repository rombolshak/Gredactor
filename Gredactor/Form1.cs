using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Gredactor
{
    public partial class MainForm : Form
    {
        ImageHandler imHandler;
        public MainForm()
        {
            try
            {
                imHandler = ImageHandler.GetInstanse();
                imHandler.Changed += new EventHandler(imHandler_Changed);
                InitializeComponent();
                LoadPlugins();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void imHandler_Changed(object sender, EventArgs e)
        {
            Logger.Log("Image changed");
            pictureBox.Image = imHandler.Image;
            pictureBox.Size = imHandler.Image.Size;
            undoToolStripMenuItem.Enabled = imHandler.CanUndo;
            if (imHandler.CanUndo) SetChanged();
            else SetNotChanged();
        }

        /// <summary>
        /// Распихивает кнопочки и менюшки по своим местам
        /// </summary>
        private void LoadPlugins()
        {
            int x = 6, y = 19;
            foreach (IEffect effect in Gredactor.plugins)
            {
                if (effect.Button != null)
                {
                    Button btn = effect.Button;
                    btn.Height = 25;
                    btn.Width = 137;
                    btn.Location = new Point(x, y);
                    btn.Tag = Tuple.Create<IEffect, object>(effect, btn.Tag);
                    btn.Click += new EventHandler(effectButtonClick);
                    toolBox.Controls.Add(btn);
                    y += 31;
                }
                if (effect.MenuItem != null)
                {
                    ToolStripMenuItem item = effect.MenuItem;                    
                    item.Tag = Tuple.Create<IEffect, object>(effect, item.Tag);
                    item.Click += new EventHandler(MenuItem_Click);
                    if ((effect.MenuGroup != "") && (effect.MenuGroup != null))
                        if (!menuStrip1.Items.ContainsKey(effect.MenuGroup))
                        {
                            ToolStripMenuItem newMenuGroup = new ToolStripMenuItem(effect.MenuGroup);
                            newMenuGroup.Name = effect.MenuGroup;
                            newMenuGroup.DropDownItems.Add(item);
                            menuStrip1.Items.Add(newMenuGroup);
                        }
                        else ((ToolStripMenuItem)menuStrip1.Items.Find(effect.MenuGroup, false)[0]).DropDownItems.Add(item);                    
                }
            }
        }

        void MenuItem_Click(object sender, EventArgs e)
        {
            ApplyEffect(sender);
        }

        void effectButtonClick(object sender, EventArgs e)
        {
            ApplyEffect(sender);
        }

        private void ApplyEffect(object sender)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                IEffect effect;
                try
                {
                    // кнопка
                    Tuple<IEffect, object> tuple = (Tuple<IEffect, object>)(((Control)sender).Tag);
                    effect = tuple.Item1;
                    ((Control)sender).Tag = tuple.Item2;
                }
                catch (InvalidCastException)
                {
                    // не кнопка -- значит, меню
                    Tuple<IEffect, object> tuple = (Tuple<IEffect, object>)(((ToolStripItem)sender).Tag);
                    effect = tuple.Item1;
                    ((ToolStripItem)sender).Tag = tuple.Item2;
                }
                catch { return; }

                Logger.Log("Start apply effect " + effect.Name);
                if (effect.Prepare(sender))
                    imHandler.ApplyEffect(effect);
                Logger.Log("End apply effect " + effect.Name);

                try
                {
                    ((Control)sender).Tag = Tuple.Create<IEffect, object>(effect, ((Control)sender).Tag);
                }
                catch (InvalidCastException)
                {
                    ((ToolStripItem)sender).Tag = Tuple.Create<IEffect, object>(effect, ((ToolStripItem)sender).Tag);
                }

                this.Cursor = Cursors.Default;
            }
            catch (Exception ex) 
            {
                Logger.Log(ex.Message + "\r\n=====================\r\nStack:" + ex.StackTrace + "\r\n====================");
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }            
        }

        private void SetChanged()
        {
            if (this.Text[this.Text.Length - 1] != '*')
                this.Text += "*";
            saveToolStripMenuItem.Enabled = true;
            undoToolStripMenuItem.Enabled = imHandler.CanUndo;
        }
        private void SetNotChanged()
        {
            if (this.Text[this.Text.Length - 1] == '*')
                this.Text = this.Text.Remove(this.Text.Length - 1);
            saveToolStripMenuItem.Enabled = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.AutoUpgradeEnabled = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.DefaultExt = ".bmp";
            ofd.Filter = "Изображения|*.bmp;*.jpg;*.jpeg;*.png";
            ofd.Multiselect = false;
            ofd.Title = "Выберите файл";
            ofd.FileOk += new CancelEventHandler(FileSelected);
            ofd.ShowDialog();
        }

        void FileSelected(object sender, CancelEventArgs e)
        {
            string file = ((OpenFileDialog)sender).FileName;
            imHandler.Reset();
            try
            {
                imHandler.Open(file);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBox.Image = null;
                this.Text = "Gredactor";
                return;
            }
            pictureBox.Image = imHandler.Image;
            pictureBox.Width = pictureBox.Image.Width;
            pictureBox.Height = pictureBox.Image.Height;
            saveAsToolStripMenuItem.Enabled = true;
            this.Text = "Gredactor [" + Path.GetFileName(file) + "] ";
            Logger.Log("File " + file + " opened");
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log("Undo button");
            imHandler.Undo();
            undoToolStripMenuItem.Enabled = imHandler.CanUndo;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = "Изображения|*.bmp;*.jpg;*.jpeg;*.png";
            svd.RestoreDirectory = true;
            if (svd.ShowDialog() == DialogResult.OK)
            {
                imHandler.SaveAs(svd.FileName);
                saveToolStripMenuItem.Enabled = false;
                this.Text = "Gredactor [" + Path.GetFileName(svd.FileName) + "] ";
                Logger.Log("File saved as " + svd.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logger.Log("Save button");
            imHandler.Save();
            SetNotChanged();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #region Selection

        // The following three methods will draw a rectangle and allow 
        // the user to use the mouse to resize the rectangle. 

        bool isDrag = false;
        Rectangle _guiRect; // на экране
        Rectangle _selectionRect = new Rectangle // на изображении
            (new Point(0, 0), new Size(0, 0));
        Point _guiStart;
        Point _selectionStart;

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (imHandler.Image == null) return;

            if (e.Button == MouseButtons.Left)
            {
                isDrag = true;
                Control control = (Control)sender; // pictureBox

                _guiStart = control.PointToScreen(new Point(e.X, e.Y));
                _selectionStart = e.Location;
                //ControlPaint.DrawReversibleFrame(_guiRect, Color.Black, FrameStyle.Dashed);
                _guiRect = new Rectangle(0, 0, 0, 0);
            }            
        }

        private void Form1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // If the mouse is being dragged, 
            // undraw and redraw the rectangle as the mouse moves.
            if (isDrag)

            // Hide the previous rectangle by calling the 
            // DrawReversibleFrame method with the same parameters.
            {
                ControlPaint.DrawReversibleFrame(_guiRect,
                    Color.Black, FrameStyle.Dashed);

                // Calculate the endpoint and dimensions for the new 
                // rectangle, again using the PointToScreen method.
                Point endPoint = ((Control)sender).PointToScreen(new Point(e.X, e.Y));

                int width = endPoint.X - _guiStart.X;
                int height = endPoint.Y - _guiStart.Y;
                _guiRect = new Rectangle(_guiStart.X, _guiStart.Y, width, height);

                // Draw the new rectangle by calling DrawReversibleFrame
                // again.  
                ControlPaint.DrawReversibleFrame(_guiRect,
                    Color.Black, FrameStyle.Dashed);
            }
        }

        private void Form1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (imHandler.Image == null) return;
            // If the MouseUp event occurs, the user is not dragging.
            isDrag = false;
            if (_selectionStart != e.Location)
            {
                int width = e.X - _selectionStart.X;
                int height = e.Y - _selectionStart.Y;
                _selectionRect = new Rectangle(_selectionStart.X, _selectionStart.Y, width, height);
                imHandler.SetSelection(_selectionRect);
            }
            else imHandler.ResetSelection();
        }

        #endregion

        private void picturePanel_MouseClick(object sender, MouseEventArgs e)
        {
            //MessageBox.Show(e.X + " " + e.Y);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            imHandler.Reset();
        }
    }
}
