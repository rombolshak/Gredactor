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
            imHandler = ImageHandler.GetInstanse();
            imHandler.Changed += new EventHandler(imHandler_Changed);
            InitializeComponent();
            LoadPlugins();
        }

        void imHandler_Changed(object sender, EventArgs e)
        {
            pictureBox.Image = imHandler.Image;
            undoToolStripMenuItem.Enabled = imHandler.CanUndo;
            if (imHandler.CanUndo) SetChanged();
            else SetNotChanged();
        }

        private void LoadPlugins()
        {
            int x = 6, y = 19;
            foreach (IEffect effect in Gredactor.plugins)
            {
                foreach (Button btn in effect.Buttons)
                {
                    btn.Height = 25;
                    btn.Width = 87;
                    btn.Location = new Point(x, y);
                    btn.Tag = effect;
                    btn.Click += new EventHandler(effectButtonClick);
                    toolBox.Controls.Add(btn);
                    //if (x == 68) {x = 6; y += 31;}
                    //else x += 31;
                    y += 31;
                }
                if (effect.MenuItem != null)
                {
                    ToolStripMenuItem item = effect.MenuItem;
                    item.Tag = effect;
                    item.Click += new EventHandler(MenuItem_Click);
                    if ((effect.MenuGroup != "") && (effect.MenuGroup != null))
                        if (!menuStrip1.Items.ContainsKey(effect.MenuGroup))
                        {
                            ToolStripMenuItem newMenuGroup = new ToolStripMenuItem(effect.MenuGroup);
                            newMenuGroup.DropDownItems.Add(item);
                            menuStrip1.Items.Add(newMenuGroup);
                        }
                        else ((ToolStripMenuItem)menuStrip1.Items.Find(effect.MenuGroup, false)[0]).DropDownItems.Add(item);
                }
            }
        }

        void MenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                imHandler.ApplyEffect((IEffect)((ToolStripMenuItem)sender).Tag);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        void effectButtonClick(object sender, EventArgs e)
        {
            try
            {
                imHandler.ApplyEffect((IEffect)((Button)sender).Tag);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
            ofd.Filter = "Изображения|*.bmp;*.jpg;*.jpeg;*.png|Все файлы|*.*";
            ofd.Multiselect = false;
            ofd.Title = "Выберите файл";
            ofd.FileOk += new CancelEventHandler(FileSelected);
            ofd.ShowDialog();
        }

        void FileSelected(object sender, CancelEventArgs e)
        {
            string file = ((OpenFileDialog)sender).FileName;
            imHandler.Open(file);
            pictureBox.Image = imHandler.Image;
            pictureBox.Width = pictureBox.Image.Width;
            pictureBox.Height = pictureBox.Image.Height;
            saveAsToolStripMenuItem.Enabled = true;
            this.Text = "Gredactor [" + Path.GetFileName(file) + "] ";
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imHandler.Undo();
            undoToolStripMenuItem.Enabled = imHandler.CanUndo;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();
            if (svd.ShowDialog() == DialogResult.OK)
            {
                imHandler.SaveAs(svd.FileName);
                saveToolStripMenuItem.Enabled = false;
                this.Text = "Gredactor [" + Path.GetFileName(svd.FileName) + "] ";
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imHandler.Save();
            SetNotChanged();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


    }
}
