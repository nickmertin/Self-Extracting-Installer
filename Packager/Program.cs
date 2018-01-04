//#define MSIL

using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Packager
{
    class Program
    {
        static void Main(string[] args)
        {
            // Info
            Console.WriteLine("Self-Extracting Installer Packager\nCopyright (c) 2015 Nicholas Mertin");
        // Folder selection prompt
        prompt_folder:
            Console.Write("Enter path of folder: ");
            string fn = Console.ReadLine();
            if (!Directory.Exists(fn))
            {
                Console.WriteLine("Folder could not be found.");
                goto prompt_folder;
            }
            var folder = new DirectoryInfo(fn);
        // Application name prompt
        prompt_name:
            Console.Write("Enter desired application name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name) || name.Contains('\a'))
            {
                Console.WriteLine("The given name is not acceptable.");
                goto prompt_name;
            }
        // Install command-line prompt
        prompt_cmd:
            Console.Write("Enter command to run in extracted folder to install program: ");
            string cmd = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(cmd))
            {
                Console.WriteLine("The given command is not acceptable.");
                goto prompt_cmd;
            }
            // File list
            Console.WriteLine("Listing files and subfolders found in \"{0}\":", fn);
            var files = new List<Tuple<string, FileInfo>>();
            var folders = new List<string>();
            addFiles(folder, files, folders, 1, "");
            // Temp folder
            var temp = Directory.CreateDirectory(Path.GetTempPath() + "\\" + Path.GetRandomFileName());
            Console.WriteLine("Created temporary folder \"{0}\"", temp.FullName);
            Environment.CurrentDirectory = temp.FullName;
            // File archive
            using (var fs = File.Create("files.dat"))
            using (var w = new BinaryWriter(fs))
            {
                Console.WriteLine("Created temporary archive file \"{0}\"", temp.FullName + "\\files.dat");
                w.Write(folders.Count);
                foreach (var f in folders)
                {
                    Console.WriteLine("Adding folder: \"{0}\" ...", Path.GetFullPath(f));
                    w.Write(f);
                }
                w.Write(files.Count);
                foreach (var f in files)
                {
                    Console.WriteLine("Adding file: \"{0}\" ...", f.Item1);
                    w.Write(f.Item1);
                    byte[] b = File.ReadAllBytes(f.Item2.FullName);
                    w.Write(b.Length);
                    w.Write(b);
                }
            }
#if MSIL
            // Base assembly
            Console.WriteLine("Creating overhead objects...");
            var buildName = new AssemblyName("SelfExtractingInstaller");
            var build = AppDomain.CurrentDomain.DefineDynamicAssembly(buildName, AssemblyBuilderAccess.Save, temp.FullName);
            var fileName = name + "_install.exe";
            var module = build.DefineDynamicModule(buildName.Name, fileName);
            var type = module.DefineType("Program", TypeAttributes.Public);
            // Function
            Console.WriteLine("Generating MSIL for entry point...");
            var func = type.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void), new Type[] { typeof(string[]) });
            var func_il = func.GetILGenerator();
            var loop = func_il.DefineLabel();
            var end = func_il.DefineLabel();
            // Locals
            func_il.DeclareLocal(typeof(FileStream));
            func_il.DeclareLocal(typeof(BinaryReader));
            func_il.DeclareLocal(typeof(int));
            func_il.DeclareLocal(typeof(string));
            func_il.DeclareLocal(typeof(FileStream));
            func_il.DeclareLocal(typeof(byte[]));
            // Confirmation
            func_il.Emit(OpCodes.Ldstr, string.Format("Installing application \"{0}\" on this computer. Press any key to continue...", name));
            func_il.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(string) }));
            func_il.Emit(OpCodes.Ldc_I4_1);
            func_il.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadKey", new Type[] { typeof(bool) }));
            func_il.Emit(OpCodes.Pop);
            // Temp folder
            func_il.EmitWriteLine("Creating temporary folder...");
            func_il.Emit(OpCodes.Call, typeof(Path).GetMethod("GetTempPath"));
            func_il.Emit(OpCodes.Ldstr, "\\SelfExtractingInstaller\\");
            func_il.Emit(OpCodes.Call, typeof(Guid).GetMethod("NewGuid"));
            func_il.Emit(OpCodes.Constrained);
            func_il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            func_il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
            func_il.Emit(OpCodes.Dup);
            func_il.Emit(OpCodes.Call, typeof(Directory).GetMethod("CreateDirectory", new Type[] { typeof(string) }));
            func_il.Emit(OpCodes.Pop);
            func_il.Emit(OpCodes.Call, typeof(Environment).GetProperty("CurrentDirectory").GetSetMethod());
            // File
            func_il.EmitWriteLine("Openning archive resource file...");
            func_il.Emit(OpCodes.Call, typeof(Assembly).GetMethod("GetExecutingAssembly"));
            func_il.Emit(OpCodes.Ldstr, "files.dat");
            func_il.Emit(OpCodes.Callvirt, typeof(Assembly).GetMethod("GetFile"));
            func_il.Emit(OpCodes.Dup);
            func_il.Emit(OpCodes.Stloc_0);
            func_il.Emit(OpCodes.Newobj, typeof(BinaryReader).GetConstructor(new Type[] { typeof(Stream) }));
            func_il.Emit(OpCodes.Dup);
            func_il.Emit(OpCodes.Stloc_1);
            func_il.Emit(OpCodes.Callvirt, typeof(BinaryReader).GetMethod("ReadInt32"));
            // Loop
            func_il.EmitWriteLine("Extracting files...");
            func_il.MarkLabel(loop);
            func_il.Emit(OpCodes.Brfalse, end);
            func_il.Emit(OpCodes.Stloc_2);
            func_il.Emit(OpCodes.Ldloc_1);
            func_il.Emit(OpCodes.Callvirt, typeof(BinaryReader).GetMethod("ReadString"));
            func_il.Emit(OpCodes.Stloc_3);
            func_il.Emit(OpCodes.Ldstr, "Extracting: \"");
            func_il.Emit(OpCodes.Ldloc_3);
            func_il.Emit(OpCodes.Ldstr, "\" ...");
            func_il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) }));
            func_il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            func_il.Emit(OpCodes.Ldloc_3);
            func_il.Emit(OpCodes.Call, typeof(File).GetMethod("Create", new Type[] { typeof(string) }));
            func_il.Emit(OpCodes.Stloc, (short)4);
            func_il.Emit(OpCodes.Ldloc_1);
            func_il.Emit(OpCodes.Dup);
            func_il.Emit(OpCodes.Call, typeof(BinaryReader).GetMethod("ReadInt32"));
            func_il.Emit(OpCodes.Call, typeof(BinaryReader).GetMethod("ReadBytes"));
            func_il.Emit(OpCodes.Stloc, (short)5);
            func_il.Emit(OpCodes.Ldloc, (short)4);
            func_il.Emit(OpCodes.Ldloc, (short)5);
            func_il.Emit(OpCodes.Ldc_I4_0);
            func_il.Emit(OpCodes.Ldloc, (short)5);
            func_il.Emit(OpCodes.Callvirt, typeof(Array).GetProperty("Length").GetGetMethod());
            func_il.Emit(OpCodes.Callvirt, typeof(FileStream).GetMethod("Write"));
            func_il.Emit(OpCodes.Ldloc_2);
            func_il.Emit(OpCodes.Jmp, loop);
            func_il.MarkLabel(end);
            // Command
            func_il.EmitWriteLine("Running command...");
            func_il.Emit(OpCodes.Ldstr, cmd);
            func_il.Emit(OpCodes.Call, typeof(Process).GetMethod("Start", new Type[] { typeof(string) }));
            func_il.Emit(OpCodes.Callvirt, typeof(Process).GetMethod("WaitForExit", Type.EmptyTypes));
            func_il.Emit(OpCodes.Ldstr, "Done! Press any key to continue...");
            func_il.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(string) }));
            func_il.Emit(OpCodes.Ldc_I4_1);
            func_il.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadKey", new Type[] { typeof(bool) }));
            func_il.Emit(OpCodes.Pop);
            func_il.Emit(OpCodes.Ret);
            Type t = type.CreateType();
            build.SetEntryPoint(func);
            build.AddResourceFile("files.dat", "files.dat");
            build.Save(fileName);
