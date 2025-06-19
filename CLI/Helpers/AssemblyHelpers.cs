using dnlib.DotNet;

namespace CLI.Helpers;

public static class AssemblyHelpers
{
    public static ModuleDefMD LoadAssembly(string path)
    {
        return ModuleDefMD.Load(path, new ModuleCreationOptions
        {
            TryToLoadPdbFromDisk = true
        });
    }
}