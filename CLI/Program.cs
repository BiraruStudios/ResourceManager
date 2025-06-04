using System.Diagnostics;
using System.Reflection;
using dnlib.DotNet;

namespace CLI
{
    public static class Program
    {
        private static readonly string[] validExtensions = [".exe", ".dll"];

        public static void Main(string[] args)
        {
            var exeName = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(exeName))
            {
                exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "");
            }
            
            if (args.Length != 2)
            {
                Console.WriteLine($"Usage: {exeName} <TargetAssembly> <SourceAssembly>");
                return;
            }

            string targetAssembly = Path.GetFullPath(args[0]);
            string sourceAssembly = Path.GetFullPath(args[1]);

            if (!File.Exists(targetAssembly))
            {
                Console.WriteLine("Error: Target file not found.");
                return;
            }
            else if (!File.Exists(sourceAssembly))
            {
                Console.WriteLine("Error: Source file not found.");
                return;
            }

            string targetExtension = Path.GetExtension(targetAssembly);
            string sourceExtension = Path.GetExtension(sourceAssembly);

            if (!IsValidExtension(targetExtension))
            {
                Console.WriteLine("Error: Target file must be a .dll or .exe.");
                return;
            }
            else if (!IsValidExtension(sourceExtension))
            {
                Console.WriteLine("Error: Source file must be a .dll or .exe.");
                return;
            }

            Console.WriteLine($"Target Assembly: {targetAssembly}");
            Console.WriteLine($"Source Assembly: {sourceAssembly}");

            string mergedOutputPath = Path.Combine(
                Path.GetDirectoryName(targetAssembly)!,
                Path.GetFileNameWithoutExtension(targetAssembly) + "_Merged" + targetExtension
            );

            ModuleDefMD targetModule = ModuleDefMD.Load(targetAssembly);
            ModuleDefMD sourceModule = ModuleDefMD.Load(sourceAssembly);

            EmbedResources(targetModule, sourceModule);
            RemoveAssemblyLinkedResources(targetModule, sourceModule);

            targetModule.Write(mergedOutputPath);
            Console.WriteLine($"Merged output saved to: {mergedOutputPath}");
        }

        private static bool IsValidExtension(string ext)
        {
            return Array.Exists(validExtensions, e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase));
        }

        private static void EmbedResources(ModuleDefMD target, ModuleDefMD source)
        {
            foreach (Resource resource in source.Resources)
            {
                if (resource is EmbeddedResource embedded)
                {
                    Console.WriteLine($"Embedding resource: {embedded.Name}");
                    byte[] data = embedded.CreateReader().ToArray();

                    target.Resources.Add(new EmbeddedResource(
                        embedded.Name,
                        data,
                        embedded.Attributes
                    ));
                }
            }
        }

        private static void RemoveAssemblyLinkedResources(ModuleDefMD target, ModuleDefMD source)
        {
            for (int i = target.Resources.Count - 1; i >= 0; i--)
            {
                if (target.Resources[i] is AssemblyLinkedResource linked &&
                    linked.Assembly.Name == source.Assembly.Name)
                {
                    Console.WriteLine($"Removing AssemblyLinkedResource: {linked.Name}");
                    target.Resources.RemoveAt(i);
                }
            }
        }
    }
}