using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Gredactor
{
    public class Logger
    {
        public static void Log(string line)
        {
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                FileStream fs1 = new FileStream(path + "log.txt", FileMode.Append);
                long lenght = fs1.Length;
                fs1.Dispose();
                if (lenght >= 10 * 1024 * 1024)
                {
                    File.Move(path + "log.txt", path + "log_" + DateTime.Now.ToShortDateString() + "." + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + @".old");
                }
                FileStream fs2 = new FileStream(path + "log.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs2);
                sw.WriteLine("[" + DateTime.Now.ToString() + "]: " + line);
                sw.Close();
                fs2.Dispose();
            }
            catch
            {
            }
        }
    }
    static class Gredactor
    {
        public static System.Collections.Generic.List<IEffect> plugins;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>                             
        [STAThread]
        static void Main()
        {
            //GenImage();
            try
            {
                Logger.Log("Starting");
                Logger.Log("Version: " + Environment.Version.ToString());
                Logger.Log("OS: " + Environment.OSVersion.ToString());
                Logger.Log("Command: " + Environment.CommandLine.ToString());

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

        private static void GenImage()
        {
            System.Drawing.Bitmap original = new System.Drawing.Bitmap(512, 512);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, original.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);
            Random r = new Random();
            for (int i = 0; i < values.Length-2; i += 3)
            {
                values[i + 2] = (byte)r.Next(0, 128);
                values[i + 1] = (byte)r.Next(0, 255);
                values[i] = (byte)r.Next(128, 255);
            }

            System.Runtime.InteropServices.Marshal.Copy(values, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            original.Save("out.jpg");
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
                            Logger.Log("Found plugin \"" + e.Name + "\"");
                            break;
                        }
                    }
                }
            }
        }
    }
}
