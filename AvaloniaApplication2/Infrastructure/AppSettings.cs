namespace AvaloniaApplication2.Infrastructure;

public sealed class AppSettings
{
    public ConnectionStringsOptions connectionStrings { get; set; } = new();

    public DatabaseOptions database { get; set; } = new();

    public ApplicationOptions application { get; set; } = new();
}

public sealed class ConnectionStringsOptions
{
    public string defaultConnection { get; set; } = "Host=localhost;Port=5432;Database=matheDb;Username=postgres;Password=changeMe";
}

public sealed class DatabaseOptions
{
    public bool applyMigrationsOnStartup { get; set; } = true;

    public bool seedDemoData { get; set; } = true;
}

public sealed class ApplicationOptions
{
    public string name { get; set; } = "Матье";

    public int pageSize { get; set; } = 3;

    public string defaultSort { get; set; } = "alphabetAsc";
}
