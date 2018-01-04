using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Installer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Installing Program \"{{name}}\" on this computer. Press any key to continue...");
            Console.ReadKey(true);
            Console.WriteLine("Creating temporary folder...");
            var folder = Path.GetTempPath() + @"\SelfExtractingInstaller\{{name}}\" + Path.GetRandomFileName();
            Directory.CreateDirectory(folder);
            Environment.CurrentDirectory = folder;
            Console.WriteLine("Openning archive file...");
            using (var data = Assembly.GetExecutingAssembly().GetManifestResourceStream("files.dat"))
            using (var r = new BinaryReader(data))
            {
                Console.WriteLine("Creating subdirectories...");
                var c = r.ReadInt32();
                for (int i = 0; i < c; ++i)
                {
                    string fn = r.ReadString();
                    Console.WriteLine("Creating subfolder: \"{0}\" ...", fn);
                    Directory.CreateDirectory(fn);
                }
                Console.WriteLine("Extracting files...");
                c = r.ReadInt32();
                for (int i = 0; i < c; ++i)
                {
                    string fn = r.ReadString();
                    Console.WriteLine("Extracting file: \"{0}\" ...", fn);
                    using (var f = File.Create(fn))
                    {
                        var l = r.ReadInt32();
                        f.Write(r.ReadBytes(l), 0, l);
                    }
                }
            }
            Console.WriteLine("Running install command: \"{{cmd}}\" ...");
            var p = Process.Start("cmd", "/c {{cmd}}");
        }
    }
}
