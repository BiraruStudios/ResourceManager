using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Resources;

class ResourceMerger
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: ResourceMerger.exe <ExePath> <DllPath>");
            return;
        }

        string exePath = args[0];
        string dllPath = args[1];

        if (!File.Exists(exePath) || !File.Exists(dllPath))
        {
            Console.WriteLine("Error: One or both files do not exist.");
            return;
        }

        string outputPath = Path.Combine(
            Path.GetDirectoryName(exePath),
            Path.GetFileNameWithoutExtension(exePath) + "_Merged.exe"
        );

        ModuleDefMD exe = ModuleDefMD.Load(exePath);
        ModuleDefMD dll = ModuleDefMD.Load(dllPath);
        
        foreach (var res in dll.Resources)
        {
            if (res is EmbeddedResource embedded)
            {
                Console.WriteLine($"Embedding resource: {embedded.Name}");
                var data = embedded.CreateReader().ToArray();
                exe.Resources.Add(new EmbeddedResource(embedded.Name, data, embedded.Attributes));
            }
        }
        
        for (int i = exe.Resources.Count - 1; i >= 0; i--)
        {
            if (exe.Resources[i] is AssemblyLinkedResource alr &&
                alr.Assembly.Name == dll.Assembly.Name)
            {
                Console.WriteLine($"Removing AssemblyLinkedResource: {alr.Name}");
                exe.Resources.RemoveAt(i);
            }
        }

        exe.Write(outputPath);
        Console.WriteLine("Merged EXE saved to: " + outputPath);
    }
}