#else
            var provider = new CSharpCodeProvider();
            var cp = new CompilerParameters(new[] { "System.dll" }) { GenerateExecutable = true, OutputAssembly = name + "_install.exe", GenerateInMemory = false };
            cp.EmbeddedResources.Add("files.dat");
            cp.MainClass = "Installer.Program";
            string source;
            using (var src = Assembly.GetExecutingAssembly().GetManifestResourceStream("Packager.Installer.cs"))
            using (var r = new StreamReader(src))
                source = r.ReadToEnd().Replace("{{name}}", name).Replace("{{cmd}}", cmd);
            var cr = provider.CompileAssemblyFromSource(cp, source);
            Process.Start("explorer", ".");
#endif
        }
        static void addFiles(DirectoryInfo info, List<Tuple<string, FileInfo>> list, List<string> foldersList, int nestLevel, string baseName)
        {
            string prefix = "";
            for (int i = 0; i < nestLevel; ++i)
                prefix += "|";
            prefix += "-";
            foreach (var file in info.EnumerateFiles())
            {
                Console.WriteLine("{0}File: {1}", prefix, file.FullName);
                list.Add(new Tuple<string, FileInfo>(baseName + file.Name, file));
            }
            foreach (var folder in info.EnumerateDirectories())
            {
                Console.WriteLine("{0}Folder: {1}", prefix, folder.FullName);
                foldersList.Add(baseName + folder.Name);
                addFiles(folder, list, foldersList, nestLevel + 1, baseName + folder.Name + "\\");
            }
        }
    }
}