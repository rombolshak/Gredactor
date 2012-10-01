using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FileMover
{
    class FileMover
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                System.Threading.Thread.Sleep(1000);
                try
                {
                    if (File.Exists(args[i])) File.Delete(args[i]);
                    File.Move(args[i] + ".tmp", args[i]);
                }
                catch {}
            }
        }
    }
}
