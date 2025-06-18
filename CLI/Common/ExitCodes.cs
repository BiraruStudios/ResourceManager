namespace CLI.Common;

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
    
    // Resource Based Errors
    NoResourceToEmbed = 6,
    NoResources = 7,
    ResourceParseFailed = 8,
}