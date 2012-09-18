using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Gredactor
{
    static class Gredactor
    {
        public static System.Collections.Generic.List<IEffect> plugins;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>                             
        [STAThread]
        static void Main()
        {
            try
            {
                FindPlugins();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void FindPlugins()
        {
            plugins = new System.Collections.Generic.List<IEffect>();
            foreach (string f in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll"))
            {
                Assembly a = Assembly.LoadFrom(f);
                foreach (Type t in a.GetTypes())
                {
                    foreach (Type i in t.GetInterfaces())
                    {
                        if (i.Equals(Type.GetType("Gredactor.IEffect")))
                        {
                            IEffect e = (IEffect)Activator.CreateInstance(t);
                            plugins.Add(e);
                            break;
                        }
                    }
                }
            }
        }
    }
}
