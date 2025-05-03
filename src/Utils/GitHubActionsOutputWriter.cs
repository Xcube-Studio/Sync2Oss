namespace Sync2Oss.Utils;

public static class GitHubActionsOutputWriter
{
    public static void WriteOutput(string key, string value)
    {
        var outputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");

        if (string.IsNullOrWhiteSpace(outputFile))
        {
            return;
        }

        var outputDirectory = Path.GetDirectoryName(outputFile);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.AppendAllText(outputFile, $"{key}={value}{Environment.NewLine}");
    }
}
