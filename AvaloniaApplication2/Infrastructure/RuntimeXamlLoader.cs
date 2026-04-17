using System.Reflection;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication2.Infrastructure;

public static class RuntimeXamlLoader
{
    public static void Load(object target, string relativePath)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var assembly = target.GetType().Assembly;
        var normalizedPath = relativePath.Replace('\\', '/');

        var baseDirectory = AppContext.BaseDirectory;
        var absolutePath = Path.Combine(baseDirectory, normalizedPath);

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException(
                $"XAML file was not found at runtime: {absolutePath}",
                absolutePath);
        }

        var xaml = File.ReadAllText(absolutePath);
        AvaloniaRuntimeXamlLoader.Load(
            xaml,
            assembly,
            target,
            new Uri($"avares://{assembly.GetName().Name}/{normalizedPath}"),
            designMode: false);
    }
}
