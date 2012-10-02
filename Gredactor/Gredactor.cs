﻿using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Drawing;      

namespace Gredactor
{
    public class Logger
    {
        public static void Log(string line)
        {
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                FileStream fs1 = new FileStream(path + "\\application.log", FileMode.Append);
                long lenght = fs1.Length;
                fs1.Dispose();
                if (lenght >= 10 * 1024 * 1024)
                {
                    File.Move(path + "\\application.log", path + "log_" + DateTime.Now.ToShortDateString() + "." + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + @".old");
                }
                FileStream fs2 = new FileStream(path + "\\application.log", FileMode.Append);
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
    
    class Gredactor
    {

        public static System.Collections.Generic.List<IEffect> plugins;
        public static System.Collections.Generic.List<string> filesToSave;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [STAThread]
        static void RunForm()
        {
            filesToSave = new System.Collections.Generic.List<string>();
            IntPtr hWnd = FindWindow(null, "Gredactor");
            if (hWnd != IntPtr.Zero)
                ShowWindow(hWnd, 0); // 0 = SW_HIDE           
            Application.Run(new MainForm());
            ShowWindow(hWnd, 1);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>                             
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Console.Title = "Gredactor";
                Logger.Log("Starting");
                Logger.Log("Version: " + Environment.Version.ToString());
                Logger.Log("OS: " + Environment.OSVersion.ToString());
                Logger.Log("Command: " + Environment.CommandLine.ToString());

                FindPlugins();

                string[] args = Environment.GetCommandLineArgs();

                if (args.Length == 1)                                    
                    RunForm();                
                else
                {
                    if (args.Length < 4)
                        PrintUsage();

                    string input = args[args.Length - 2];
                    string output = args[args.Length - 1];

                    if (!File.Exists(input))
                    {
                        Console.WriteLine("ERROR\t\tВходной файл {0} не найден", input);
                        //Console.WriteLine("Нажмите любую клавишу для выхода");
                        //Console.ReadKey(true);
                        Environment.Exit(0);
                    }

                    System.Collections.Generic.Dictionary<char, IEffect> shortKeys = new System.Collections.Generic.Dictionary<char, IEffect>(plugins.Count);
                    System.Collections.Generic.Dictionary<string, IEffect> longKeys = new System.Collections.Generic.Dictionary<string, IEffect>(plugins.Count);
                    foreach (IEffect e in plugins)
                    {
                        if (!shortKeys.ContainsKey(e.ShortConsoleKey))
                            shortKeys.Add(e.ShortConsoleKey, e);
                        if (!longKeys.ContainsKey(e.LongConsoleKey))
                            longKeys.Add(e.LongConsoleKey, e);
                    }

                    System.Collections.Queue effects = new System.Collections.Queue();

                    int i = 1;
                    while (i < args.Length - 2)
                        if (args[i][0] == '-')
                            if (args[i][1] == '-')                            
                                i = ExtractEffect(args, longKeys, effects, i);                            
                            else                            
                                i = ExtractEffect(args, shortKeys, effects, i);                            
                        else PrintUsage();


                    DateTime timeStart = DateTime.Now;
                    Console.Write("{0,-" + (Console.WindowWidth - 6) + "}", "Загружаем изображение");
                    Bitmap image = new Bitmap(input);
                    Console.WriteLine("{0," + 6 + "}", "[DONE]");

                    while (effects.Count > 0)
                    {
                        IEffect effect = (IEffect)effects.Dequeue();
                        Console.Write("{0,-" + (Console.WindowWidth-28) + "}", "Применяем " + effect.Name);
                        try
                        {
                            DateTime start = DateTime.Now;
                            image = effect.Apply((Bitmap)image.Clone());
                            TimeSpan sp = DateTime.Now - start; 
                            Console.WriteLine("{0," + 28 + "}", "[DONE (за " + sp.TotalSeconds + " сек.)]");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("{0," + 6 + "}", "[FAIL]");
                            Console.WriteLine(ex.Message);
                            //Console.ReadKey(true);
                            Environment.Exit(1);
                        }
                    }
                    Console.Write("{0,-" + (Console.WindowWidth - 6) + "}", "Сохраняем изображение");
                    image.Save(output);
                    Console.WriteLine("{0," + 6 + "}", "[DONE]");
                    TimeSpan span = DateTime.Now - timeStart;
                    Console.WriteLine("Выполнено за {0} минут, {1} секунд", span.Minutes, span.Seconds);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message + "\n=====================" + Environment.NewLine + "Stack:" + ex.StackTrace + Environment.NewLine + "====================");
                System.Windows.Forms.MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (filesToSave != null)
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("FileMover.exe");
                //psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                psi.Arguments = String.Join(" ", filesToSave.ToArray());
                System.Diagnostics.Process.Start(psi);
            }
        }

        private static int ExtractEffect(string[] args, System.Collections.Generic.Dictionary<char, IEffect> shortKeys, System.Collections.Queue effects, int i)
        {
            if (args[i].Length != 2) PrintUsage();            
            try
            {
                IEffect eff = shortKeys[args[i][1]];

                if (i + 1 != args.Length - 2)
                {
                    if (args[i + 1][0] != '-')
                        // параметр
                        if (eff.Prepare(args[i++ + 1], true))
                            effects.Enqueue(eff);
                        else
                            EffectFailed(eff.Name);
                    else
                        if (eff.Prepare(null, true))
                            effects.Enqueue(eff);
                        else
                            EffectFailed(eff.Name);
                }
                else
                {
                    if (eff.Prepare(null, true))
                        effects.Enqueue(eff);
                    else
                        EffectFailed(eff.Name);
                }
                ++i;
            }
            catch { PrintUsage(); }
            return i;
        }

        private static int ExtractEffect(string[] args, System.Collections.Generic.Dictionary<string, IEffect> longKeys, System.Collections.Queue effects, int i)
        {
            try
            {
                IEffect eff = longKeys[args[i].Substring(2)];
                if (i + 1 != args.Length - 2)
                {
                    if (args[i + 1][0] != '-')
                        // параметр
                        if (eff.Prepare(args[i++ + 1], true))
                            effects.Enqueue(eff);
                        else
                            EffectFailed(eff.Name);
                    else
                        if (eff.Prepare(null, true))
                            effects.Enqueue(eff);
                        else
                            EffectFailed(eff.Name);
                }
                else
                {
                    if (eff.Prepare(null, true))
                        effects.Enqueue(eff);
                    else
                        EffectFailed(eff.Name);
                }
                ++i;
            }
            catch { PrintUsage(); }
            return i;
        }

        private static void EffectFailed(string name)
        {
            Console.WriteLine("ERROR:\t Сбой модуля {0}", name);
            //Console.ReadKey(true);
            Environment.Exit(0);
        }

        private static void PrintUsage()
        {
            string[] args = Environment.GetCommandLineArgs();
            string nLine = Environment.NewLine;
            string usage = "Usage: ";
            usage += Path.GetFileName(args[0]);
            usage += " [options] <input> <output>" + nLine;
            Console.WriteLine(usage);
            Console.WriteLine("Options могут быть: ");
            foreach (IEffect e in plugins)
                Console.WriteLine("-{0}, --{1} {2,-15}\t{4}",
                    e.ShortConsoleKey,
                    e.LongConsoleKey,
                    e.ConsoleParams,
                    " ",
                    e.Description);
            //Console.ReadKey(true);
            Environment.Exit(0);
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
