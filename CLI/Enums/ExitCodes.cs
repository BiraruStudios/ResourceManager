namespace CLI.Enums;

public enum ExitCodes
{
    // Generic Errors
    Success = 0,
    FatalError = 1,

    // File Based Errors
    TargetMissing = 2,
    SourceMissing = 3,
    TargetExtensionInvalid = 4,
    SourceExtensionInvalid = 5,
    OutputDirectoryInvalid = 9,
    OutputExtensionInvalid = 10,
    TargetEqualsSource = 11,
    OutputEqualsTarget = 12,
    OutputEqualsSource = 13,
    OutputPermissionDenied = 14,
    TargetInvalidDotNetAssembly = 15,
    SourceInvalidDotNetAssembly = 16,

    // Resource Based Errors
    NoResourceToEmbed = 6,
    NoResources = 7,
    ResourceParseFailed = 8
}