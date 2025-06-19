using CLI.Common;

namespace CLI.Helpers;

public static class PathHelpers
{
    public static string ResolveOutputPath(string targetPath, string? userOutput)
    {
        var extension = Path.GetExtension(targetPath);

        if (!string.IsNullOrWhiteSpace(userOutput))
        {
            var fullPath = Path.GetFullPath(userOutput);
            return Utils.IsValidExtension(Path.GetExtension(fullPath))
                ? fullPath
                : Path.Combine(
                    fullPath,
                    Path.GetFileNameWithoutExtension(targetPath) + "_Merged" + extension
                );
        }

        return Path.Combine(
            Path.GetDirectoryName(targetPath)!,
            Path.GetFileNameWithoutExtension(targetPath) + "_Merged" + extension
        );
    }
}