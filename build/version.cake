#tool "dotnet:?package=minver-cli&version=2.4.0"
#addin "nuget:?package=Cake.MinVer&version=1.0.0"

var fallbackVersion = Argument<string>("force-version", EnvironmentVariable("FALLBACK_VERSION") ?? "0.1.0");
 
string BuildVersion(string fallbackVersion) {
    var PackageVersion = string.Empty;
    try {
        Information("Attempting MinVer...");
        var settings = new MinVerSettings()
        {
            DefaultPreReleasePhase = "preview",
            TagPrefix = "v"
        };
        var version = MinVer(settings);
        PackageVersion = version.PackageVersion;
    } catch (Exception ex) {
        Warning($"Error when getting version {ex.Message}");
        Information($"Falling back to version: {fallbackVersion}");
        PackageVersion = fallbackVersion;
    } finally {
        Information($"Building for version '{PackageVersion}'");
    }
    return PackageVersion;
}