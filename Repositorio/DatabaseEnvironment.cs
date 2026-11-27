using System;

namespace Repositorio;

public static class DatabaseEnvironment
{
    public static void Load()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DATABASE_URL")))
            return;

        var directories = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(AppContext.BaseDirectory)
        };

        foreach (var start in directories)
        {
            for (var directory = start; directory is not null; directory = directory.Parent)
            {
                var path = Path.Combine(directory.FullName, ".env");
                if (!File.Exists(path))
                    continue;

                foreach (var rawLine in File.ReadAllLines(path))
                {
                    var line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;

                    var separator = line.IndexOf('=');
                    if (separator <= 0)
                        continue;

                    var key = line[..separator].Trim();
                    var value = line[(separator + 1)..].Trim();
                    if (value.Length >= 2 &&
                        ((value[0] == '"' && value[^1] == '"') ||
                         (value[0] == (char)39 && value[^1] == (char)39)))
                        value = value[1..^1];

                    if (key == "DATABASE_URL" &&
                        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
                        Environment.SetEnvironmentVariable(key, value);
                }

                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DATABASE_URL")))
                    return;
            }
        }
    }
}
