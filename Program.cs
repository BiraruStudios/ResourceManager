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
            Console.WriteLine("Usage: ResourceMerger.exe <TargetAssembly> <SourceAssembly>");
            return;
        }

        string targetAssemblyPath = args[0];
        string sourceAssemblyPath = args[1];

        if (!File.Exists(targetAssemblyPath) || !File.Exists(sourceAssemblyPath))
        {
            Console.WriteLine("Error: One or both files do not exist.");
            return;
        }

        Console.WriteLine($"Target Assembly: {Path.GetFileName(targetAssemblyPath)}");
        Console.WriteLine($"Source Assembly: {Path.GetFileName(sourceAssemblyPath)}");

        string mergedAssemblyOutputPath = Path.Combine(
            Path.GetDirectoryName(targetAssemblyPath),
            Path.GetFileNameWithoutExtension(targetAssemblyPath) + "_Merged" + Path.GetExtension(targetAssemblyPath)
        );

        ModuleDefMD targetModule = ModuleDefMD.Load(targetAssemblyPath);
        ModuleDefMD sourceModule = ModuleDefMD.Load(sourceAssemblyPath);

        foreach (var resource in sourceModule.Resources)
        {
            if (resource is EmbeddedResource embeddedResource)
            {
                Console.WriteLine($"Embedding resource: {embeddedResource.Name}");
                byte[] resourceData = embeddedResource.CreateReader().ToArray();
                targetModule.Resources.Add(new EmbeddedResource(
                    embeddedResource.Name,
                    resourceData,
                    embeddedResource.Attributes));
            }
        }

        for (int i = targetModule.Resources.Count - 1; i >= 0; i--)
        {
            if (targetModule.Resources[i] is AssemblyLinkedResource linkedResource &&
                linkedResource.Assembly.Name == sourceModule.Assembly.Name)
            {
                Console.WriteLine($"Removing AssemblyLinkedResource: {linkedResource.Name}");
                targetModule.Resources.RemoveAt(i);
            }
        }

        targetModule.Write(mergedAssemblyOutputPath);
        Console.WriteLine("Merged output saved to: " + mergedAssemblyOutputPath);
    }
}